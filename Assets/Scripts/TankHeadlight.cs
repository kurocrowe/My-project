using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class TankHeadlight : MonoBehaviour
{
    [Header("Light")]
    [SerializeField] private Light2D headlight;
    [SerializeField] private Color lightColor = new Color(1f, 0.86f, 0.48f, 1f);
    [SerializeField] private float normalRange = 6f;
    [SerializeField] private float expandedRange = 8.5f;
    [SerializeField] private float innerAngle = 24f;
    [SerializeField] private float outerAngle = 54f;
    [SerializeField] private float intensity = 1.35f;
    [SerializeField] private float rangeChangeSpeed = 10f;

    [Header("Controls")]
    [SerializeField] private string expandActionName = "Sprint";
    [SerializeField] private Key expandKey = Key.LeftShift;

    private const string HeadlightObjectName = "Tank Headlight";
    private InputAction expandAction;

    private void Awake()
    {
        expandAction = InputSystem.actions?.FindAction(expandActionName, false);
        EnsureHeadlight();
        ConfigureHeadlight(normalRange);
    }

    private void Update()
    {
        float targetRange = IsExpandPressed() ? expandedRange : normalRange;
        headlight.pointLightOuterRadius = Mathf.Lerp(
            headlight.pointLightOuterRadius,
            targetRange,
            Time.deltaTime * rangeChangeSpeed
        );
    }

    private void EnsureHeadlight()
    {
        if (headlight != null)
        {
            return;
        }

        Transform existingHeadlight = transform.Find(HeadlightObjectName);
        if (existingHeadlight != null)
        {
            headlight = existingHeadlight.GetComponent<Light2D>();
        }

        if (headlight == null)
        {
            var headlightObject = new GameObject(HeadlightObjectName);
            headlightObject.transform.SetParent(transform, false);
            headlight = headlightObject.AddComponent<Light2D>();
        }

        headlight.transform.localPosition = Vector3.zero;
        headlight.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
    }

    private void ConfigureHeadlight(float range)
    {
        headlight.lightType = Light2D.LightType.Point;
        headlight.color = lightColor;
        headlight.intensity = intensity;
        headlight.pointLightInnerAngle = innerAngle;
        headlight.pointLightOuterAngle = outerAngle;
        headlight.pointLightOuterRadius = range;
    }

    private bool IsExpandPressed()
    {
        if (expandAction != null)
        {
            return expandAction.IsPressed();
        }

        return Keyboard.current != null && Keyboard.current[expandKey].isPressed;
    }
}
