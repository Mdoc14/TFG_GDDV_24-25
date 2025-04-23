using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Slider[] sliders;
    public void SaveSettings()
    {
        if (sliders == null) return; 
        PlayerPrefs.SetFloat("generalVolume", sliders[0].value);
        PlayerPrefs.SetFloat("musicVolume", sliders[1].value);
        PlayerPrefs.SetFloat("sfxVolume", sliders[2].value);
        PlayerPrefs.SetFloat("brightAmmount", sliders[3].value);
    }
    public void LoadSettings()
    {
        try
        {
            sliders[0].value = PlayerPrefs.GetFloat("generalVolume");
            sliders[1].value = PlayerPrefs.GetFloat("musicVolume");
            sliders[2].value = PlayerPrefs.GetFloat("sfxVolume");
            sliders[3].value = PlayerPrefs.GetFloat("brightAmmount");
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
