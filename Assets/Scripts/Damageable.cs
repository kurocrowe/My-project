using UnityEngine;
public class Damageable : MonoBehaviour
{
    public int health = 100;
        [SerializeField] private UISliderUpdater sliderUI;
    [SerializeField] private AudioClip sfxDamage;
    [SerializeField] private GameObject vfxDamage;
    //[SerializeField] private AudioSource sfxDamage;
    public void TakeDamage(int damage)
    {
        // Play hit sound
        var hitSfx = GameObject.Find("Sound Effects Player");

        if (hitSfx &&
            hitSfx.TryGetComponent<AudioSource>(out AudioSource source))
        {
            source.PlayOneShot(sfxDamage);
        }

        // Reduce health
        health -= damage;
        sliderUI.SetSliderValue(health);
        // Only explode when dead
        if (health <= 0)
        {
            Instantiate(
                vfxDamage,
                transform.position,
                transform.rotation
            );

            Destroy(gameObject);
            var player = GameObject.Find("Tank");
            if (player && player.TryGetComponent<ScoreManager>(out ScoreManager
            mgr))
            {
                mgr.AddScore(10);
            }
        }
    }
}