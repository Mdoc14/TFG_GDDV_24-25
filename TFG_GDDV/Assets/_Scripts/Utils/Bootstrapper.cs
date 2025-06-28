using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System.Collections;

public class Bootstrapper : MonoBehaviour
{
    private void Awake()
    {
        StartCoroutine(CleanAndLoadMainMenu());
    }

    private IEnumerator CleanAndLoadMainMenu()
    {
        if (NetworkManager.Singleton != null)
        {
            Debug.Log("Shutting down NetworkManager and cleaning up objects.");
            NetworkManager.Singleton.Shutdown();
            Destroy(NetworkManager.Singleton.gameObject);
        }
        foreach(GameObject obj in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (obj == gameObject) continue; // No destruir este objeto
            Destroy(obj);
        }
        yield return null;

        
        SceneManager.LoadScene("MainMenu");
    }
}
