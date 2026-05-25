// TankShoot.cs

using UnityEngine;
using UnityEngine.InputSystem;

public class TankShoot : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletForce = 20f;
    [SerializeField] private GameObject muzzleFlashPrefab;
    void Update()
    {
        Shoot();
    }

    void Shoot()
    {
        bool mousePressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool keyPressed = Keyboard.current != null && Keyboard.current[GameSettings.ShootKey].wasPressedThisFrame;

        if (mousePressed || keyPressed)
        {
            GameObject bullet = Instantiate(
                bulletPrefab,
                firePoint.position,
                firePoint.rotation
            );

            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.linearVelocity = firePoint.up * bulletForce;
            }

            if (muzzleFlashPrefab != null)
            {
                Instantiate(
                    muzzleFlashPrefab,
                    firePoint.position,
                    firePoint.rotation
                );
            }
        }
    }
}
