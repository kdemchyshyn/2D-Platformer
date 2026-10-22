using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0f)]
    private float moveSpeed = 6f;

    [Header("Ground Movement")]
    [SerializeField, Min(0f)]
    private float groundAcceleration = 50f;
    [SerializeField, Min(0f)]
    private float groundDeceleration = 60f;

    [Header("Air Movement")]
    [SerializeField, Range(0f, 1f)]
    private float airMoveSpeedMultiplier = 0.5f;
    [SerializeField, Min(0f)]
    private float airAcceleration = 25f;
    [SerializeField, Min(0f)]
    private float airDeceleration = 15f;

    [Header("Ground Check")]
    [SerializeField]
    private LayerMask groundLayers = Physics2D.DefaultRaycastLayers;
    [SerializeField, Min(0f)]
    private float groundCheckDistance = 0.08f;

    [Header("Jump")]
    [SerializeField, Min(0f)]
    private float jumpSpeed = 10f;

    [Header("Visuals")]
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    [SerializeField]
    private bool spriteFacesRight = true;

    private Rigidbody2D _rigidbody;
    private PlayerInput _playerInput;
    private Collider2D _collider;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _playerInput = GetComponent<PlayerInput>();
        _collider = GetComponent<Collider2D>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void FixedUpdate()
    {
        int horizontalInput = _playerInput.Horizontal;
        UpdateFacing(horizontalInput);

        if (_collider == null) return;

        Vector2 velocity = _rigidbody.linearVelocity;
        bool isGrounded = IsGrounded();
        float movementSpeed = isGrounded ? moveSpeed : moveSpeed * airMoveSpeedMultiplier;
        float targetSpeed = horizontalInput * movementSpeed;

        float acceleration;
        if (isGrounded)
        {
            acceleration = horizontalInput == 0 ? groundDeceleration : groundAcceleration;
        }
        else
        {
            acceleration = horizontalInput == 0 ? airDeceleration : airAcceleration;
        }

        velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, acceleration * Time.fixedDeltaTime);

        if (isGrounded && _playerInput.JumpBuffered && _playerInput.ConsumeJump())
        {
            velocity.y = jumpSpeed;
        }

        _rigidbody.linearVelocity = velocity;
    }

    private void UpdateFacing(int horizontalInput)
    {
        if (spriteRenderer == null || horizontalInput == 0) return;

        spriteRenderer.flipX = spriteFacesRight ? horizontalInput < 0 : horizontalInput > 0;
    }

    private bool IsGrounded()
    {
        Bounds bounds = _collider.bounds;
        Vector2 origin = new Vector2(bounds.center.x, bounds.min.y - 0.01f);
        return Physics2D.Raycast(origin, Vector2.down, groundCheckDistance + 0.01f, groundLayers);
    }
}
