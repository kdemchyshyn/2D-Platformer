using DefaultNamespace;
using UnityEngine;

[RequireComponent(typeof(PlayerInput), typeof(MovementComponent), typeof(EnvironmentNavigator))]
public class PlayerController : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    [SerializeField]
    private bool spriteFacesRight = true;

    private PlayerInput _playerInput;
    private MovementComponent _movement;
    private EnvironmentNavigator _navigator;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _movement = GetComponent<MovementComponent>();
        _navigator = GetComponent<EnvironmentNavigator>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void FixedUpdate()
    {
        int horizontalInput = _playerInput.Horizontal;
        UpdateFacing(horizontalInput);

        if (_navigator.IsGrounded) _movement.Move(horizontalInput);
        else _movement.AirControl(horizontalInput);

        if (_playerInput.JumpBuffered && _movement.TryJump())
        {
            _playerInput.ConsumeJump();
        }
    }

    private void UpdateFacing(int horizontalInput)
    {
        if (spriteRenderer == null || horizontalInput == 0) return;

        spriteRenderer.flipX = spriteFacesRight ? horizontalInput < 0 : horizontalInput > 0;
    }
}
