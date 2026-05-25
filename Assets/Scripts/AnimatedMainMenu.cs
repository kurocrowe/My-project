using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public class AnimatedMainMenu : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string playSceneName = "Scene1";

    [Header("Text")]
    [SerializeField] private string gameTitle = "TANK BATTLE";
    [SerializeField] private string subtitle = "Lock on. Roll out. Survive.";

    [Header("Animation")]
    [SerializeField] private float introDuration = 0.8f;
    [SerializeField] private float floatAmount = 14f;
    [SerializeField] private float pulseSpeed = 2.2f;
    [SerializeField] private float backgroundDriftSpeed = 0.35f;

    private RectTransform titleRect;
    private RectTransform panelRect;
    private CanvasGroup menuGroup;
    private GameObject mainMenuPanel;
    private GameObject optionsPanel;
    private Image background;
    private Image glow;
    private TextMeshProUGUI masterVolumeValue;
    private TextMeshProUGUI musicVolumeValue;
    private TextMeshProUGUI sfxVolumeValue;
    private Slider masterVolumeSlider;
    private Slider musicVolumeSlider;
    private Slider sfxVolumeSlider;
    private readonly List<KeyBindButton> keyBindButtons = new List<KeyBindButton>();
    private string waitingForKeyAction;
    private bool isStarting;

    private void Awake()
    {
        BuildMenu();
    }

    private void Start()
    {
        StartCoroutine(PlayIntro());
    }

    private void Update()
    {
        float time = Time.unscaledTime;

        if (titleRect != null)
        {
            titleRect.anchoredPosition = new Vector2(0f, 135f + Mathf.Sin(time * pulseSpeed) * floatAmount);
            titleRect.localScale = Vector3.one * (1f + Mathf.Sin(time * pulseSpeed * 0.75f) * 0.025f);
        }

        if (background != null)
        {
            float drift = Mathf.Sin(time * backgroundDriftSpeed) * 0.08f;
            background.color = Color.Lerp(new Color(0.08f, 0.12f, 0.18f), new Color(0.18f, 0.28f, 0.36f), 0.5f + drift);
        }

        if (glow != null)
        {
            glow.transform.localScale = Vector3.one * (1.2f + Mathf.Sin(time * 1.1f) * 0.08f);
            glow.color = new Color(0.22f, 0.82f, 1f, 0.12f + Mathf.Sin(time * 1.5f) * 0.035f);
        }

        ListenForKeybind();
    }

    public void Play()
    {
        if (isStarting)
        {
            return;
        }

        StartCoroutine(StartGame());
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void OpenOptions()
    {
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(true);
        RefreshKeybindLabels();
    }

    public void CloseOptions()
    {
        waitingForKeyAction = null;
        mainMenuPanel.SetActive(true);
        optionsPanel.SetActive(false);
        RefreshKeybindLabels();
    }

    public void ResetOptions()
    {
        GameSettings.ResetToDefaults();
        GameSettings.ApplyAudioSettings();
        SetSliderWithoutNotify(masterVolumeSlider, GameSettings.MasterVolume);
        SetSliderWithoutNotify(musicVolumeSlider, GameSettings.MusicVolume);
        SetSliderWithoutNotify(sfxVolumeSlider, GameSettings.SfxVolume);
        UpdateVolumeLabels();
        RefreshKeybindLabels();
    }

    private IEnumerator StartGame()
    {
        isStarting = true;

        float timer = 0f;
        while (timer < 0.35f)
        {
            timer += Time.unscaledDeltaTime;
            menuGroup.alpha = 1f - timer / 0.35f;
            panelRect.localScale = Vector3.one * Mathf.Lerp(1f, 0.92f, timer / 0.35f);
            yield return null;
        }

        SceneManager.LoadScene(playSceneName);
    }

    private IEnumerator PlayIntro()
    {
        menuGroup.alpha = 0f;
        panelRect.anchoredPosition = new Vector2(0f, -45f);
        panelRect.localScale = Vector3.one * 0.92f;

        float timer = 0f;
        while (timer < introDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = EaseOutBack(Mathf.Clamp01(timer / introDuration));

            menuGroup.alpha = Mathf.Clamp01(timer / (introDuration * 0.65f));
            panelRect.anchoredPosition = Vector2.Lerp(new Vector2(0f, -45f), Vector2.zero, t);
            panelRect.localScale = Vector3.one * Mathf.Lerp(0.92f, 1f, t);
            yield return null;
        }

        menuGroup.alpha = 1f;
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.localScale = Vector3.one;
    }

    private void BuildMenu()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Main Menu Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }
        else
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        EnsureEventSystem();
        ClearCanvas(canvas.transform);

        background = CreateImage("Animated Background", canvas.transform, new Color(0.08f, 0.12f, 0.18f));
        Stretch(background.rectTransform);

        glow = CreateImage("Soft Center Glow", canvas.transform, new Color(0.22f, 0.82f, 1f, 0.14f));
        glow.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        glow.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        glow.rectTransform.sizeDelta = new Vector2(900f, 900f);
        glow.rectTransform.anchoredPosition = Vector2.zero;

        AddDecorLine(canvas.transform, new Vector2(-520f, 240f), 32f, new Color(1f, 0.78f, 0.22f, 0.75f));
        AddDecorLine(canvas.transform, new Vector2(520f, -255f), -28f, new Color(0.22f, 0.82f, 1f, 0.65f));
        AddDecorLine(canvas.transform, new Vector2(-650f, -310f), -8f, new Color(1f, 1f, 1f, 0.18f));

        GameObject root = new GameObject("Animated Main Menu", typeof(RectTransform), typeof(CanvasGroup));
        root.transform.SetParent(canvas.transform, false);
        panelRect = root.GetComponent<RectTransform>();
        Stretch(panelRect);
        menuGroup = root.GetComponent<CanvasGroup>();

        mainMenuPanel = new GameObject("Main Panel", typeof(RectTransform));
        mainMenuPanel.transform.SetParent(root.transform, false);
        Stretch(mainMenuPanel.GetComponent<RectTransform>());

        optionsPanel = new GameObject("Options Panel", typeof(RectTransform));
        optionsPanel.transform.SetParent(root.transform, false);
        Stretch(optionsPanel.GetComponent<RectTransform>());
        optionsPanel.SetActive(false);

        titleRect = CreateText("Title", mainMenuPanel.transform, gameTitle, 88, FontStyles.Bold, new Color(1f, 0.94f, 0.68f)).rectTransform;
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.sizeDelta = new Vector2(900f, 130f);
        titleRect.anchoredPosition = new Vector2(0f, 135f);

        TextMeshProUGUI subtitleText = CreateText("Subtitle", mainMenuPanel.transform, subtitle, 30, FontStyles.Normal, new Color(0.78f, 0.94f, 1f));
        subtitleText.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        subtitleText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        subtitleText.rectTransform.sizeDelta = new Vector2(720f, 60f);
        subtitleText.rectTransform.anchoredPosition = new Vector2(0f, 65f);

        VerticalLayoutGroup layout = CreateButtonStack(mainMenuPanel.transform);
        CreateButton("PLAY", layout.transform, Play, new Color(0.98f, 0.68f, 0.16f), new Color(1f, 0.85f, 0.32f));
        CreateButton("OPTIONS", layout.transform, OpenOptions, new Color(0.13f, 0.48f, 0.64f), new Color(0.22f, 0.82f, 1f));
        CreateButton("QUIT", layout.transform, Quit, new Color(0.13f, 0.48f, 0.64f), new Color(0.22f, 0.82f, 1f));

        BuildOptionsPanel(optionsPanel.transform);
        GameSettings.ApplyAudioSettings();
    }

    private VerticalLayoutGroup CreateButtonStack(Transform parent)
    {
        GameObject stack = new GameObject("Buttons", typeof(RectTransform), typeof(VerticalLayoutGroup));
        stack.transform.SetParent(parent, false);

        RectTransform rect = stack.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(320f, 245f);
        rect.anchoredPosition = new Vector2(0f, -120f);

        VerticalLayoutGroup layout = stack.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 18f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        return layout;
    }

    private void BuildOptionsPanel(Transform parent)
    {
        keyBindButtons.Clear();

        TextMeshProUGUI title = CreateText("Options Title", parent, "OPTIONS", 66, FontStyles.Bold, new Color(1f, 0.94f, 0.68f));
        title.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        title.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        title.rectTransform.sizeDelta = new Vector2(720f, 90f);
        title.rectTransform.anchoredPosition = new Vector2(0f, 235f);

        GameObject scrollView = new GameObject("Options Scroll View", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        scrollView.transform.SetParent(parent, false);
        RectTransform scrollRect = scrollView.GetComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0.5f, 0.5f);
        scrollRect.anchorMax = new Vector2(0.5f, 0.5f);
        scrollRect.sizeDelta = new Vector2(720f, 430f);
        scrollRect.anchoredPosition = new Vector2(0f, -15f);

        Image scrollBackground = scrollView.GetComponent<Image>();
        scrollBackground.color = new Color(0f, 0f, 0f, 0.01f);

        GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewport.transform.SetParent(scrollView.transform, false);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        Stretch(viewportRect);

        Image viewportImage = viewport.GetComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
        viewportImage.raycastTarget = true;

        Mask viewportMask = viewport.GetComponent<Mask>();
        viewportMask.showMaskGraphic = false;

        GameObject content = new GameObject("Options Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 1f);
        contentRect.anchorMax = new Vector2(0.5f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.sizeDelta = new Vector2(680f, 0f);
        contentRect.anchoredPosition = Vector2.zero;

        VerticalLayoutGroup contentLayout = content.GetComponent<VerticalLayoutGroup>();
        contentLayout.spacing = 14f;
        contentLayout.childAlignment = TextAnchor.UpperCenter;
        contentLayout.childControlHeight = false;
        contentLayout.childControlWidth = true;
        contentLayout.childForceExpandWidth = true;

        ContentSizeFitter contentFitter = content.GetComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect optionsScroll = scrollView.GetComponent<ScrollRect>();
        optionsScroll.viewport = viewportRect;
        optionsScroll.content = contentRect;
        optionsScroll.horizontal = false;
        optionsScroll.vertical = true;
        optionsScroll.movementType = ScrollRect.MovementType.Clamped;
        optionsScroll.scrollSensitivity = 34f;

        masterVolumeValue = CreateSliderRow(content.transform, "MASTER", GameSettings.MasterVolume, value =>
        {
            GameSettings.MasterVolume = value;
            GameSettings.ApplyAudioSettings();
            UpdateVolumeLabels();
        }, out masterVolumeSlider);

        musicVolumeValue = CreateSliderRow(content.transform, "MUSIC", GameSettings.MusicVolume, value =>
        {
            GameSettings.MusicVolume = value;
            GameSettings.ApplyAudioSettings();
            UpdateVolumeLabels();
        }, out musicVolumeSlider);

        sfxVolumeValue = CreateSliderRow(content.transform, "SFX", GameSettings.SfxVolume, value =>
        {
            GameSettings.SfxVolume = value;
            GameSettings.ApplyAudioSettings();
            UpdateVolumeLabels();
        }, out sfxVolumeSlider);

        AddOptionSpacer(content.transform, 8f);
        CreateKeybindRow(content.transform, "MOVE UP", GameSettings.MoveUpAction);
        CreateKeybindRow(content.transform, "MOVE DOWN", GameSettings.MoveDownAction);
        CreateKeybindRow(content.transform, "TURN LEFT", GameSettings.TurnLeftAction);
        CreateKeybindRow(content.transform, "TURN RIGHT", GameSettings.TurnRightAction);
        CreateKeybindRow(content.transform, "SHOOT", GameSettings.ShootAction);

        GameObject footer = new GameObject("Options Footer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        footer.transform.SetParent(parent, false);
        RectTransform footerRect = footer.GetComponent<RectTransform>();
        footerRect.anchorMin = new Vector2(0.5f, 0.5f);
        footerRect.anchorMax = new Vector2(0.5f, 0.5f);
        footerRect.sizeDelta = new Vector2(440f, 62f);
        footerRect.anchoredPosition = new Vector2(0f, -315f);

        HorizontalLayoutGroup footerLayout = footer.GetComponent<HorizontalLayoutGroup>();
        footerLayout.spacing = 18f;
        footerLayout.childAlignment = TextAnchor.MiddleCenter;
        footerLayout.childControlWidth = true;
        footerLayout.childControlHeight = true;
        footerLayout.childForceExpandWidth = true;

        CreateButton("RESET", footer.transform, ResetOptions, new Color(0.25f, 0.32f, 0.38f), new Color(0.45f, 0.55f, 0.62f));
        CreateButton("BACK", footer.transform, CloseOptions, new Color(0.98f, 0.68f, 0.16f), new Color(1f, 0.85f, 0.32f));

        UpdateVolumeLabels();
        RefreshKeybindLabels();
    }

    private TextMeshProUGUI CreateSliderRow(Transform parent, string label, float value, UnityEngine.Events.UnityAction<float> onChanged, out Slider slider)
    {
        GameObject row = CreateOptionRow(parent, label);

        slider = CreateSlider(row.transform, value);
        slider.onValueChanged.AddListener(onChanged);

        TextMeshProUGUI valueText = CreateText("Value", row.transform, string.Empty, 22, FontStyles.Bold, Color.white);
        valueText.rectTransform.sizeDelta = new Vector2(80f, 44f);

        return valueText;
    }

    private void SetSliderWithoutNotify(Slider slider, float value)
    {
        if (slider != null)
        {
            slider.SetValueWithoutNotify(value);
        }
    }

    private void CreateKeybindRow(Transform parent, string label, string actionName)
    {
        GameObject row = CreateOptionRow(parent, label);
        Button button = CreateSmallButton(row.transform, string.Empty, () => StartKeybind(actionName), new Color(0.13f, 0.48f, 0.64f), new Color(0.22f, 0.82f, 1f));
        keyBindButtons.Add(new KeyBindButton(actionName, button.GetComponentInChildren<TextMeshProUGUI>()));
    }

    private GameObject CreateOptionRow(Transform parent, string label)
    {
        GameObject row = new GameObject(label + " Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        row.transform.SetParent(parent, false);

        LayoutElement layout = row.GetComponent<LayoutElement>();
        layout.preferredHeight = 44f;

        HorizontalLayoutGroup group = row.GetComponent<HorizontalLayoutGroup>();
        group.spacing = 14f;
        group.childAlignment = TextAnchor.MiddleCenter;
        group.childControlHeight = true;
        group.childControlWidth = false;

        TextMeshProUGUI labelText = CreateText("Label", row.transform, label, 22, FontStyles.Bold, new Color(0.78f, 0.94f, 1f));
        labelText.alignment = TextAlignmentOptions.MidlineLeft;
        labelText.rectTransform.sizeDelta = new Vector2(170f, 44f);

        return row;
    }

    private Slider CreateSlider(Transform parent, float value)
    {
        GameObject sliderObject = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
        sliderObject.transform.SetParent(parent, false);
        sliderObject.GetComponent<RectTransform>().sizeDelta = new Vector2(350f, 44f);

        Image backgroundImage = CreateImage("Background", sliderObject.transform, new Color(1f, 1f, 1f, 0.18f));
        backgroundImage.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        backgroundImage.rectTransform.anchorMax = new Vector2(1f, 0.5f);
        backgroundImage.rectTransform.sizeDelta = new Vector2(0f, 10f);
        backgroundImage.rectTransform.anchoredPosition = Vector2.zero;

        Image fillImage = CreateImage("Fill", sliderObject.transform, new Color(0.98f, 0.68f, 0.16f));
        fillImage.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        fillImage.rectTransform.anchorMax = new Vector2(0f, 0.5f);
        fillImage.rectTransform.sizeDelta = new Vector2(0f, 10f);
        fillImage.rectTransform.anchoredPosition = Vector2.zero;

        Image handleImage = CreateImage("Handle", sliderObject.transform, Color.white);
        handleImage.raycastTarget = true;
        handleImage.rectTransform.sizeDelta = new Vector2(24f, 24f);

        Slider slider = sliderObject.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = value;
        slider.fillRect = fillImage.rectTransform;
        slider.handleRect = handleImage.rectTransform;
        slider.targetGraphic = handleImage;
        slider.direction = Slider.Direction.LeftToRight;

        return slider;
    }

    private Button CreateSmallButton(Transform parent, string label, UnityEngine.Events.UnityAction action, Color normal, Color highlighted)
    {
        GameObject buttonObject = new GameObject(label + " Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(MenuButtonHover));
        buttonObject.transform.SetParent(parent, false);
        buttonObject.GetComponent<RectTransform>().sizeDelta = new Vector2(170f, 44f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = normal;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);
        button.colors = CreateButtonColors(normal, highlighted);

        MenuButtonHover hover = buttonObject.GetComponent<MenuButtonHover>();
        hover.Setup(buttonObject.GetComponent<RectTransform>());

        TextMeshProUGUI text = CreateText("Label", buttonObject.transform, label, 22, FontStyles.Bold, Color.white);
        Stretch(text.rectTransform);

        return button;
    }

    private void AddOptionSpacer(Transform parent, float height)
    {
        GameObject spacer = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
        spacer.transform.SetParent(parent, false);
        spacer.GetComponent<LayoutElement>().preferredHeight = height;
    }

    private void StartKeybind(string actionName)
    {
        waitingForKeyAction = actionName;
        RefreshKeybindLabels();
    }

    private void ListenForKeybind()
    {
        if (string.IsNullOrEmpty(waitingForKeyAction))
        {
            return;
        }

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current == null)
        {
            return;
        }

        foreach (var key in Keyboard.current.allKeys)
        {
            if (!key.wasPressedThisFrame)
            {
                continue;
            }

            if (key.keyCode == Key.Escape)
            {
                waitingForKeyAction = null;
                RefreshKeybindLabels();
                return;
            }

            GameSettings.SetKey(waitingForKeyAction, key.keyCode);
            waitingForKeyAction = null;
            RefreshKeybindLabels();
            return;
        }
#endif
    }

    private void UpdateVolumeLabels()
    {
        if (masterVolumeValue != null)
        {
            masterVolumeValue.text = Mathf.RoundToInt(GameSettings.MasterVolume * 100f) + "%";
        }

        if (musicVolumeValue != null)
        {
            musicVolumeValue.text = Mathf.RoundToInt(GameSettings.MusicVolume * 100f) + "%";
        }

        if (sfxVolumeValue != null)
        {
            sfxVolumeValue.text = Mathf.RoundToInt(GameSettings.SfxVolume * 100f) + "%";
        }
    }

    private void RefreshKeybindLabels()
    {
        foreach (KeyBindButton keyBindButton in keyBindButtons)
        {
            if (keyBindButton.Label == null)
            {
                continue;
            }

            keyBindButton.Label.text = waitingForKeyAction == keyBindButton.ActionName
                ? "PRESS KEY"
                : GameSettings.GetKeyName(keyBindButton.ActionName);
        }
    }

    private void CreateButton(string label, Transform parent, UnityEngine.Events.UnityAction action, Color normal, Color highlighted)
    {
        GameObject buttonObject = new GameObject(label + " Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement), typeof(MenuButtonHover));
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.GetComponent<Image>();
        image.color = normal;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);
        button.transition = Selectable.Transition.ColorTint;
        button.colors = CreateButtonColors(normal, highlighted);

        LayoutElement layout = buttonObject.GetComponent<LayoutElement>();
        layout.preferredHeight = 62f;

        MenuButtonHover hover = buttonObject.GetComponent<MenuButtonHover>();
        hover.Setup(buttonObject.GetComponent<RectTransform>());

        TextMeshProUGUI text = CreateText("Label", buttonObject.transform, label, 34, FontStyles.Bold, Color.white);
        Stretch(text.rectTransform);
    }

    private ColorBlock CreateButtonColors(Color normal, Color highlighted)
    {
        return new ColorBlock
        {
            normalColor = normal,
            highlightedColor = highlighted,
            pressedColor = Color.white,
            selectedColor = highlighted,
            disabledColor = new Color(0.45f, 0.45f, 0.45f),
            colorMultiplier = 1f,
            fadeDuration = 0.12f
        };
    }

    private TextMeshProUGUI CreateText(string objectName, Transform parent, string value, int size, FontStyles style, Color color)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;

        return text;
    }

    private Image CreateImage(string objectName, Transform parent, Color color)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;

        return image;
    }

    private void AddDecorLine(Transform parent, Vector2 position, float rotation, Color color)
    {
        Image line = CreateImage("Decor Line", parent, color);
        line.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        line.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        line.rectTransform.sizeDelta = new Vector2(420f, 8f);
        line.rectTransform.anchoredPosition = position;
        line.rectTransform.localEulerAngles = new Vector3(0f, 0f, rotation);
    }

    private void ClearCanvas(Transform canvasTransform)
    {
        for (int i = canvasTransform.childCount - 1; i >= 0; i--)
        {
            Transform child = canvasTransform.GetChild(i);
            if (child == transform)
            {
                continue;
            }

            Destroy(child.gameObject);
        }
    }

    private void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

#if ENABLE_INPUT_SYSTEM
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
#else
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
#endif
    }

    private void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}

