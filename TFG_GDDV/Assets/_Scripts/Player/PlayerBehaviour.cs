using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using Unity.Netcode;

public class PlayerBehaviour : NetworkBehaviour
{
    public float moveSpeed = 5f;                // Velocidad de movimiento del jugador
    public float sensitivity = 0.3f;            // Sensibilidad del ratón para mirar alrededor
    public float smoothTime = 0.05f;            // Controla la suavidad de la cámara

    private Vector3 direction = Vector3.zero;   // Dirección en la que se mueve el personaje
    private float targetRotationY = 0f;         // Rotación deseada en el eje Y
    private float targetRotationX = 0f;         // Rotación deseada en el eje X
    private float currentRotationY = 0f;        // Rotación actual en el eje Y
    private float currentRotationX = 0f;        // Rotación actual en el eje X

    private float rotationVelocityY = 0f;       // Velocidad de interpolación en el eje Y para suavizado
    private float rotationVelocityX = 0f;       // Velocidad de interpolación en el eje X para suavizado

    public CinemachineCamera cam;               // Referencia a la cámara del jugador
    private CharacterController controller;     // Referencia al CharacterController para el movimiento

    public override void OnNetworkSpawn()
    {
        cam = GetComponentInChildren<CinemachineCamera>();   
        controller = GetComponent<CharacterController>();     

        if (!IsOwner)
        {
            cam.enabled = false;                            // Se deshabilita la cámara para otros jugadores
            GetComponent<PlayerInput>().enabled = false;    
        }
    }

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;    // Se bloquea el cursor en el centro de la pantalla
        Cursor.visible = false;                      // Se oculta el cursor
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;  
            Cursor.visible = true;                   
        }

        if (IsOwner)
        {
            HandleMovement();   // Se gestiona el movimiento del personaje
            HandleRotation();   // Se gestiona la rotación del personaje y la cámara
        }
    }

    private void HandleMovement()
    {
        Vector3 move = (transform.forward * direction.z + transform.right * direction.x) * moveSpeed * Time.deltaTime; // Se calcula la dirección del movimiento
        controller.Move(move);  // Se aplica el movimiento al CharacterController
    }

    private void HandleRotation()
    {
        currentRotationY = Mathf.SmoothDamp(currentRotationY, targetRotationY, ref rotationVelocityY, smoothTime); // Se suaviza la rotación en el eje Y
        transform.rotation = Quaternion.Euler(0f, currentRotationY, 0f);  // Se aplica la rotación en Y

        targetRotationX = Mathf.Clamp(targetRotationX, -80f, 80f);  // Se limita la inclinación de la cámara en X
        currentRotationX = Mathf.SmoothDamp(currentRotationX, targetRotationX, ref rotationVelocityX, smoothTime); // Se suaviza la rotación en X
        cam.transform.localRotation = Quaternion.Euler(currentRotationX, 0f, 0f);  // Se aplica la rotación en X a la cámara
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;  // Se asegura de que solo el propietario del personaje pueda moverse
        Vector2 input = context.ReadValue<Vector2>();  // Se obtiene la entrada de movimiento
        direction = new Vector3(input.x, 0, input.y);  // Se almacena la dirección de movimiento
    }

    public void Look(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;  // Se asegura de que solo el propietario controle la cámara
        Vector2 input = context.ReadValue<Vector2>();  // Se obtiene la entrada del ratón o joystick
        targetRotationY += input.x * sensitivity;  // Se actualiza la rotación en el eje Y
        targetRotationX -= input.y * sensitivity;  // Se actualiza la inclinación en el eje X
    }
}