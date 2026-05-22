using UnityEngine;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveForce = 10f;
    [SerializeField] private float maxSpeed = 8f;

    [Header("Rotation")]
    [SerializeField] private float rotationTorque = 5f;

    private Rigidbody2D rb;

    private Vector2 inputDir;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        inputDir =
            InputSystem.actions["Move"].ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        RotateTank();
        MoveTank();
        ClampVelocity();
    }

    void RotateTank()
    {
        float rotationInput = -inputDir.x;

        rb.AddTorque(
            rotationInput * rotationTorque,
            ForceMode2D.Force
        );
    }

    void MoveTank()
    {
        rb.AddForce(inputDir * moveForce, ForceMode2D.Force);
    }

    void ClampVelocity()
    {
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity =
                rb.linearVelocity.normalized * maxSpeed;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log(collision.gameObject.name);
    }
}