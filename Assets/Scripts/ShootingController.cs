using UnityEngine;
using UnityEngine.InputSystem;
public class ShootingController : MonoBehaviour
{
    [SerializeField] private GameObject bullet;
  
void Start()
    {
    }
    // Update is called once per frame
    void Update()
    {
        var isShooting =
        InputSystem.actions["Shoot"].WasPressedThisFrame();
        if (isShooting)
        {
            Instantiate(bullet, transform.position, transform.rotation);
        }
    }
}
