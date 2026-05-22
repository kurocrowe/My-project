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

        if (collision.TryGetComponent<Damageable>(out Damageable damageable))
        {
            damageable.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (collision.TryGetComponent<Damageable1>(out Damageable1 damageable1))
        {
            damageable1.TakeDamage(damage);
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
