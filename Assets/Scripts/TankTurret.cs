// TankTurret.cs

using UnityEngine;
using UnityEngine.InputSystem;

public class TankTurret : MonoBehaviour
{
    public Camera mainCamera;

    void Update()
    {
        RotateTurret();
    }

    void RotateTurret()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

        Vector3 mouseWorldPos =
            mainCamera.ScreenToWorldPoint(mouseScreenPos);

        mouseWorldPos.z = 0f;

        Vector2 direction =
            mouseWorldPos - transform.position;

        float angle =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        transform.rotation =
            Quaternion.Euler(0f, 0f, angle);
    }
}