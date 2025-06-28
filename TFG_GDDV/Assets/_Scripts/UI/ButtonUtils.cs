using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using UnityEngine.Audio;

public class ButtonUtils : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float scaleMultiplier = 1.1f;
    [SerializeField] private float animationSpeed = 10f;

    private Button button;
    private Vector3 originalScale;
    private Vector3 targetScale;

    private AudioSource audioSource;
    private AudioReferences audioReferences;

    void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
        button = GetComponent<Button>();
        button.AddComponent<AudioSource>();

        audioSource = button.gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        audioReferences = Resources.Load<AudioReferences>("AudioReferences");
        AudioMixer mixer = audioReferences.mixerGeneral;

        AudioMixerGroup[] groups = mixer.FindMatchingGroups("SFX");
        if (groups.Length > 0)
        {
            audioSource.outputAudioMixerGroup = groups[0];
        }
        else
        {
            Debug.LogWarning("No se encontró el grupo SFX en el AudioMixer.");
        }
        audioSource.clip = audioReferences.clipBotonHover;
    }

    void OnEnable()
    {
        transform.localScale = originalScale;
        targetScale = originalScale;
    }

    void OnDisable()
    {
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
        audioSource.Play();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;
        targetScale = originalScale;
    }
}
