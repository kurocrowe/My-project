// UISliderUpdater.cs
// Attach to Health Bar Canvas

using UnityEngine;
using UnityEngine.UI;

public class UISliderUpdater : MonoBehaviour
{
    [Header("Slider")]
    [SerializeField] private Slider sliderUi;

    [Header("Follow Target")]
    [SerializeField] private Transform target;

    [SerializeField]
    private Vector3 offset =
        new Vector3(0f, 1.5f, 0f);

    void LateUpdate()
    {
        if (target == null)
            return;

        // Follow target
        transform.position =
            target.position + offset;

        // Prevent rotation
        transform.rotation = Quaternion.identity;
    }

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