public class MenuButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform target;
    private Coroutine animationRoutine;

    public void Setup(RectTransform rectTransform)
    {
        target = rectTransform;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        AnimateTo(1.08f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        AnimateTo(1f);
    }

    private void AnimateTo(float scale)
    {
        if (target == null)
        {
            target = GetComponent<RectTransform>();
        }

        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
        }

        animationRoutine = StartCoroutine(ScaleTo(scale));
    }

    private IEnumerator ScaleTo(float scale)
    {
        Vector3 startScale = target.localScale;
        Vector3 endScale = Vector3.one * scale;

        float timer = 0f;
        while (timer < 0.12f)
        {
            timer += Time.unscaledDeltaTime;
            target.localScale = Vector3.Lerp(startScale, endScale, timer / 0.12f);
            yield return null;
        }

        target.localScale = endScale;
    }
}

public struct KeyBindButton
{
    public KeyBindButton(string actionName, TextMeshProUGUI label)
    {
        ActionName = actionName;
        Label = label;
    }

    public string ActionName { get; }
    public TextMeshProUGUI Label { get; }
}

public static class GameSettings
{
    public const string MoveUpAction = "MoveUp";
    public const string MoveDownAction = "MoveDown";
    public const string TurnLeftAction = "TurnLeft";
    public const string TurnRightAction = "TurnRight";
    public const string ShootAction = "Shoot";

