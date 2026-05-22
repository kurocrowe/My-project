using UnityEngine;

public class ShieldPowerup : MonoBehaviour
{
    [SerializeField] private float shieldDuration = 5f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
        {
            return;
        }

        PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.ActivateShield(shieldDuration);
            Destroy(gameObject);
        }
    }
}