using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.VisualScripting;

public class HostBehaviour : NetworkBehaviour
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

    public Camera cam;               // Referencia a la c�mara del jugador
    private CharacterController controller;     // Referencia al CharacterController para el movimiento
    public Animator animator;                  // Referencia al Animator para animaciones

    private bool isInteracting = false;         // Flag para controlar la interacci�n
    private IInteractable currentInteractable;  // Referencia al objeto interactuable actual
    private GameObject interactKeyHint;         // Referencia al objeto de la UI de la tecla de interacci�n
    private RectTransform crosshairRectTransform;             // Referencia al objeto de la UI del crosshair
    public RectTransform customizerUIRectTransform;             // Referencia al objeto de la UI de customizaci�n de avatar
    public RectTransform basicGuideUIRectTransform;             // Referencia al objeto de la UI de gu�a b�sica
    private Image crosshairUI;             // Referencia al objeto de la UI del crosshair
    Color crosshairColor;  // Obt�n el color actual del crosshair

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
        HandleRotation();   // Se gestiona la rotaci�n del personaje y la c�mara
        if (CheckForInteractable())
        {
            interactKeyHint.SetActive(true);  // Se activa el bot�n de interacci�n si se encuentra un objeto interactuable
            crosshairColor.a = 0.8f;                   // Modifica la propiedad 'a' (alfa)
            crosshairUI.color = crosshairColor;   // Se aplica el nuevo color al crosshair
            crosshairRectTransform.localScale = new Vector3(1.5f, 1.5f, 1.5f); // Se aumenta el tama�o del crosshair
            if (isInteracting && currentInteractable != null)
            {
                currentInteractable.Interact();  // Se llama al m�todo Interact del objeto interactuable
                isInteracting = false;  // Se resetea el flag de interacci�n
            }
        }
        else
        {
            interactKeyHint.SetActive(false);  // Se desactiva el bot�n de interacci�n si no se encuentra un objeto interactuable
            transform.Find("PlayerUI/InteractButton").gameObject.SetActive(false);
            crosshairColor.a = 0.4f;                   // Modifica la propiedad 'a' (alfa)
            crosshairUI.color = crosshairColor;   // Se aplica el nuevo color al crosshair
            crosshairRectTransform.localScale = new Vector3(1f, 1f, 1f); // Se resetea el tama�o del crosshair
            currentInteractable = null;  // Se resetea el objeto interactuable si no hay ninguno en el campo de visi�n
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

    private bool CheckForInteractable()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 3f))
        {
            Debug.DrawRay(ray.origin, ray.direction * 3f, Color.red);  // Se dibuja un rayo en la escena para depuraci�n
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
        direction = new Vector3(input.x, 0, input.y);  // Se almacena la direcci�n de movimiento

        animator.SetFloat("moveX", input.x);
        animator.SetFloat("moveY", input.y);


        if (input.x != 0 || input.y != 0)
        {
            animator.SetBool("isMoving", true);  // Se activa la animaci�n de movimiento
        }
        else
        {
            animator.SetBool("isMoving", false);  // Se desactiva la animaci�n de movimiento
        }
    }

    public void Look(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;  // Se asegura de que solo el propietario controle la c�mara
        Vector2 input = context.ReadValue<Vector2>();  // Se obtiene la entrada del rat�n o joystick
        targetRotationY += input.x * sensitivity;  // Se actualiza la rotaci�n en el eje Y
        targetRotationX -= input.y * sensitivity;  // Se actualiza la inclinaci�n en el eje X
    }

    public void Interact(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;  // Se asegura de que solo el propietario interact�e
        bool input = context.ReadValueAsButton();  // Se obtiene la entrada del teclado
        isInteracting = input;  // Se actualiza el estado de interacci�n
    }

    public void OpenCustomUI(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;  // Se asegura de que solo el propietario abra la UI
        if (context.ReadValueAsButton())
        {
            customizerUIRectTransform.gameObject.SetActive(true);  // Se activa o desactiva la UI de customizaci�n

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
            basicGuideUIRectTransform.gameObject.SetActive(true);  // Se activa o desactiva la UI de customizaci�n

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