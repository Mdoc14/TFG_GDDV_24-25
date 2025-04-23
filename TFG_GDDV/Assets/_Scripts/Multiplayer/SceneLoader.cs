using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public void ChangeToGameScene()
    {
        SceneManager.LoadSceneAsync("GameScene");
    }
    public void ChangeToMenuScene()
    {
        GameObject.Destroy(this);
        SceneManager.LoadSceneAsync("MainMenu");
    }
}
