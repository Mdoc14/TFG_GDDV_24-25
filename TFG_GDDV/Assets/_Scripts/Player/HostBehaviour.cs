using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.VisualScripting;

public class HostBehaviour : NetworkBehaviour
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

    public Camera cam;               // Referencia a la cámara del jugador
    private CharacterController controller;     // Referencia al CharacterController para el movimiento
    public Animator animator;                  // Referencia al Animator para animaciones

    private bool isInteracting = false;         // Flag para controlar la interacción
    private IInteractable currentInteractable;  // Referencia al objeto interactuable actual
    private GameObject interactKeyHint;         // Referencia al objeto de la UI de la tecla de interacción
    private RectTransform crosshairRectTransform;             // Referencia al objeto de la UI del crosshair
    public RectTransform customizerUIRectTransform;             // Referencia al objeto de la UI de customización de avatar
    public RectTransform basicGuideUIRectTransform;             // Referencia al objeto de la UI de guía básica
    private Image crosshairUI;             // Referencia al objeto de la UI del crosshair
    Color crosshairColor;  // Obtén el color actual del crosshair

    public override void OnNetworkSpawn()
    {

        controller = GetComponent<CharacterController>();

    }

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;    // Se bloquea el cursor en el centro de la pantalla
        Cursor.visible = false;                      // Se oculta el cursor
        interactKeyHint = transform.Find("PlayerUI/InteractButton").gameObject;
        crosshairRectTransform = transform.Find("PlayerUI/Crosshair").gameObject.GetComponent<RectTransform>();
        crosshairUI = transform.Find("PlayerUI/Crosshair").gameObject.GetComponent<Image>();
        crosshairColor = crosshairUI.color;
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }


        HandleMovement();   // Se gestiona el movimiento del personaje
        HandleRotation();   // Se gestiona la rotación del personaje y la cámara
        if (CheckForInteractable())
        {
            interactKeyHint.SetActive(true);  // Se activa el botón de interacción si se encuentra un objeto interactuable
            crosshairColor.a = 0.8f;                   // Modifica la propiedad 'a' (alfa)
            crosshairUI.color = crosshairColor;   // Se aplica el nuevo color al crosshair
            crosshairRectTransform.localScale = new Vector3(1.5f, 1.5f, 1.5f); // Se aumenta el tamaño del crosshair
            if (isInteracting && currentInteractable != null)
            {
                currentInteractable.Interact();  // Se llama al método Interact del objeto interactuable
                isInteracting = false;  // Se resetea el flag de interacción
            }
        }
        else
        {
            interactKeyHint.SetActive(false);  // Se desactiva el botón de interacción si no se encuentra un objeto interactuable
            transform.Find("PlayerUI/InteractButton").gameObject.SetActive(false);
            crosshairColor.a = 0.4f;                   // Modifica la propiedad 'a' (alfa)
            crosshairUI.color = crosshairColor;   // Se aplica el nuevo color al crosshair
            crosshairRectTransform.localScale = new Vector3(1f, 1f, 1f); // Se resetea el tamaño del crosshair
            currentInteractable = null;  // Se resetea el objeto interactuable si no hay ninguno en el campo de visión
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

    private bool CheckForInteractable()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 3f))
        {
            Debug.DrawRay(ray.origin, ray.direction * 3f, Color.red);  // Se dibuja un rayo en la escena para depuración
            if (hit.collider.CompareTag("interactable"))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    currentInteractable = interactable;
                    return true;  // Se encuentra un objeto interactuable
                }
            }
        }
        return false;
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;  // Se asegura de que solo el propietario del personaje pueda moverse
        Vector2 input = context.ReadValue<Vector2>();  // Se obtiene la entrada de movimiento
        direction = new Vector3(input.x, 0, input.y);  // Se almacena la dirección de movimiento

        animator.SetFloat("moveX", input.x);
        animator.SetFloat("moveY", input.y);


        if (input.x != 0 || input.y != 0)
        {
            animator.SetBool("isMoving", true);  // Se activa la animación de movimiento
        }
        else
        {
            animator.SetBool("isMoving", false);  // Se desactiva la animación de movimiento
        }
    }

    public void Look(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;  // Se asegura de que solo el propietario controle la cámara
        Vector2 input = context.ReadValue<Vector2>();  // Se obtiene la entrada del ratón o joystick
        targetRotationY += input.x * sensitivity;  // Se actualiza la rotación en el eje Y
        targetRotationX -= input.y * sensitivity;  // Se actualiza la inclinación en el eje X
    }

    public void Interact(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;  // Se asegura de que solo el propietario interactúe
        bool input = context.ReadValueAsButton();  // Se obtiene la entrada del teclado
        isInteracting = input;  // Se actualiza el estado de interacción
    }

    public void OpenCustomUI(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;  // Se asegura de que solo el propietario abra la UI
        if (context.ReadValueAsButton())
        {
            customizerUIRectTransform.gameObject.SetActive(true);  // Se activa o desactiva la UI de customización

            Cursor.lockState = CursorLockMode.None;  // Se desbloquea el cursor
            Cursor.visible = true;  // Se muestra el cursor
            PlayerInput playerInput = GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                playerInput.enabled = false;  // Se desactiva el PlayerInput 
            }
        }
    }
    public void OpenBasicGuideUI(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        if (context.ReadValueAsButton())
        {
            basicGuideUIRectTransform.gameObject.SetActive(true);  // Se activa o desactiva la UI de customización

            Cursor.lockState = CursorLockMode.None;  // Se desbloquea el cursor
            Cursor.visible = true;  // Se muestra el cursor
            PlayerInput playerInput = GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                playerInput.enabled = false;  // Se desactiva el PlayerInput 
            }
        }
    }
    public void CloseUIBehaviour()
    {
        if (!IsOwner) return;  // Se asegura de que solo el propietario abra la UI

        Cursor.lockState = CursorLockMode.Locked;  // Se bloquea el cursor
        Cursor.visible = false;  // Se oculta el cursor
        PlayerInput playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = true;  // Se reactiva el PlayerInput
        }
    }

}