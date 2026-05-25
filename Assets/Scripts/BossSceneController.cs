using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class BossSceneController : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int playerMaxHealth = 100;
    [SerializeField] private int bossMaxHealth = 500;

    [Header("Boss")]
    [SerializeField] private float bossSpinSpeed = 95f;
    [SerializeField] private float bulletInterval = 1.05f;
    [SerializeField] private float laserInterval = 3.2f;

    [Header("Player")]
    [SerializeField] private GameObject tankPrefab;
    
   
    [SerializeField] private string arenaName = "Arena";
    [SerializeField] private float startScaleMultiplier = 0.08f;
    [SerializeField] private float overshootMultiplier = 1.05f;
    [SerializeField] private float expandDuration = 1.2f;

    private Transform player;
  
    private Rigidbody2D playerRigidbody;
    private PlayerHealth playerHealthComponent;
    private Transform boss;
    private Image playerHealthFill;
    private Image bossHealthFill;
    private TextMeshProUGUI bannerText;
  
    private int playerHealth;
    private int bossHealth;
    private bool bossDead;
    private bool laserPhase;
    private bool bossWinStarted;
    private float winTimer;
 
    private Camera mainCamera;
    private Sprite squareSprite;
    private Sprite circleSprite;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RegisterSceneHook()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        HandleSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Scene3")
        {
            return;
        }

        if (FindFirstObjectByType<BossSceneController>() != null)
        {
            return;
        }

        new GameObject("Boss Scene Controller", typeof(BossSceneController));
    }

    private void Awake()
    {
        playerHealth = playerMaxHealth;
        bossHealth = bossMaxHealth;
        mainCamera = Camera.main;

        ConfigureCamera();
        BuildArena();
        BuildUi();
        SpawnPlayer();
        SpawnBoss();
        UpdateHealthBars();
    }

    private void Start()
    {
        StartCoroutine(ShowBanner("BOSS APPEARED", 2.2f, true));
        StartCoroutine(BossBulletRoutine());
        StartCoroutine(BossLaserRoutine());
        StartCoroutine(ExpandArenaOnStart());
    }

    private IEnumerator ExpandArenaOnStart()
    {
        if (SceneManager.GetActiveScene().name != "Scene3")
        {
            yield break;
        }

        GameObject arena = GameObject.Find(arenaName);
        if (arena == null)
        {
            Debug.LogWarning("BossSceneController could not find an object named " + arenaName);
            yield break;
        }

        Transform arenaTransform = arena.transform;
        Vector3 finalScale = arenaTransform.localScale;
        Vector3 startScale = finalScale * startScaleMultiplier;
        Vector3 overshootScale = finalScale * overshootMultiplier;

        arenaTransform.localScale = startScale;

        float timer = 0f;
        while (timer < expandDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / expandDuration);
            float eased = EaseOutBack(t);

            arenaTransform.localScale = Vector3.LerpUnclamped(startScale, overshootScale, eased);
            yield return null;
        }

        timer = 0f;
        while (timer < 0.18f)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / 0.18f);

            arenaTransform.localScale = Vector3.Lerp(overshootScale, finalScale, t);
            yield return null;
        }

        arenaTransform.localScale = finalScale;
    }
    private void Update()
    {
        if (bossHealth <= 0 && !bossWinStarted)
        {
            bossWinStarted = true;
            bossDead = true;
            winTimer = 0f;

            if (boss != null)
            {
                Destroy(boss.gameObject);
            }

            DestroyBossAttacks();
            StartCoroutine(ShowBanner("BOSS DEAD - YOU WIN!", 3f, true));
        }

        if (bossWinStarted)
        {
            winTimer += Time.deltaTime;

            if (winTimer >= 3f)
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene("MenuScene");
            }

            return;
        }

        if (bossDead)
        {
            return;
        }

        if (boss != null)
        {
            boss.Rotate(0f, 0f, bossSpinSpeed * Time.deltaTime);
        }

        SyncPlayerHealthBar();
    }

    public void DamageBoss(int damage)
    {
        if (bossDead)
        {
            return;
        }

        bossHealth = Mathf.Max(0, bossHealth - damage);
        UpdateHealthBars();
        Debug.Log("Boss health: " + bossHealth + " / " + bossMaxHealth);

        if (!laserPhase && bossHealth <= bossMaxHealth / 2)
        {
            laserPhase = true;
            StartCoroutine(ShowBanner("LASER PHASE", 1.4f, false));
        }
    }

    public void DamagePlayer(int damage)
    {
        if (bossDead)
        {
            return;
        }

        if (playerHealthComponent != null)
        {
            playerHealth = Mathf.Clamp(playerHealthComponent.health, 0, playerMaxHealth);
        }
        else
        {
            playerHealth = Mathf.Max(0, playerHealth - damage);
        }

        UpdateHealthBars();

        if (playerHealth <= 0)
        {
           
            UpdateHealthBars();
        }
    }
  
      

    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;

        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
    private IEnumerator WinFight()
    {
        bossDead = true;

        if (boss != null)
        {
            Destroy(boss.gameObject);
        }

        DestroyBossAttacks();
        yield return ShowBanner("BOSS DEAD - YOU WIN!", 999f, true);
    }

    private IEnumerator BossBulletRoutine()
    {
        yield return new WaitForSeconds(1.8f);

        while (!bossDead)
        {
            FireRadialBossBullets(laserPhase ? 16 : 10);
            yield return new WaitForSeconds(laserPhase ? bulletInterval * 0.65f : bulletInterval);
        }
    }

    private IEnumerator BossLaserRoutine()
    {
        while (!bossDead)
        {
            yield return new WaitForSeconds(laserInterval);

            if (laserPhase && !bossDead)
            {
                FireBossLasers();
            }
        }
    }

    
    private void FireRadialBossBullets(int count)
    {
        if (boss == null)
        {
            return;
        }

        float angleStep = 360f / count;
        float offset = Time.time * 38f;

        for (int i = 0; i < count; i++)
        {
            float angle = (angleStep * i + offset) * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            GameObject bullet = CreateSpriteObject("Boss Bullet", boss.position + (Vector3)(direction * 0.9f), new Vector2(0.24f, 0.24f), new Color(1f, 0.28f, 0.15f), true);

            CircleCollider2D collider = bullet.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.18f;

            BossProjectile projectile = bullet.AddComponent<BossProjectile>();
            projectile.Setup(direction, laserPhase ? 4.4f : 3.6f, 8, false, 6f);
        }
    }

    private void FireBossLasers()
    {
        if (boss == null || player == null)
        {
            return;
        }

        Vector2 directionToPlayer = (player.position - boss.position).normalized;
        for (int i = -1; i <= 1; i++)
        {
            Vector2 direction = Quaternion.Euler(0f, 0f, i * 18f) * directionToPlayer;
            GameObject laser = CreateSpriteObject("Boss Laser", boss.position + (Vector3)(direction * 1.2f), new Vector2(0.34f, 4.2f), new Color(0.95f, 0.08f, 0.08f, 0.9f));
            laser.transform.up = direction;

            BoxCollider2D collider = laser.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(0.9f, 4.2f);

            BossProjectile projectile = laser.AddComponent<BossProjectile>();
            projectile.Setup(direction, 6.5f, 18, false, 3.5f);
        }
    }

    private void SpawnPlayer()
    {
        GameObject resolvedTankPrefab = ResolveTankPrefab();
        GameObject playerObject = resolvedTankPrefab != null
            ? Instantiate(resolvedTankPrefab, new Vector3(0f, -9.09f, 0f), Quaternion.identity)
            : GameObject.Find("Tank");

        if (playerObject == null)
        {
            Debug.LogError("BossSceneController needs the Scene1 Tank prefab assigned to tankPrefab.");
            enabled = false;
            return;
        }

        playerObject.name = "Tank";
        playerObject.transform.position = new Vector3(0f, -9.09f, 0f);
        playerObject.transform.rotation = Quaternion.identity;
        player = playerObject.transform;
        AssignMainCameraToTankTurret(playerObject);
        playerRigidbody = playerObject.GetComponent<Rigidbody2D>();
        playerHealthComponent = playerObject.GetComponent<PlayerHealth>();

        if (playerHealthComponent != null)
        {
            playerHealthComponent.health = playerMaxHealth;
        }

        TankHeadlight headlight = player.GetComponent<TankHeadlight>();
        if (headlight != null)
        {
            headlight.enabled = false;
        }
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
            playerRigidbody.angularVelocity = 0f;
        }

        Collider2D tankCollider = playerObject.GetComponent<Collider2D>();
        if (tankCollider == null)
        {
            BoxCollider2D collider = playerObject.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.9f, 1.2f);
        }

      
    }

    private void AssignMainCameraToTankTurret(GameObject tank)
    {
        TankTurret turret = tank.GetComponent<TankTurret>();
        if (turret == null)
        {
            turret = tank.GetComponentInChildren<TankTurret>(true);
        }

        if (turret == null)
        {
            Debug.LogWarning("TankTurret was not found on the Tank.");
            return;
        }

        Camera cameraToUse = Camera.main;
        if (cameraToUse == null)
        {
            cameraToUse = FindFirstObjectByType<Camera>();
        }

        if (cameraToUse == null)
        {
            Debug.LogWarning("No Camera found to assign to TankTurret.");
            return;
        }

        turret.SetCamera(cameraToUse);
    }
    private GameObject ResolveTankPrefab()
    {
        if (tankPrefab != null)
        {
            return tankPrefab;
        }

#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Tank.prefab");
#else
        return Resources.Load<GameObject>("Tank");
#endif
    }

 
    private Transform FindChildByName(Transform parent, string childName)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == childName)
            {
                return child;
            }
        }

        return null;
    }

    private void SpawnBoss()
    {
        GameObject bossObject = new GameObject("Sun Circle Boss");
        bossObject.transform.position = new Vector3(0f, 2.15f, 0f);
        boss = bossObject.transform;

        GameObject core = CreateSpriteObject("Boss Core", boss.position, new Vector2(1.6f, 1.6f), new Color(1f, 0.48f, 0.12f), true);
        core.transform.SetParent(boss, true);

        CircleCollider2D collider = core.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.82f;
        core.AddComponent<BossHitbox>().Setup(this);

        for (int i = 0; i < 14; i++)
        {
            float angle = i * (360f / 14f);
            Vector3 direction = Quaternion.Euler(0f, 0f, angle) * Vector3.up;
            GameObject ray = CreateSpriteObject("Boss Ray", boss.position + direction * 1.12f, new Vector2(0.24f, 1.15f), new Color(1f, 0.82f, 0.18f));
            ray.transform.SetParent(boss, true);
            ray.transform.up = direction;
        }
    }

    private void BuildArena()
    {
        GameObject background = CreateSpriteObject("Boss Arena Background", Vector3.zero, new Vector2(18f, 10f), new Color(0.05f, 0.07f, 0.12f));
        background.GetComponent<SpriteRenderer>().sortingOrder = -20;

        for (int i = 0; i < 24; i++)
        {
            float x = -8.5f + i * 0.75f;
            GameObject star = CreateSpriteObject("Star", new Vector3(x, Random.Range(-4f, 4.5f), 0f), Vector2.one * Random.Range(0.04f, 0.09f), new Color(0.6f, 0.9f, 1f, 0.7f), true);
            star.GetComponent<SpriteRenderer>().sortingOrder = -10;
        }
    }

    private void BuildUi()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Boss Fight Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        CanvasScaler existingScaler = canvas.GetComponent<CanvasScaler>();
        if (existingScaler != null)
        {
            existingScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            existingScaler.referenceResolution = new Vector2(1920f, 1080f);
            existingScaler.matchWidthOrHeight = 0.5f;
        }

        playerHealthFill = CreateHealthBar(canvas.transform, "PLAYER", new Vector2(-320f, -80f), new Color(0.18f, 0.82f, 1f));
        bossHealthFill = CreateHealthBar(canvas.transform, "BOSS", new Vector2(320f, -80f), new Color(1f, 0.28f, 0.15f));

        bannerText = CreateUiText("Boss Banner", canvas.transform, string.Empty, 58, FontStyles.Bold, new Color(1f, 0.92f, 0.35f));
        bannerText.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        bannerText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        bannerText.rectTransform.sizeDelta = new Vector2(900f, 110f);
        bannerText.rectTransform.anchoredPosition = new Vector2(0f, 315f);
        bannerText.alpha = 0f;

       
    }

    private Image CreateHealthBar(Transform parent, string label, Vector2 position, Color fillColor)
    {
        GameObject root = new GameObject(label + " Health Bar", typeof(RectTransform));
        root.transform.SetParent(parent, false);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 1f);
        rootRect.anchorMax = new Vector2(0.5f, 1f);
        rootRect.sizeDelta = new Vector2(420f, 78f);
        rootRect.anchoredPosition = position;

        TextMeshProUGUI labelText = CreateUiText(label + " Label", root.transform, label, 24, FontStyles.Bold, Color.white);
        labelText.rectTransform.anchorMin = new Vector2(0f, 1f);
        labelText.rectTransform.anchorMax = new Vector2(0f, 1f);
        labelText.rectTransform.pivot = new Vector2(0f, 1f);
        labelText.rectTransform.sizeDelta = new Vector2(180f, 30f);
        labelText.rectTransform.anchoredPosition = Vector2.zero;
        labelText.alignment = TextAlignmentOptions.Left;

        Image background = CreateUiImage("Background", root.transform, new Color(1f, 1f, 1f, 0.18f));
        background.rectTransform.anchorMin = new Vector2(0f, 0f);
        background.rectTransform.anchorMax = new Vector2(1f, 0f);
        background.rectTransform.pivot = new Vector2(0f, 0f);
        background.rectTransform.sizeDelta = new Vector2(0f, 26f);
        background.rectTransform.anchoredPosition = Vector2.zero;

        Image fill = CreateUiImage("Fill", background.transform, fillColor);

        fill.rectTransform.anchorMin = new Vector2(0f, 0f);
        fill.rectTransform.anchorMax = new Vector2(1f, 1f);
        fill.rectTransform.pivot = new Vector2(0f, 0.5f);
        fill.rectTransform.offsetMin = Vector2.zero;
        fill.rectTransform.offsetMax = Vector2.zero;
        fill.rectTransform.localScale = Vector3.one;

        return fill;
    }

    private IEnumerator ShowBanner(string message, float duration, bool bigPulse)
    {
        bannerText.text = message;

        float timer = 0f;
        while (timer < 0.35f)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / 0.35f;
            bannerText.alpha = t;
            bannerText.rectTransform.localScale = Vector3.one * Mathf.Lerp(bigPulse ? 1.35f : 1.12f, 1f, t);
            yield return null;
        }

        if (duration > 10f)
        {
            bannerText.alpha = 1f;
            while (true)
            {
                bannerText.rectTransform.localScale = Vector3.one * (1f + Mathf.Sin(Time.unscaledTime * 3f) * 0.05f);
                yield return null;
            }
        }

        yield return new WaitForSecondsRealtime(duration);

        timer = 0f;
        while (timer < 0.35f)
        {
            timer += Time.unscaledDeltaTime;
            bannerText.alpha = 1f - timer / 0.35f;
            yield return null;
        }

        bannerText.alpha = 0f;
    }

    private void UpdateHealthBars()
    {
        if (playerHealthFill != null)
        {
            float playerPercent = Mathf.Clamp01((float)playerHealth / playerMaxHealth);
            playerHealthFill.rectTransform.localScale = new Vector3(playerPercent, 1f, 1f);
        }

        if (bossHealthFill != null)
        {
            float bossPercent = Mathf.Clamp01((float)bossHealth / bossMaxHealth);
            bossHealthFill.rectTransform.localScale = new Vector3(bossPercent, 1f, 1f);
        }
    }

  

    private void SyncPlayerHealthBar()
    {
        if (playerHealthComponent == null)
        {
            return;
        }

        playerHealth = playerHealthComponent.health;
        UpdateHealthBars();
    }

    private void DestroyBossAttacks()
    {
        foreach (BossProjectile projectile in FindObjectsByType<BossProjectile>(FindObjectsSortMode.None))
        {
            Destroy(projectile.gameObject);
        }
    }

    private GameObject CreateSpriteObject(string objectName, Vector3 position, Vector2 scale, Color color, bool useCircle = false)
    {
        GameObject spriteObject = new GameObject(objectName, typeof(SpriteRenderer));
        spriteObject.transform.position = position;
        spriteObject.transform.localScale = new Vector3(scale.x, scale.y, 1f);

        SpriteRenderer renderer = spriteObject.GetComponent<SpriteRenderer>();
        renderer.sprite = useCircle ? GetCircleSprite() : GetSquareSprite();
        renderer.color = color;
        renderer.sortingOrder = 1;

        return spriteObject;
    }

    private Sprite GetSquareSprite()
    {
        if (squareSprite != null)
        {
            return squareSprite;
        }

        Texture2D texture = Texture2D.whiteTexture;
        squareSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), texture.width);
        return squareSprite;
    }

    private Sprite GetCircleSprite()
    {
        if (circleSprite != null)
        {
            return circleSprite;
        }

        const int size = 64;
        Texture2D texture = new Texture2D(size, size);
        Color clear = new Color(1f, 1f, 1f, 0f);
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.48f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                texture.SetPixel(x, y, distance <= radius ? Color.white : clear);
            }
        }

        texture.Apply();
        circleSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        return circleSprite;
    }

    private Image CreateUiImage(string objectName, Transform parent, Color color)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private TextMeshProUGUI CreateUiText(string objectName, Transform parent, string value, int size, FontStyles style, Color color)
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

    private void ConfigureCamera()
    {
        if (mainCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            mainCamera = cameraObject.GetComponent<Camera>();
            cameraObject.tag = "MainCamera";
        }

        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 20f;
        mainCamera.transform.position = new Vector3(0f, 0f, -10f);
        mainCamera.backgroundColor = new Color(0.04f, 0.06f, 0.11f);
    }
}

public class BossProjectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private int damage;
    private bool hurtsBoss;
    private float lifetime;

    public void Setup(Vector2 moveDirection, float moveSpeed, int hitDamage, bool targetsBoss, float aliveTime)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        direction = moveDirection.normalized;
        speed = moveSpeed;
        damage = hitDamage;
        hurtsBoss = targetsBoss;
        lifetime = aliveTime;
        transform.up = direction;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hurtsBoss)
        {
            return;
        }

        if (!collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            BossSceneController controller = FindFirstObjectByType<BossSceneController>();
            if (controller != null)
            {
                controller.DamagePlayer(damage);
            }

            Destroy(gameObject);
        }
    }


}

public class BossHitbox : MonoBehaviour
{
    private BossSceneController controller;
  
    public void Setup(BossSceneController sceneController)
    {
        controller = sceneController;
    }

    public void TakeDamage(int damage)
    {
       

        if (controller != null)
        {
            controller.DamageBoss(damage);
        }
    }
}

