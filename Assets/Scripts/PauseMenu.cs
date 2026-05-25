using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MenuScene";

    private CanvasGroup overlayGroup;
    private GameObject pausePanel;
    private GameObject optionsPanel;
    private Button pauseButton;
    private TextMeshProUGUI masterVolumeValue;
    private TextMeshProUGUI musicVolumeValue;
    private TextMeshProUGUI sfxVolumeValue;
    private Slider masterVolumeSlider;
    private Slider musicVolumeSlider;
    private Slider sfxVolumeSlider;
    private readonly List<KeyBindButton> keyBindButtons = new List<KeyBindButton>();
    private string waitingForKeyAction;
    private bool isPaused;
    [SerializeField] private GameObject tank;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RegisterSceneHook()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        HandleSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Scene1" && scene.name != "Scene2"&&scene.name!="Scene3")
        {
            Time.timeScale = 1f;
            return;
        }

        if (FindFirstObjectByType<PauseMenu>() != null)
        {
            return;
        }

        new GameObject("Pause Menu Controller", typeof(PauseMenu));
    }

    private void Awake()
    {
        tank = GameObject.Find("Tank");
        BuildPauseMenu();
        GameSettings.ApplyAudioSettings();
        SetPaused(false);
    }

    private void Update()
    {
        bool wasWaitingForKey = !string.IsNullOrEmpty(waitingForKeyAction);
        ListenForKeybind();

        if (wasWaitingForKey)
        {
            return;
        }

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (optionsPanel.activeSelf)
            {
                OpenPausePanel();
                return;
            }

            SetPaused(!isPaused);
        }
