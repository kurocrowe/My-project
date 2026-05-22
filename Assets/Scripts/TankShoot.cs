// TankShoot.cs

using UnityEngine;
using UnityEngine.InputSystem;

public class TankShoot : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletForce = 20f;

    void Update()
    {
        Shoot();
    }

    void Shoot()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            GameObject bullet = Instantiate(
                bulletPrefab,
                firePoint.position,
                firePoint.rotation
            );

            Rigidbody2D rb =
                bullet.GetComponent<Rigidbody2D>();

            rb.linearVelocity =
                firePoint.up * bulletForce;
        }
    }
}