    private const string MasterVolumeKey = "Settings.MasterVolume";
    private const string MusicVolumeKey = "Settings.MusicVolume";
    private const string SfxVolumeKey = "Settings.SfxVolume";
    private const string BgmMixerParameter = "BGMVolume";
    private const string SfxMixerParameter = "SFXVolume";

    public static float MasterVolume
    {
        get => PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        set
        {
            PlayerPrefs.SetFloat(MasterVolumeKey, Mathf.Clamp01(value));
            PlayerPrefs.Save();
        }
    }

    public static float MusicVolume
    {
        get => PlayerPrefs.GetFloat(MusicVolumeKey, 0.8f);
        set
        {
            PlayerPrefs.SetFloat(MusicVolumeKey, Mathf.Clamp01(value));
            PlayerPrefs.Save();
        }
    }

    public static float SfxVolume
    {
        get => PlayerPrefs.GetFloat(SfxVolumeKey, 0.8f);
        set
        {
            PlayerPrefs.SetFloat(SfxVolumeKey, Mathf.Clamp01(value));
            PlayerPrefs.Save();
        }
    }

    public static void ApplyAudioSettings()
    {
        AudioListener.volume = MasterVolume;

        AudioMixer mixer = FindGameAudioMixer();
        if (mixer == null)
        {
            return;
        }

        mixer.SetFloat(BgmMixerParameter, VolumeToDb(MusicVolume));
        mixer.SetFloat("MyExposedParam", VolumeToDb(MusicVolume));
        mixer.SetFloat(SfxMixerParameter, VolumeToDb(SfxVolume));
        mixer.SetFloat("SFX", VolumeToDb(SfxVolume));
        mixer.SetFloat("SfxVolume", VolumeToDb(SfxVolume));
    }

