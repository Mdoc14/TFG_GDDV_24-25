using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.Video;
using System;

public class VideoUploaderGoogleDrive : NetworkBehaviour
{
    [Header("UI")]
    public InputField googleDriveInputField;
    public Button uploadButton;

    [Header("Video")]
    public VideoPlayer videoPlayer;

    private bool videoReadyToPlay = false;
    private string localVideoPath;

    private void Start()
    {
        if (IsServer)
        {
            uploadButton.onClick.AddListener(OnUploadButtonClicked);
        }
        else
        {
            uploadButton.interactable = false;
            googleDriveInputField.interactable = false;
        }
    }

    private void Update()
    {
        if (IsServer && videoReadyToPlay)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("HOST: Lanzando reproducción para todos...");
                PlayVideoClientRpc();
                videoPlayer.Play();
                videoReadyToPlay = false;
            }
        }
    }

    private void OnUploadButtonClicked()
    {
        string originalLink = googleDriveInputField.text;
        if (!string.IsNullOrEmpty(originalLink))
        {
            string downloadLink = GetGoogleDriveDownloadLink(originalLink);
            if (!string.IsNullOrEmpty(downloadLink))
            {
                string fileName = "video_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".mp4";
                ShareVideoUrlClientRpc(downloadLink, fileName);
            }
        }
    }

    [ClientRpc]
    private void ShareVideoUrlClientRpc(string url, string fileName)
    {
        StopAllCoroutines(); // Muy importante: paramos descargas anteriores
        StartCoroutine(DownloadAndPrepare(url, fileName));
    }

    private IEnumerator DownloadAndPrepare(string url, string fileName)
    {
        string videosFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
        string newFolderPath = Path.Combine(videosFolderPath, "RecuerDos_videos");

        if (!Directory.Exists(newFolderPath))
            Directory.CreateDirectory(newFolderPath);

        localVideoPath = Path.Combine(newFolderPath, fileName);

        Debug.Log("Descargando nuevo video desde: " + url);
        UnityWebRequest uwr = UnityWebRequest.Get(url);
        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.Success)
        {
            File.WriteAllBytes(localVideoPath, uwr.downloadHandler.data);
            Debug.Log("Nuevo video descargado en: " + localVideoPath);
        }
        else
        {
            Debug.LogError("Error al descargar el video: " + uwr.error);
            yield break;
        }

        videoPlayer.Stop();
        videoPlayer.url = "file://" + localVideoPath;
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        Debug.Log("Nuevo video preparado. Esperando PLAY...");
        videoReadyToPlay = true;
    }

    [ClientRpc]
    private void PlayVideoClientRpc()
    {
        Debug.Log("Cliente recibido señal de PLAY para nuevo video");
        videoPlayer.Play();
    }

    private string GetGoogleDriveDownloadLink(string originalLink)
    {
        try
        {
            Uri uri = new Uri(originalLink);
            string[] segments = uri.Segments;

            if (segments.Length >= 3)
            {
                string fileId = segments[2].Trim('/');
                return $"https://drive.google.com/uc?export=download&id={fileId}";
            }
            else
            {
                Debug.LogError("Formato de URL de Google Drive no válido.");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error procesando URL: " + ex.Message);
            return null;
        }
    }
}
