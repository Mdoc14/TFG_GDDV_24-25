using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PlayerCameraManager : MonoBehaviour
{
    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
    }

    private void HandleClientConnected(ulong clientId)
    {
        Debug.Log($"SetupCamera will be called (delayed): I'm host? {NetworkManager.Singleton.IsHost}");
        StartCoroutine(WaitAndSetupCamera(NetworkManager.Singleton.IsHost));
    }

    private IEnumerator WaitAndSetupCamera(bool isHost)
    {
        // Esperamos hasta que ambas c�maras est�n instanciadas
        Camera hostCam = null;
        Camera clientCam = null;

        float timeout = 3f;
        float elapsed = 0f;

        while ((hostCam == null || clientCam == null) && elapsed < timeout)
        {
            hostCam = GameObject.Find("HostCamera")?.GetComponent<Camera>();
            clientCam = GameObject.Find("ClientCamera")?.GetComponent<Camera>();
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (hostCam == null || clientCam == null)
        {
            Debug.LogError("No se encontraron ambas c�maras despu�s de esperar.");
            yield break;
        }

        Debug.Log("C�maras encontradas. Ajustando visibilidad.");

        if (isHost)
        {
            clientCam.gameObject.SetActive(false);
            hostCam.tag = "MainCamera";
        }
        else
        {
            hostCam.gameObject.SetActive(false);
            clientCam.tag = "MainCamera";
        }
    }
}
