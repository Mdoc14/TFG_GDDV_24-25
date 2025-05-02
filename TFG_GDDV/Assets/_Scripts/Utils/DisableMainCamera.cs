using UnityEngine;

public class DisableMainCamera : MonoBehaviour
{
    
    GameObject loadingScreen;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        loadingScreen = GameObject.Find("LoadingScreen");
    }

    // Update is called once per frame
    void Update()
    {
        if (!loadingScreen.activeSelf)
        {
            this.gameObject.SetActive(false);
        }
    }
}
