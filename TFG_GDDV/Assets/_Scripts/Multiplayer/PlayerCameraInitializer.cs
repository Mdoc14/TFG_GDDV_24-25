using UnityEngine;
using Unity.Netcode;

public class PlayerCameraInitializer : NetworkBehaviour
{
    public Camera myCamera;
    public AudioListener myAudio;
    public Canvas hostUI;

    void Start()
    {
        // Esperar a que se spawnee por red
        if (IsOwner)
        {
            EnableMyCamera();
        }
        else
        {
            if(!IsServer && hostUI != null)
            {
                hostUI.gameObject.SetActive(false);
            }
            DisableMyCamera();
        }
    }

    private void EnableMyCamera()
    {
        if (myCamera != null) myCamera.enabled = true;
        if (myAudio != null) myAudio.enabled = true;
    }

    private void DisableMyCamera()
    {
        if (myCamera != null) myCamera.enabled = false;
        if (myAudio != null) myAudio.enabled = false;
    }
}
