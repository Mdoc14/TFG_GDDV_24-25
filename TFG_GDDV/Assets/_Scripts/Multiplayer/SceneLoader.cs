using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System.Collections;
using System;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;

public class SceneLoader : MonoBehaviour
{
    private void Start()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("Cliente: desconectado del host. Volviendo al menú...");
            StartCoroutine(LoadMenuAfterHostDisconnect());
        }
    }

    private IEnumerator LoadMenuAfterHostDisconnect()
    {
        yield return new WaitForSeconds(0.5f);
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("BootScene");
    }

    // Funciones del host
    public void ChangeToGameScene()
    {
        SceneManager.LoadSceneAsync("GameScene");
    }

    public void ChangeToMenuScene()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            StartCoroutine(LoadBootSceneAsHost());
        }
    }
    
    private IEnumerator LoadBootSceneAsHost()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("Host: cambiando a BootScene (sin desconectar)");
        SceneManager.LoadSceneAsync("BootScene");
    }
}
