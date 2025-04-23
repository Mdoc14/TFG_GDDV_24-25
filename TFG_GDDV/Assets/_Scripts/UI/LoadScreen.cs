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
        if (GameObject.FindGameObjectsWithTag("Player") == null) return;
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (!player.GetComponentInChildren<CinemachineCamera>().enabled) continue;
            gameObject.SetActive(false);
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
