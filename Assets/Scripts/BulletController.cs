
using UnityEngine;
public class BulletController : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    public int damage = 10;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }
    // Update is called once per frame
    void Update()
    {
        transform.position += transform.up * moveSpeed * Time.deltaTime;
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        // Prints out name of object that collides
        Debug.Log("Collided with: " + collision.gameObject.name);
      
        Damageable damageable;
        if (collision.TryGetComponent<Damageable>(out damageable))
        {
            damageable.TakeDamage(damage);
            Destroy(this.gameObject);
        }
    }



}