#endif
    }

    public void Resume()
    {

        SetPaused(false);
    }

    public void OpenOptions()
    {
        pausePanel.SetActive(false);
        optionsPanel.SetActive(true);
        RefreshKeybindLabels();
    }

    public void OpenPausePanel()
    {
        SetTankControlsEnabled(false);
        waitingForKeyAction = null;
        optionsPanel.SetActive(false);
        pausePanel.SetActive(true);
        RefreshKeybindLabels();
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void Quit()
    {
        Time.timeScale = 1f;
        Application.Quit();
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

    private void SetPaused(bool paused)
    {
        isPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
        pauseButton.gameObject.SetActive(!paused);
        overlayGroup.alpha = paused ? 1f : 0f;
        overlayGroup.interactable = paused;
        overlayGroup.blocksRaycasts = paused;
        SetTankControlsEnabled(!paused);

        if (paused)
        {
            OpenPausePanel();
        }
        else
        {
            waitingForKeyAction = null;
            optionsPanel.SetActive(false);
            pausePanel.SetActive(true);
        }
    }

    private void SetTankControlsEnabled(bool enabled)
    {
        if (tank == null)
        {
            tank = GameObject.Find("Tank");
        }

        if (tank == null)
        {
            return;
        }

        MovementController movement = tank.GetComponent<MovementController>();
        if (movement != null)
        {
            movement.enabled = enabled;
        }

        TankShoot shoot = tank.GetComponentInChildren<TankShoot>();
        if (shoot != null)
        {
            shoot.enabled = enabled;
        }

        TankTurret turret = tank.GetComponentInChildren<TankTurret>();
        if (turret != null)
        {
            turret.enabled = enabled;
        }

        TankHeadlight headlight = tank.GetComponentInChildren<TankHeadlight>();
        if (headlight != null)
        {
            headlight.enabled = false;
        }
    }

    private void BuildPauseMenu()
    {
        Canvas canvas = CreateCanvas();
        EnsureEventSystem();

        GameObject pauseButtonObject = new GameObject("Pause Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(MenuButtonHover));
        pauseButtonObject.transform.SetParent(canvas.transform, false);
        RectTransform pauseButtonRect = pauseButtonObject.GetComponent<RectTransform>();
        pauseButtonRect.anchorMin = new Vector2(1f, 1f);
        pauseButtonRect.anchorMax = new Vector2(1f, 1f);
        pauseButtonRect.pivot = new Vector2(1f, 1f);
        pauseButtonRect.sizeDelta = new Vector2(92f, 52f);
        pauseButtonRect.anchoredPosition = new Vector2(-22f, -22f);

        Image pauseButtonImage = pauseButtonObject.GetComponent<Image>();
        pauseButtonImage.color = new Color(0.08f, 0.12f, 0.18f, 0.88f);

        pauseButton = pauseButtonObject.GetComponent<Button>();
        pauseButton.targetGraphic = pauseButtonImage;
        pauseButton.onClick.AddListener(() => SetPaused(true));
        pauseButton.colors = CreateButtonColors(new Color(0.08f, 0.12f, 0.18f, 0.88f), new Color(0.22f, 0.82f, 1f, 0.94f));

        MenuButtonHover pauseHover = pauseButtonObject.GetComponent<MenuButtonHover>();
        pauseHover.Setup(pauseButtonRect);

        TextMeshProUGUI pauseText = CreateText("Label", pauseButtonObject.transform, "II", 28, FontStyles.Bold, Color.white);
        Stretch(pauseText.rectTransform);

        GameObject overlay = new GameObject("Pause Overlay", typeof(RectTransform), typeof(CanvasGroup));
        overlay.transform.SetParent(canvas.transform, false);
        Stretch(overlay.GetComponent<RectTransform>());
        overlayGroup = overlay.GetComponent<CanvasGroup>();

        Image shade = CreateImage("Shade", overlay.transform, new Color(0.02f, 0.04f, 0.07f, 0.82f));
        Stretch(shade.rectTransform);

        pausePanel = new GameObject("Pause Panel", typeof(RectTransform));
        pausePanel.transform.SetParent(overlay.transform, false);
        Stretch(pausePanel.GetComponent<RectTransform>());

        optionsPanel = new GameObject("Pause Options Panel", typeof(RectTransform));
        optionsPanel.transform.SetParent(overlay.transform, false);
        Stretch(optionsPanel.GetComponent<RectTransform>());
        optionsPanel.SetActive(false);

        BuildPausePanel(pausePanel.transform);
        BuildOptionsPanel(optionsPanel.transform);
    }

    private void BuildPausePanel(Transform parent)
    {
        TextMeshProUGUI title = CreateText("Pause Title", parent, "PAUSED", 76, FontStyles.Bold, new Color(1f, 0.94f, 0.68f));
        title.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        title.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        title.rectTransform.sizeDelta = new Vector2(700f, 96f);
        title.rectTransform.anchoredPosition = new Vector2(0f, 190f);

        VerticalLayoutGroup layout = CreateButtonStack(parent, new Vector2(360f, 330f), new Vector2(0f, -30f));
        CreateButton("RESUME", layout.transform, Resume, new Color(0.98f, 0.68f, 0.16f), new Color(1f, 0.85f, 0.32f));
        CreateButton("OPTIONS", layout.transform, OpenOptions, new Color(0.13f, 0.48f, 0.64f), new Color(0.22f, 0.82f, 1f));
        CreateButton("MAIN MENU", layout.transform, ReturnToMainMenu, new Color(0.25f, 0.32f, 0.38f), new Color(0.45f, 0.55f, 0.62f));
        CreateButton("QUIT", layout.transform, Quit, new Color(0.45f, 0.18f, 0.18f), new Color(0.85f, 0.28f, 0.22f));
    }

    private void BuildOptionsPanel(Transform parent)
    {
        keyBindButtons.Clear();

        TextMeshProUGUI title = CreateText("Options Title", parent, "OPTIONS", 62, FontStyles.Bold, new Color(1f, 0.94f, 0.68f));
        title.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        title.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        title.rectTransform.sizeDelta = new Vector2(720f, 84f);
        title.rectTransform.anchoredPosition = new Vector2(0f, 245f);

        GameObject scrollView = new GameObject("Options Scroll View", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        scrollView.transform.SetParent(parent, false);
        RectTransform scrollRect = scrollView.GetComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0.5f, 0.5f);
        scrollRect.anchorMax = new Vector2(0.5f, 0.5f);
        scrollRect.sizeDelta = new Vector2(720f, 410f);
        scrollRect.anchoredPosition = new Vector2(0f, -10f);

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
        footerRect.anchoredPosition = new Vector2(0f, -305f);

        HorizontalLayoutGroup footerLayout = footer.GetComponent<HorizontalLayoutGroup>();
        footerLayout.spacing = 18f;
        footerLayout.childAlignment = TextAnchor.MiddleCenter;
        footerLayout.childControlWidth = true;
        footerLayout.childControlHeight = true;
        footerLayout.childForceExpandWidth = true;

        CreateButton("RESET", footer.transform, ResetOptions, new Color(0.25f, 0.32f, 0.38f), new Color(0.45f, 0.55f, 0.62f));
        CreateButton("BACK", footer.transform, OpenPausePanel, new Color(0.98f, 0.68f, 0.16f), new Color(1f, 0.85f, 0.32f));

        UpdateVolumeLabels();
        RefreshKeybindLabels();
    }

    private Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Pause Menu Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        return canvas;
    }

    private VerticalLayoutGroup CreateButtonStack(Transform parent, Vector2 size, Vector2 position)
    {
        GameObject stack = new GameObject("Buttons", typeof(RectTransform), typeof(VerticalLayoutGroup));
        stack.transform.SetParent(parent, false);

        RectTransform rect = stack.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        VerticalLayoutGroup layout = stack.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 18f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        return layout;
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

    private void SetSliderWithoutNotify(Slider slider, float value)
    {
        if (slider != null)
        {
            slider.SetValueWithoutNotify(value);
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

        TextMeshProUGUI text = CreateText("Label", buttonObject.transform, label, 32, FontStyles.Bold, Color.white);
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
}
