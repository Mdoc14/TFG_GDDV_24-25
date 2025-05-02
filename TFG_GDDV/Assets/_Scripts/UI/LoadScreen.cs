using System.Collections;
using System.Windows.Forms;
using TMPro;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class LoadScreen : MonoBehaviour
{
    public TextMeshProUGUI loadingText;
    public float animationSpeed = 0.5f; 

    private string baseText = "Cargando";
    private int state = 0;

    void Awake()
    {
        StartCoroutine(TextAnimation());
    }
    void Update()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            var netObject = player.GetComponent<Unity.Netcode.NetworkObject>();
            if (netObject != null && netObject.IsOwner)
            {
                Camera playerCam = player.GetComponentInChildren<Camera>(true); // Incluye cámaras desactivadas
                if (playerCam != null && playerCam.enabled)
                {
                    gameObject.SetActive(false); // Oculta la pantalla de carga
                }
            }
        }
    }


    IEnumerator TextAnimation()
    {
        while (true)
        {
            loadingText.text = baseText + new string('.', state);
            state = (state + 1) % 4; 

            yield return new WaitForSeconds(animationSpeed);
        }
    }
}
