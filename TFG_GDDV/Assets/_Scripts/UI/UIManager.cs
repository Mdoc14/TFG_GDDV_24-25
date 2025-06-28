using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    AudioReferences audioReferences;
    private void Start()
    {
        audioReferences = Resources.Load<AudioReferences>("AudioReferences");

        if (SceneManager.GetActiveScene().name.Equals("MainMenu"))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        for (int i = 0; i < sliders.Length; i++)
        {
            int index = i; 
            sliders[i].onValueChanged.AddListener((_) => OnSliderValueChanged(index));
        }

        LoadSettings(); 
    }
    public Slider[] sliders;
    public void SaveSettings()
    {
        if (sliders == null) return;

        float generalVol = sliders[0].value;
        float musicVol = sliders[1].value;
        float sfxVol = sliders[2].value;

        PlayerPrefs.SetFloat("generalVolume", generalVol);
        PlayerPrefs.SetFloat("musicVolume", musicVol);
        PlayerPrefs.SetFloat("sfxVolume", sfxVol);

        float generalDB = Mathf.Clamp(ConvertToDecibels(generalVol), -80f, 0f);
        float musicDB = Mathf.Clamp(ConvertToDecibels(musicVol), -80f, 0f);
        float sfxDB = Mathf.Clamp(ConvertToDecibels(sfxVol), -80f, 0f);

        AudioMixer mixer = audioReferences.mixerGeneral;
        mixer.SetFloat("MasterMixer", generalDB);
        mixer.SetFloat("MusicMixer", musicDB);
        mixer.SetFloat("SFXMixer", sfxDB);
    }

    public void LoadSettings()
    {
        try
        {
            sliders[0].value = PlayerPrefs.GetFloat("generalVolume", 100);
            sliders[1].value = PlayerPrefs.GetFloat("musicVolume", 100);
            sliders[2].value = PlayerPrefs.GetFloat("sfxVolume", 100);

            float generalVol = sliders[0].value;
            float musicVol = sliders[1].value;
            float sfxVol = sliders[2].value;

            float generalDB = Mathf.Clamp(ConvertToDecibels(generalVol), -80f, 0f);
            float musicDB = Mathf.Clamp(ConvertToDecibels(musicVol), -80f, 0f);
            float sfxDB = Mathf.Clamp(ConvertToDecibels(sfxVol), -80f, 0f);

            AudioMixer mixer = audioReferences.mixerGeneral;
            mixer.SetFloat("MasterMixer", generalDB);
            mixer.SetFloat("MusicMixer", musicDB);
            mixer.SetFloat("SFXMixer", sfxDB);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error al cargar los ajustes de audio: " + e.Message);
        }
    }

    private float ConvertToDecibels(float sliderValue)
    {
        if (sliderValue <= 0.01f)
            return -80f;

        return Mathf.Log10(sliderValue / 100f) * 20f;
    }

    public void OnSliderValueChanged(int index)
    {
        if (audioReferences == null || sliders == null) return;

        float value = sliders[index].value;
        float db = Mathf.Clamp(ConvertToDecibels(value), -80f, 0f);

        switch (index)
        {
            case 0: // General
                audioReferences.mixerGeneral.SetFloat("MasterMixer", db);
                break;
            case 1: // Música
                audioReferences.mixerGeneral.SetFloat("MusicMixer", db);
                break;
            case 2: // SFX
                audioReferences.mixerGeneral.SetFloat("SFXMixer", db);
                break;
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < sliders.Length; i++)
        {
            sliders[i].onValueChanged.RemoveAllListeners();
        }
    }

    public void Quit()
    {
        Debug.Log("Cerrando aplicación...");
        Application.Quit();
    }
}
