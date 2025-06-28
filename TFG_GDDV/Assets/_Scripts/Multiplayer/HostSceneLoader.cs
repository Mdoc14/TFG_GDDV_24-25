using UnityEngine;

public class HostSceneLoader : MonoBehaviour
{
    public void OnExitToMenu()
    {
        SceneLoader loader = FindAnyObjectByType<SceneLoader>();

        if (loader != null)
        {
            loader.ChangeToMenuScene();
        }
        else
        {
            Debug.LogWarning("SceneLoader no encontrado en la escena.");
        }
    }
}
