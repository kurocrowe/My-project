using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int health = 5;

    [SerializeField] private GameObject shieldCircle;
    [SerializeField] private UISliderUpdater1 sliderUI;
    private bool shieldActive;
    private Coroutine shieldCoroutine;

    private void Start()
    {
        if (shieldCircle != null)
        {
            shieldCircle.SetActive(false);
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

        if (health <= 0)
        {
            Destroy(gameObject);
        }
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
}