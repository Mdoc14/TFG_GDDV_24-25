using UnityEngine;
using UnityEngine.Video;

public class InteractableTV : MonoBehaviour, IInteractable
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private MeshRenderer meshRenderer;

    public void Interact()
    {
        Debug.Log("Interactuando con la TV");
        string videoPath = LoadFileScript.PickVideoPath();

        if (!string.IsNullOrEmpty(videoPath))
        {
            videoPlayer.Stop();

            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = videoPath;

            // Configurar el modo de salida de audio
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;

            // Activar todas las pistas de audio disponibles
            int trackCount = (int)videoPlayer.audioTrackCount;
            for (int i = 0; i < trackCount; i++)
            {
                videoPlayer.EnableAudioTrack((ushort)i, true);
                videoPlayer.SetTargetAudioSource((ushort)i, audioSource);
            }

            videoPlayer.Prepare();
            videoPlayer.prepareCompleted += OnVideoPrepared;

            meshRenderer.enabled = true;
            videoPlayer.loopPointReached += OnVideoEnded;
        }
        else
        {
            meshRenderer.enabled = false;
        }
    }



    private void OnVideoPrepared(VideoPlayer vp)
    {
        vp.Play();
    }

    private void OnVideoEnded(VideoPlayer vp)
    {
        meshRenderer.enabled = false;
    }
}
