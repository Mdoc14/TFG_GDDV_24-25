using UnityEngine;
using TMPro;
using System.Collections;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class LoadScreen : MonoBehaviour
{
    public TextMeshProUGUI loadingText;
    public float animationSpeed = 0.5f;
    private bool isLoading = true;
    public float disconnectTimeout = 15f;

    private string baseText = "Cargando";
    private int state = 0;

    private Camera localPlayerCam;

    void Start()
    {
        StartCoroutine(TextAnimation());
        StartCoroutine(TimeOutDisconnect());
    }

    void Update()
    {
        if (localPlayerCam == null)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                var netObj = player.GetComponent<Unity.Netcode.NetworkObject>();
                if (netObj != null && netObj.IsOwner)
                {
                    localPlayerCam = player.GetComponentInChildren<Camera>(true);
                }
            }
        }

        if (localPlayerCam != null && localPlayerCam.targetTexture == null && localPlayerCam.enabled)
        {
            // Se oculta la pantalla de carga
            gameObject.SetActive(false);
        }
    }

    public IEnumerator TextAnimation()
    {
        while (isLoading)
        {
            loadingText.text = baseText + new string('.', state);
            state = (state + 1) % 4;
            yield return new WaitForSeconds(animationSpeed);
        }
    }
    public IEnumerator TimeOutDisconnect()
    {
        yield return new WaitForSeconds(disconnectTimeout);
        isLoading = false;
        yield return new WaitForSeconds(1f);
        loadingText.text = "Error, volviendo al menú principal";
        loadingText.color = Color.red;
        yield return new WaitForSeconds(4f);
        if (gameObject.activeSelf)
        {
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene("BootScene");
        }
    }
}
