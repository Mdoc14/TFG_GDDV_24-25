using UnityEngine;
using SFB;
using System.IO;
using UnityEditor;

public static class LoadPhotoScript
{
    public static string currentPhoto = null;
    public static string PickImagePath()
    {
        var extensions = new[] { new ExtensionFilter("Image Files", "png", "jpg", "jpeg") }; // Filtrado de extensiones para que solo se puedan cargar imágenes.
        string[] filePath = StandaloneFileBrowser.OpenFilePanel("Selecciona una imagen", "", extensions, false);
        currentPhoto = filePath[0];
        return currentPhoto;
    }
}
