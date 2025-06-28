using UnityEngine;
using TMPro;

public class LoadScreen : MonoBehaviour
{
    public TextMeshProUGUI loadingText;
    public float animationSpeed = 0.5f;

    private string baseText = "Cargando";
    private int state = 0;

    private Camera localPlayerCam;

    void Start()
    {
        StartCoroutine(TextAnimation());
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

    System.Collections.IEnumerator TextAnimation()
    {
        while (true)
        {
            loadingText.text = baseText + new string('.', state);
            state = (state + 1) % 4;
            yield return new WaitForSeconds(animationSpeed);
        }
    }
}
