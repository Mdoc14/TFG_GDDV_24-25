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
using System.Windows.Forms;
using System.Linq;

public class InteractableTV : NetworkBehaviour, IInteractable
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private TMP_InputField urlInputField;
    [SerializeField] private GameObject videoUploaderScreen;
    
    private PlayerInput playerInput;
    private GameObject playerUI;



    public void OnPlayButtonPressed()
    {
        Debug.Log("Confirm button clicked!");
        if (!IsServer) return; // Solo el host puede lanzar el video

        string originalUrl = urlInputField.text.Trim();
        string url = ConvertGoogleDriveUrl(originalUrl);

        if (string.IsNullOrEmpty(url))
        {
            ShowPopupMessage("La URL ingresada no es válida.");
            Debug.LogWarning("La URL ingresada está vacía o mal formada.");
            return; 
        }

        Debug.Log("Host ha ingresado URL: " + url);
        // Usa un nombre de archivo basado en la URL codificada (sin caracteres inválidos)
        string safeName = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(url));
        safeName = new string(safeName.Where(char.IsLetterOrDigit).ToArray()); // Elimina caracteres raros
        string fileName = safeName + ".mp4";
        if (!fileName.EndsWith(".mp4"))
        {
            fileName += ".mp4"; // Si no tiene extensión .mp4, la agregamos manualmente
        }


        ShareVideoUrlClientRpc(url, fileName);
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
                    // Forma alternativa para extraer el ID manualmente
                    int start = originalUrl.IndexOf("/d/") + 3;
                    int end = originalUrl.IndexOf("/", start);
                    string fileId = originalUrl.Substring(start, end - start);
                    return $"https://drive.google.com/uc?export=download&id={fileId}";
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error procesando la URL de Google Drive: " + e.Message);
            }
        }

        return originalUrl; // Si no es de Drive, la dejamos igual
    }


    [ClientRpc]
    private void ShareVideoUrlClientRpc(string url, string fileName)
    {
        StartCoroutine(DownloadAndPrepare(url, fileName));
    }

    private IEnumerator DownloadAndPrepare(string url, string fileName)
    {
        string videosFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
        string baseFolderPath = Path.Combine(videosFolderPath, "RecuerDos_videos");

        // --- Carpeta distinta según si eres Host o Cliente ---
        string roleFolder = IsServer ? "host" : "client";
        string newFolderPath = Path.Combine(baseFolderPath, roleFolder);

        if (!Directory.Exists(newFolderPath))
            Directory.CreateDirectory(newFolderPath);

        // --- Aseguramos extensión ---
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
                    case "video/ogg":
                        extension = ".ogv";
                        break;
                }
            }
        }

        if (Path.GetExtension(fileName) == string.Empty)
            fileName += extension;

        string localPath = Path.Combine(newFolderPath, fileName);

        // --- VERIFICAR SI YA EXISTE ---
        if (File.Exists(localPath))
        {
            Debug.Log($"[{roleFolder.ToUpper()}] El video ya existe: {localPath}");
        }
        else
        {
            Debug.Log($"[{roleFolder.ToUpper()}] Descargando video en: {localPath}");

            UnityWebRequest downloadRequest = UnityWebRequest.Get(url);
            yield return downloadRequest.SendWebRequest();

            if (downloadRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error descargando el video: " + downloadRequest.error);
                ShowPopupMessage("Error al descargar el video.\n¿Es pública la URL?");
                yield break;
            }

            if (downloadRequest.downloadHandler.data.Length < 1000)
            {
                Debug.LogError("Descarga sospechosa: puede no ser un archivo válido.");
                ShowPopupMessage("No se pudo descargar el video.\nRevisa que el archivo esté compartido como público en Google Drive.");
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
                ShowPopupMessage("Error escribiendo archivo. ¿Está siendo usado?");
                yield break;
            }
        }

        // --- PREPARAR el VideoPlayer ---
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



    private void ShowPopupMessage(string message)
    {
        GameObject popupCanvas = new GameObject("PopupCanvas");
        popupCanvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        popupCanvas.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        popupCanvas.AddComponent<GraphicRaycaster>();

        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(popupCanvas.transform, false);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(400, 200);
        panelRect.anchoredPosition = Vector2.zero;

        GameObject textGO = new GameObject("PopupText");
        textGO.transform.SetParent(panel.transform, false);
        TMP_Text text = textGO.AddComponent<TextMeshProUGUI>();
        text.text = message;
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 24;
        text.color = Color.white;
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(380, 160);
        textRect.anchoredPosition = Vector2.zero;

        GameObject buttonGO = new GameObject("CloseButton");
        buttonGO.transform.SetParent(panel.transform, false);
        UnityEngine.UI.Button button = buttonGO.AddComponent<UnityEngine.UI.Button>();
        Image buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = new Color(1, 1, 1, 0.6f);
        RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(100, 40);
        buttonRect.anchoredPosition = new Vector2(0, -70);

        GameObject buttonTextGO = new GameObject("ButtonText");
        buttonTextGO.transform.SetParent(buttonGO.transform, false);
        TMP_Text buttonText = buttonTextGO.AddComponent<TextMeshProUGUI>();
        buttonText.text = "Cerrar";
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.fontSize = 20;
        buttonText.color = Color.black;
        RectTransform buttonTextRect = buttonTextGO.GetComponent<RectTransform>();
        buttonTextRect.sizeDelta = new Vector2(100, 40);
        buttonTextRect.anchoredPosition = Vector2.zero;

        button.onClick.AddListener(() => GameObject.Destroy(popupCanvas));
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




    public void Interact()
    {
        videoUploaderScreen.SetActive(true);
        UnityEngine.Cursor.lockState = CursorLockMode.None; // Desbloquear el cursor
        UnityEngine.Cursor.visible = true; // Hacer visible el cursor
        // Aqui se deshabilita el inputSystem del jugador que es owner del runtime
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (player.GetComponent<NetworkObject>().IsOwner)
            {
                playerUI = player.transform.Find("PlayerUI").gameObject; // Desactivar el UI del jugador
                playerUI.SetActive(false);
                playerInput = player.GetComponent<PlayerInput>();
                playerInput.enabled = false; // Deshabilitar el input del jugador
                break;
            }
        }
    }

    public void ReturnToGame() {

        GameObject.Find("VideoUploaderScreen").SetActive(false);
        UnityEngine.Cursor.lockState = CursorLockMode.Locked; // Bloquear el cursor
        UnityEngine.Cursor.visible = false; // Hacer invisible el cursor
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
