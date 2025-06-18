using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonScaler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float scaleMultiplier = 1.1f;
    [SerializeField] private float animationSpeed = 10f;

    private Button button;
    private Vector3 originalScale;
    private Vector3 targetScale;

    void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
        button = GetComponent<Button>();
    }

    void OnEnable()
    {
        // Restaurar escala al activarse por si se quedó mal
        transform.localScale = originalScale;
        targetScale = originalScale;
    }

    void OnDisable()
    {
        // Restaurar escala cuando el botón se desactiva
        transform.localScale = originalScale;
        targetScale = originalScale;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;
        targetScale = originalScale * scaleMultiplier;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;
        targetScale = originalScale;
    }
}
