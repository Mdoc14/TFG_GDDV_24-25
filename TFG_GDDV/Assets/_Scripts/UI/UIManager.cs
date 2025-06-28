using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private void Start()
    {
        if (SceneManager.GetActiveScene().name.Equals("MainMenu"))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    public Slider[] sliders;
    public void SaveSettings()
    {
        if (sliders == null) return; 
        PlayerPrefs.SetFloat("generalVolume", sliders[0].value);
        PlayerPrefs.SetFloat("musicVolume", sliders[1].value);
        PlayerPrefs.SetFloat("sfxVolume", sliders[2].value);
    }
    public void LoadSettings()
    {
        try
        {
            sliders[0].value = PlayerPrefs.GetFloat("generalVolume");
            sliders[1].value = PlayerPrefs.GetFloat("musicVolume");
            sliders[2].value = PlayerPrefs.GetFloat("sfxVolume");
        }
        catch 
        { 
        }
    }
    public void Quit()
    {
        Debug.Log("Cerrando aplicación...");
        Application.Quit();
    }
}
