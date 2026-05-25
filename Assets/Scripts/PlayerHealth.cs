using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PlayerHealth : MonoBehaviour
{
    public int health = 5;
    private bool isGameOver;
    private string sceneName = "MenuScene";
    [SerializeField] private GameObject shieldCircle;
    [SerializeField] private UISliderUpdater1 sliderUI;
    private bool shieldActive;
    private Coroutine shieldCoroutine;
    [SerializeField] private AudioClip sfxDamage;

    private void Start()
    {
        if (shieldCircle != null)
        {
            shieldCircle.SetActive(false);
        }

    }
    private void Update()
    {
        if (!isGameOver)
        {
            return;
        }

        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
        }
    }
    public void TakeDamage(int damage)
    {
        if (shieldActive)
        {
            return;
        }
        BulletController.ResetToNormalBullet();
        health -= damage;
        if (sliderUI != null)
        {
            sliderUI.SetSliderValue(health);
        }

      if (health <= 0 && !isGameOver)
{
            isGameOver = true;
            Time.timeScale = 0f;
            ShowGameOverText();

            GameObject hitSfx = GameObject.Find("Sound Effects Player");

    if (hitSfx != null &&
        sfxDamage != null &&
        hitSfx.TryGetComponent<AudioSource>(out AudioSource source))
    {
        source.PlayOneShot(sfxDamage);
    }

            DisableTankForGameOver();

        }
    }
    private void ShowGameOverText()
    {
        GameObject canvasObject = new GameObject("Game Over Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject textObject = new GameObject("Game Over Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(canvasObject.transform, false);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = "GAME OVER\n Try again?\nHint: 'R' ";
        text.color = Color.red;
        text.fontSize = 120f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;

        RectTransform rect = text.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(900f, 200f);
        rect.localScale = Vector3.one;
    }

    public void ActivateShield(float duration)
    {
        if (shieldCoroutine != null)
        {
            StopCoroutine(shieldCoroutine);
        }

        shieldCoroutine = StartCoroutine(ShieldTimer(duration));
    }

    private System.Collections.IEnumerator ShieldTimer(float duration)
    {
        shieldActive = true;

        if (shieldCircle != null)
        {
            shieldCircle.SetActive(true);
        }

        yield return new WaitForSeconds(duration);

        shieldActive = false;

        if (shieldCircle != null)
        {
            shieldCircle.SetActive(false);
        }

        shieldCoroutine = null;
    }
    private void DisableTankForGameOver()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.enabled = false;
        }

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        MovementController movement = GetComponent<MovementController>();
        if (movement != null)
        {
            movement.enabled = false;
        }

        TankShoot shoot = GetComponent<TankShoot>();
        if (shoot != null)
        {
            shoot.enabled = false;
        }

        TankTurret turret = GetComponent<TankTurret>();
        if (turret != null)
        {
            turret.enabled = false;
        }
    }
}
