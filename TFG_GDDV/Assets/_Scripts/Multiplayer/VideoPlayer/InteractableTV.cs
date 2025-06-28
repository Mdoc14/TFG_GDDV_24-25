using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;
using System.IO;
using UnityEngine.Networking;
using System;
using UnityEngine.InputSystem;
using TMPro;
using System.Linq;

public class InteractableTV : NetworkBehaviour, IInteractable
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private GameObject videoUploaderScreen;
    [SerializeField] private TMP_InputField urlInputField;
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private TextMeshProUGUI globalErrorText;
    
    private PlayerInput playerInput;
    private GameObject playerUI;



    public void OnPlayButtonPressed()
    {
        if (!IsServer) return; 

        string originalUrl = urlInputField.text.Trim();
        if (string.IsNullOrEmpty(originalUrl))
        {
            errorText.text = "La URL ingresada está vacía";
            return;
        }
        if (!originalUrl.Contains("drive.google.com"))
        {
            errorText.text = "Error procesando la URL de Google Drive. Asegúrate de que sea válida.";
            return;
        }
        
        string url = ConvertGoogleDriveUrl(originalUrl);

        if (string.IsNullOrEmpty(url))
        {
            errorText.text = "La URL ingresada está vacía";
            return; 
        }
        
        ReturnToGame(); 
        videoUploaderScreen.SetActive(false); 

        Debug.Log("Host ha ingresado URL: " + url);

        ShareVideoUrlClientRpc(url);
    }



    private string ConvertGoogleDriveUrl(string originalUrl)
    {
        if (originalUrl.Contains("drive.google.com"))
        {
            try
            {
                var uri = new System.Uri(originalUrl);
                var segments = uri.Segments;
                if (segments.Length >= 3 && segments[1] == "file/" && segments[2] == "d/")
                {
                    string fileId = segments[3].TrimEnd('/');
                    return $"https://drive.google.com/uc?export=download&id={fileId}";
                }
                else if (originalUrl.Contains("/d/"))
                {
                    string fileId = ExtractDriveId(originalUrl);
                    return $"https://drive.google.com/uc?export=download&id={fileId}";
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error procesando la URL de Google Drive: " + e.Message);
            }
        }

        return originalUrl; 
    }
    private string ExtractDriveId(string url)
    {
        if (url.Contains("/d/"))
        {
            int start = url.IndexOf("/d/") + 3;
            int end = url.IndexOf("/", start);
            return url.Substring(start, end - start);
        }
        return null;
    }

    [ClientRpc]
    private void ShareVideoUrlClientRpc(string url)
    {
        StartCoroutine(DownloadAndPrepare(url));
    }

    private IEnumerator DownloadAndPrepare(string url)
    {
        string videosFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
        string baseFolderPath = Path.Combine(videosFolderPath, "RecuerDos_videos");

        string roleFolder = IsServer ? "host" : "client";
        string newFolderPath = Path.Combine(baseFolderPath, roleFolder);

        if (!Directory.Exists(newFolderPath))
            Directory.CreateDirectory(newFolderPath);

        string extension = ".mp4";
        string contentType = null;

        UnityWebRequest uwr = UnityWebRequest.Head(url);
        yield return uwr.SendWebRequest();
        if (uwr.result == UnityWebRequest.Result.Success)
        {
            contentType = uwr.GetResponseHeader("Content-Type");

            if (!string.IsNullOrEmpty(contentType))
            {
                switch (contentType)
                {
                    case "video/mp4":
                        extension = ".mp4";
                        break;
                    case "video/webm":
                        extension = ".webm";
                        break;
                    case "video/ogv":
                        extension = ".ogv";
                        break;
                }
            }
        }

        // Aquí se extrae el ID de la url de Google Drive para darle un nombre único al archivo que se guarda en local
        string fileName = ExtractDriveId(url) + extension;

        string localPath = Path.Combine(newFolderPath, fileName);

        if (File.Exists(localPath))
        {
            Debug.Log($"[{roleFolder.ToUpper()}] El video ya existe: {localPath}");
        }
        else
        {
            Debug.Log($"[{roleFolder.ToUpper()}] Descargando video en: {localPath}");

            UnityWebRequest downloadRequest = UnityWebRequest.Get(url);
            yield return downloadRequest.SendWebRequest();

            if (downloadRequest.result != UnityWebRequest.Result.Success || downloadRequest.downloadHandler.data.Length < 1000)
            {
                StartCoroutine(ShowErrorMessageCoroutine("No se pudo descargar el video. Revisa que el archivo esté compartido como público en Google Drive."));
                yield break;
            }

            try
            {
                File.WriteAllBytes(localPath, downloadRequest.downloadHandler.data);
                Debug.Log("Video descargado correctamente.");
            }
            catch (IOException ioEx)
            {
                Debug.LogError("Error escribiendo archivo: " + ioEx.Message);
                yield break;
            }
        }

        videoPlayer.Stop();
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = "file://" + localPath;

        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.EnableAudioTrack(0, true);
        videoPlayer.SetTargetAudioSource(0, audioSource);

        videoPlayer.prepareCompleted -= OnVideoPrepared;
        videoPlayer.prepareCompleted += OnVideoPrepared;

        videoPlayer.loopPointReached -= OnVideoEnded;
        videoPlayer.loopPointReached += OnVideoEnded;

        videoPlayer.Prepare();
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        meshRenderer.enabled = true;
        vp.Play();
    }

    private void OnVideoEnded(VideoPlayer vp)
    {
        meshRenderer.enabled = false;
    }

    private IEnumerator ShowErrorMessageCoroutine(string message)
    {
        globalErrorText.text = message;
        yield return new WaitForSeconds(7f);
        globalErrorText.text = "";
    }


    public void Interact()
    {
        videoUploaderScreen.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true; 
        
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (player.GetComponent<NetworkObject>().IsOwner)
            {
                playerUI = player.transform.Find("PlayerUI").gameObject; 
                playerUI.SetActive(false);
                playerInput = player.GetComponent<PlayerInput>();
                playerInput.enabled = false; 
                break;
            }
        }
    }

    public void ReturnToGame() {

        videoUploaderScreen.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked; // Bloquear el cursor
        Cursor.visible = false; // Hacer invisible el cursor
        // Aqui se habilita el inputSystem del jugador que es owner del runtime
        if (playerInput != null && playerUI) { 
            playerInput.enabled = true; // Habilitar el input del jugador
            playerUI.SetActive(true); // Activar el UI del jugador
            return; // Si el input ya fue habilitado, no hacer nada
        }

        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (player.GetComponent<NetworkObject>().IsOwner)
            {
                playerInput = player.GetComponent<PlayerInput>();
                playerInput.enabled = true; // Habilitar el input del jugador
                break;
            }
        }
    }
}
