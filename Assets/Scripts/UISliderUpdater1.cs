// UISliderUpdater.cs
// Attach to Health Bar Canvas

using UnityEngine;
using UnityEngine.UI;

public class UISliderUpdater1 : MonoBehaviour
{
    [Header("Slider")]
    [SerializeField] private Slider sliderUi;

    [SerializeField]
    private Vector3 offset =
        new Vector3(0f, 1.5f, 0f);

   

    // Set max health
    public void SetMaxValue(float maxValue)
    {
        sliderUi.maxValue = maxValue;
        sliderUi.value = maxValue;
    }

    // Update current health
    public void SetSliderValue(float value)
    {
        sliderUi.value = value;
    }
}