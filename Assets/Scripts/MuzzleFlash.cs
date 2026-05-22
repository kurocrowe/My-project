using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    [SerializeField] private float lifeTime = 0.08f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }
}