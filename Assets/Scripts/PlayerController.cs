using DefaultNamespace;
using UnityEngine;

[RequireComponent(typeof(PlayerInput), typeof(MovementComponent), typeof(EnvironmentNavigator))]
public class PlayerController : MonoBehaviour
{
    private float _faceDirection = 1f;

    private PlayerInput _playerInput;
    private MovementComponent _movement;
    private EnvironmentNavigator _navigator;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _movement = GetComponent<MovementComponent>();
        _navigator = GetComponent<EnvironmentNavigator>();
    }

    private void FixedUpdate()
    {
        int horizontalInput = _playerInput.Horizontal;
        UpdateDirection(horizontalInput);

        if (_navigator.IsGrounded) _movement.Move(horizontalInput);
        else _movement.AirControl(horizontalInput);

        if (_playerInput.JumpBuffered && _movement.TryJump())
        {
            _playerInput.ConsumeJump();
        }
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
