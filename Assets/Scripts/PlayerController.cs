using DefaultNamespace;
using UnityEngine;

[RequireComponent(typeof(PlayerInput), typeof(MovementComponent), typeof(EnvironmentNavigator))]
public class PlayerController : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite crouchSprite;

    private float _faceDirection = 1f;
    private Sprite _standingSprite;

    private PlayerInput _playerInput;
    private MovementComponent _movement;
    private EnvironmentNavigator _navigator;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _movement = GetComponent<MovementComponent>();
        _navigator = GetComponent<EnvironmentNavigator>();

        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null) _standingSprite = spriteRenderer.sprite;
    }

    private void FixedUpdate()
    {
        int horizontalInput = _playerInput.Horizontal;
        UpdateDirection(horizontalInput);

        bool isGrounded = _navigator.IsGrounded;
        if (_playerInput.CrouchHeld && isGrounded) _movement.TryCrouch();
        else _movement.TryStand();

        if (isGrounded) _movement.Move(horizontalInput);
        else _movement.AirControl(horizontalInput);

        if (_playerInput.JumpBuffered && _movement.TryJump())
        {
            _playerInput.ConsumeJump();
        }
    }

    private void LateUpdate()
    {
        if (spriteRenderer == null || crouchSprite == null) return;

        // Keep the crouch pose while a ceiling prevents the character from standing.
        spriteRenderer.sprite = _movement.IsCrouching ? crouchSprite : _standingSprite;
    }

    private void UpdateDirection(float direction)
    {
        if (direction == 0 ||
            _faceDirection == Mathf.Sign(direction))
            return;

        _faceDirection = Mathf.Sign(direction);
        transform.Rotate(0f, 180f, 0f);
    }
}
