using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using Unity.Netcode;

public class PlayerBehaviour : NetworkBehaviour
{
    public float moveSpeed = 5f;                // Velocidad de movimiento del jugador
    public float sensitivity = 0.3f;            // Sensibilidad del rat�n para mirar alrededor
    public float smoothTime = 0.05f;            // Controla la suavidad de la c�mara

    private Vector3 direction = Vector3.zero;   // Direcci�n en la que se mueve el personaje
    private float targetRotationY = 0f;         // Rotaci�n deseada en el eje Y
    private float targetRotationX = 0f;         // Rotaci�n deseada en el eje X
    private float currentRotationY = 0f;        // Rotaci�n actual en el eje Y
    private float currentRotationX = 0f;        // Rotaci�n actual en el eje X

    private float rotationVelocityY = 0f;       // Velocidad de interpolaci�n en el eje Y para suavizado
    private float rotationVelocityX = 0f;       // Velocidad de interpolaci�n en el eje X para suavizado

    public CinemachineCamera cam;               // Referencia a la c�mara del jugador
    private CharacterController controller;     // Referencia al CharacterController para el movimiento

    public override void OnNetworkSpawn()
    {
        cam = GetComponentInChildren<CinemachineCamera>();   
        controller = GetComponent<CharacterController>();     

        if (!IsOwner)
        {
            cam.enabled = false;                            // Se deshabilita la c�mara para otros jugadores
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
            HandleRotation();   // Se gestiona la rotaci�n del personaje y la c�mara
        }
    }

    private void HandleMovement()
    {
        Vector3 move = (transform.forward * direction.z + transform.right * direction.x) * moveSpeed * Time.deltaTime; // Se calcula la direcci�n del movimiento
        controller.Move(move);  // Se aplica el movimiento al CharacterController
    }

    private void HandleRotation()
    {
        currentRotationY = Mathf.SmoothDamp(currentRotationY, targetRotationY, ref rotationVelocityY, smoothTime); // Se suaviza la rotaci�n en el eje Y
        transform.rotation = Quaternion.Euler(0f, currentRotationY, 0f);  // Se aplica la rotaci�n en Y

        targetRotationX = Mathf.Clamp(targetRotationX, -80f, 80f);  // Se limita la inclinaci�n de la c�mara en X
        currentRotationX = Mathf.SmoothDamp(currentRotationX, targetRotationX, ref rotationVelocityX, smoothTime); // Se suaviza la rotaci�n en X
        cam.transform.localRotation = Quaternion.Euler(currentRotationX, 0f, 0f);  // Se aplica la rotaci�n en X a la c�mara
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;  // Se asegura de que solo el propietario del personaje pueda moverse
        Vector2 input = context.ReadValue<Vector2>();  // Se obtiene la entrada de movimiento
        direction = new Vector3(input.x, 0, input.y);  // Se almacena la direcci�n de movimiento
    }

    public void Look(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;  // Se asegura de que solo el propietario controle la c�mara
        Vector2 input = context.ReadValue<Vector2>();  // Se obtiene la entrada del rat�n o joystick
        targetRotationY += input.x * sensitivity;  // Se actualiza la rotaci�n en el eje Y
        targetRotationX -= input.y * sensitivity;  // Se actualiza la inclinaci�n en el eje X
    }
}