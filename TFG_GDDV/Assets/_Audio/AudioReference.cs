using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "Audio/AudioReference", fileName = "AudioReferences")]
public class AudioReferences : ScriptableObject
{
    [Header("Mixers")]
    public AudioMixer mixerGeneral;

    [Header("Clips")]
    public AudioClip clipBotonHover;
}
