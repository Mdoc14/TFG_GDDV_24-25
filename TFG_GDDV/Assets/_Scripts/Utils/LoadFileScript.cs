using UnityEngine;
using SFB;
using System.IO;
using UnityEditor;

public static class LoadFileScript
{
    public static string currentPhoto = null;
    public static string currentVideo = null;
    public static string PickImagePath()
    {
        var extensions = new[] { new ExtensionFilter("Image Files", "png", "jpg", "jpeg") }; // Filtrado de extensiones para que solo se puedan cargar imágenes.
        string[] filePath = StandaloneFileBrowser.OpenFilePanel("Selecciona una imagen", "", extensions, false);
        currentPhoto = filePath[0];
        return currentPhoto;
    }
    public static string PickVideoPath()
    {
        var extensions = new[] { new ExtensionFilter("Video Files", "mp4") }; // Filtrado de extensiones para que solo se puedan cargar videos en mp4.
        string[] filePath = StandaloneFileBrowser.OpenFilePanel("Selecciona un video", "", extensions, false);
        currentVideo = filePath[0];
        return currentVideo;
    }
}
