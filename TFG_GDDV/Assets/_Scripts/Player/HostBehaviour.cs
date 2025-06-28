using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.UI;

public class HostBehaviour : NetworkBehaviour
{
    public float moveSpeed = 5f;                // Velocidad de movimiento del jugador
    public float sensitivity = 0.3f;            // Sensibilidad del ratón para mirar alrededor
    public float smoothTime = 0.05f;            // Controla la suavidad de la cámara

    private Vector3 direction = Vector3.zero;               // Dirección en la que se mueve el personaje
    private float targetRotationY = 0f;                     // Rotación deseada en el eje Y
    private float targetRotationX = 0f;                     // Rotación deseada en el eje X
    private float currentRotationY = 0f;                    // Rotación actual en el eje Y
    private float currentRotationX = 0f;                    // Rotación actual en el eje X

    private float rotationVelocityY = 0f;                   // Velocidad de interpolación en el eje Y para suavizado
    private float rotationVelocityX = 0f;                   // Velocidad de interpolación en el eje X para suavizado

    public Camera cam;                                      // Referencia a la cámara del jugador
    private CharacterController controller;                 // Referencia al CharacterController para el movimiento
    public Animator animator;                               // Referencia al Animator para animaciones

    private bool isInteracting = false;                     // Flag para controlar la interacción
    private IInteractable currentInteractable;              // Referencia al objeto interactuable actual
    private GameObject interactKeyHint;                     // Referencia al objeto de la UI de la tecla de interacción
    private RectTransform crosshairRectTransform;           // Referencia al objeto de la UI del crosshair
    public RectTransform customizerUIRectTransform;         // Referencia al objeto de la UI de customización de avatar
    public RectTransform pauseUIRectTransform;              // Referencia al objeto de la UI de pausa
    public RectTransform basicGuideUIRectTransform;         // Referencia al objeto de la UI de guía básica
    public RectTransform controlsUI;                        // Referencia al objeto de la UI de controles
    private Image crosshairUI;                              // Referencia al objeto de la UI del crosshair
    Color crosshairColor;                                   // Obtén el color actual del crosshair

    public override void OnNetworkSpawn()
    {

        controller = GetComponent<CharacterController>();

    }

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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


        HandleMovement();  
        HandleRotation();  
        if (CheckForInteractable())
        {
            interactKeyHint.SetActive(true);  
            crosshairColor.a = 0.8f;                
            crosshairUI.color = crosshairColor;   
            crosshairRectTransform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            if (isInteracting && currentInteractable != null)
            {
                currentInteractable.Interact(); 
                isInteracting = false;  
            }
        }
        else
        {
            interactKeyHint.SetActive(false);  
            transform.Find("PlayerUI/InteractButton").gameObject.SetActive(false);
            crosshairColor.a = 0.4f;                   
            crosshairUI.color = crosshairColor; 
            crosshairRectTransform.localScale = new Vector3(1f, 1f, 1f); 
            currentInteractable = null; 
        }
    }

    private void HandleMovement()
    {
        Vector3 move = (transform.forward * direction.z + transform.right * direction.x) * moveSpeed * Time.deltaTime;
        controller.Move(move);  
    }

    private void HandleRotation()
    {
        currentRotationY = Mathf.SmoothDamp(currentRotationY, targetRotationY, ref rotationVelocityY, smoothTime);
        transform.rotation = Quaternion.Euler(0f, currentRotationY, 0f); 

        targetRotationX = Mathf.Clamp(targetRotationX, -80f, 80f); 
        currentRotationX = Mathf.SmoothDamp(currentRotationX, targetRotationX, ref rotationVelocityX, smoothTime);
        cam.transform.localRotation = Quaternion.Euler(currentRotationX, 0f, 0f);  
    }

    private bool CheckForInteractable()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 3f))
        {
            Debug.DrawRay(ray.origin, ray.direction * 3f, Color.red); 
            if (hit.collider.CompareTag("interactable"))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    currentInteractable = interactable;
                    return true;  
                }
            }
        }
        return false;
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;  
        Vector2 input = context.ReadValue<Vector2>();  
        direction = new Vector3(input.x, 0, input.y);  

        animator.SetFloat("moveX", input.x);
        animator.SetFloat("moveY", input.y);


        if (input.x != 0 || input.y != 0)
        {
            animator.SetBool("isMoving", true);  
        }
        else
        {
            animator.SetBool("isMoving", false);  
        }
    }

    public void Look(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;  
        Vector2 input = context.ReadValue<Vector2>();  
        targetRotationY += input.x * sensitivity; 
        targetRotationX -= input.y * sensitivity;  
    }

    public void Interact(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;  
        bool input = context.ReadValueAsButton();  
        isInteracting = input;  
    }

    public void OpenCustomUI(InputAction.CallbackContext context)
    {
        if (!IsOwner) return; 
        if (context.ReadValueAsButton())
        {
            customizerUIRectTransform.gameObject.SetActive(true);  

            Cursor.lockState = CursorLockMode.None;  
            Cursor.visible = true;  
            PlayerInput playerInput = GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                playerInput.enabled = false; 
            }
        }
    }

    public void OpenPauseUI(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;  
        if (context.ReadValueAsButton())
        {
            pauseUIRectTransform.gameObject.SetActive(true);  
            controlsUI.gameObject.SetActive(false);  

            Cursor.lockState = CursorLockMode.None;  
            Cursor.visible = true;  
            PlayerInput playerInput = GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                playerInput.enabled = false;  
            }
        }
    }

    public void OpenBasicGuideUI(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        if (context.ReadValueAsButton())
        {
            basicGuideUIRectTransform.gameObject.SetActive(true); 

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            PlayerInput playerInput = GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                playerInput.enabled = false; 
            }
        }
    }
    public void CloseUIBehaviour()
    {
        if (!IsOwner) return;  

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        PlayerInput playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = true;
        }
    }

}