    private static AudioMixer FindGameAudioMixer()
    {
        AudioSource[] sources = UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource source in sources)
        {
            if (source.outputAudioMixerGroup == null)
            {
                continue;
            }

            AudioMixer mixer = source.outputAudioMixerGroup.audioMixer;
            if (mixer != null && mixer.name == "GameAudioMixer")
            {
                return mixer;
            }
        }

        return null;
    }

    private static float VolumeToDb(float volume)
    {
        return Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
    }

#if ENABLE_INPUT_SYSTEM
    public static Key MoveUpKey => GetKey(MoveUpAction, Key.W);
    public static Key MoveDownKey => GetKey(MoveDownAction, Key.S);
    public static Key TurnLeftKey => GetKey(TurnLeftAction, Key.A);
    public static Key TurnRightKey => GetKey(TurnRightAction, Key.D);
    public static Key ShootKey => GetKey(ShootAction, Key.Space);

    public static void SetKey(string actionName, Key key)
    {
        PlayerPrefs.SetString(GetKeyPrefsName(actionName), key.ToString());
        PlayerPrefs.Save();
    }

    public static Key GetKey(string actionName, Key defaultKey)
    {
        string savedValue = PlayerPrefs.GetString(GetKeyPrefsName(actionName), defaultKey.ToString());
        return System.Enum.TryParse(savedValue, out Key key) ? key : defaultKey;
    }

    public static string GetKeyName(string actionName)
    {
        switch (actionName)
        {
            case MoveUpAction:
                return MoveUpKey.ToString().ToUpperInvariant();
            case MoveDownAction:
                return MoveDownKey.ToString().ToUpperInvariant();
            case TurnLeftAction:
                return TurnLeftKey.ToString().ToUpperInvariant();
            case TurnRightAction:
                return TurnRightKey.ToString().ToUpperInvariant();
            case ShootAction:
                return ShootKey.ToString().ToUpperInvariant();
            default:
                return "UNSET";
        }
    }
#else
    public static string GetKeyName(string actionName)
    {
        return "INPUT SYSTEM";
    }
#endif

    public static void ResetToDefaults()
    {
        PlayerPrefs.DeleteKey(MasterVolumeKey);
        PlayerPrefs.DeleteKey(MusicVolumeKey);
        PlayerPrefs.DeleteKey(SfxVolumeKey);
        PlayerPrefs.DeleteKey(GetKeyPrefsName(MoveUpAction));
        PlayerPrefs.DeleteKey(GetKeyPrefsName(MoveDownAction));
        PlayerPrefs.DeleteKey(GetKeyPrefsName(TurnLeftAction));
        PlayerPrefs.DeleteKey(GetKeyPrefsName(TurnRightAction));
        PlayerPrefs.DeleteKey(GetKeyPrefsName(ShootAction));
        PlayerPrefs.Save();
    }

    private static string GetKeyPrefsName(string actionName)
    {
        return "Settings.Key." + actionName;
    }
}
