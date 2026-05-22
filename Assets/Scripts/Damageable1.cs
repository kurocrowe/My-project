using UnityEngine;

public class Damageable1 : MonoBehaviour
{
    public static int length = 5;
    private static int enemiesKilled;
    public int health = 100;

    [SerializeField] private UISliderUpdater sliderUI;
    [SerializeField] private AudioClip sfxDamage;
    [SerializeField] private GameObject vfxDamage;

    private bool isDead;

    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            return;
        }

        GameObject hitSfx = GameObject.Find("Sound Effects Player");

        if (hitSfx != null &&
            sfxDamage != null &&
            hitSfx.TryGetComponent<AudioSource>(out AudioSource source))
        {
            source.PlayOneShot(sfxDamage);
        }

        health -= damage;

        if (sliderUI != null)
        {
            sliderUI.SetSliderValue(health);
        }

        if (health > 0)
        {
            return;
        }

        isDead = true;

        if (vfxDamage != null)
        {
            Instantiate(vfxDamage, transform.position, transform.rotation);
        }

        GameObject player = GameObject.Find("Tank");

        if (player != null &&
            player.TryGetComponent<ScoreManager>(out ScoreManager mgr))
        {
            mgr.AddScore(10);
        }

        Damageable1.length--;
        enemiesKilled++;

        if (enemiesKilled >= 3)
        {
            BulletController.UnlockLaser();
        }
        Debug.Log("Enemies left: " + Damageable.length);

        if (Damageable1.length <= 0)
        {
            BulletController.ResetToNormalBullet();
          
        }

        Destroy(gameObject);
    }
}