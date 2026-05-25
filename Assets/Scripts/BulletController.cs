using UnityEngine;

public class BulletController : MonoBehaviour
{
    public static bool laserUnlocked;

    [Header("Normal Bullet")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private int normalDamage = 10;

    [Header("Laser Bullet")]
    [SerializeField] private float laserMoveSpeed = 18f;
    [SerializeField] private int laserDamage = 30;
    [SerializeField] private Vector3 laserScale = new Vector3(0.35f, 4f, 1f);
    [SerializeField] private Color laserColor = Color.red;

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private AudioClip sfxDamage;

    private int damage;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (boxCollider == null)
        {
            boxCollider = GetComponent<BoxCollider2D>();
        }
    }

    private void Start()
    {
        damage = normalDamage;

        if (laserUnlocked)
        {
            BecomeLaser();
        }
    }

    private void Update()
    {
        transform.position += transform.up * moveSpeed * Time.deltaTime;
    }

    private void BecomeLaser()
    {
        damage = laserDamage;
        moveSpeed = laserMoveSpeed;
        transform.localScale = laserScale;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = laserColor;
        }

        if (boxCollider != null)
        {
            boxCollider.size = new Vector2(1f, 4f);
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Collided with: " + collision.gameObject.name);

        Damageable damageable = collision.GetComponentInParent<Damageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        Damageable1 damageable1 = collision.GetComponentInParent<Damageable1>();
        if (damageable1 != null)
        {
            damageable1.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        BossHitbox boss = collision.GetComponentInParent<BossHitbox>();
        if (boss != null)
        {
            GameObject hitSfx = GameObject.Find("Sound Effects Player");

            if (hitSfx != null &&
                sfxDamage != null &&
                hitSfx.TryGetComponent<AudioSource>(out AudioSource source))
            {
                source.PlayOneShot(sfxDamage);
            }
            boss.TakeDamage(damage);
            Destroy(gameObject);
        }
    }

    public static void UnlockLaser()
    {
        laserUnlocked = true;
    }

    public static void ResetToNormalBullet()
    {
        laserUnlocked = false;
    }
}
