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
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            inputDir = InputSystem.actions["Move"].ReadValue<Vector2>();
            return;
        }

        float horizontal = 0f;
        float vertical = 0f;

        if (keyboard[GameSettings.TurnLeftKey].isPressed)
        {
            horizontal -= 1f;
        }

        if (keyboard[GameSettings.TurnRightKey].isPressed)
        {
            horizontal += 1f;
        }

        if (keyboard[GameSettings.MoveUpKey].isPressed)
        {
            vertical += 1f;
        }

        if (keyboard[GameSettings.MoveDownKey].isPressed)
        {
            vertical -= 1f;
        }

        inputDir = new Vector2(horizontal, vertical);
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
