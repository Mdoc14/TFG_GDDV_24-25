using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderText : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Slider slider;

    public void changeText()
    {
        text.text = slider.value.ToString();
    }
}
