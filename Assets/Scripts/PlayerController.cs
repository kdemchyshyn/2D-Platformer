using DefaultNamespace;
using UnityEngine;

[RequireComponent(typeof(PlayerInput), typeof(MovementComponent), typeof(EnvironmentNavigator))]
public class PlayerController : MonoBehaviour
{
    private float _faceDirection = 1f;

    private PlayerInput _playerInput;
    private MovementComponent _movement;
    private EnvironmentNavigator _navigator;
    
    private int _facingDirection;
    private float _ladderRegrabTime;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _movement = GetComponent<MovementComponent>();
        _navigator = GetComponent<EnvironmentNavigator>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        _facingDirection = spriteFacesRight ? 1 : -1;
    }

    private void FixedUpdate()
    {
        int horizontalInput = _playerInput.Horizontal;
        int verticalInput = _playerInput.Vertical;
        
        if (_movement.IsClimbing)
        {
            if (_playerInput.JumpBuffered)
            {
                _movement.Climb(0f, false);
                if (_movement.TryJump())
                {
                    _playerInput.ConsumeJump();
                }
                _ladderRegrabTime = Time.time + 0.25f;
                return;
            }

            // Leave when the ladder is gone or the player
            // is standing on ground without vertical input.
            if (!_navigator.IsFacingLadder(_facingDirection) ||
                (_navigator.IsGrounded && verticalInput == 0))
            {
                _movement.Climb(0f, false);
            }
            else
            {
                _movement.Climb(verticalInput);
                return;
            }
        }
        
        if (_movement.IsDashing) return;
        
        UpdateFacing(horizontalInput);

        if (_playerInput.DashBuffered && _movement.TryDash(_facingDirection))
        {
            _playerInput.ConsumeDash();
            return;
        }
        
        if (verticalInput != 0 &&
            Time.time >= _ladderRegrabTime &&
            !_movement.IsDashing &&
            _navigator.IsFacingLadder(_facingDirection))
        {
            _movement.Climb(verticalInput);
            return;
        }
        
        if (_navigator.IsGrounded) _movement.Move(horizontalInput);
        else _movement.AirControl(horizontalInput);

        if (_playerInput.JumpBuffered && _movement.TryJump())
        {
            _playerInput.ConsumeJump();
        }
    }

    private void OnDisable()
    {
        if (_movement != null && _movement.IsClimbing)
            _movement.Climb(0f, false);
    }
    
    private void UpdateFacing(int horizontalInput)
    {
        if (horizontalInput == 0) return;

        _facingDirection = horizontalInput;

        if (spriteRenderer == null) return;

        _faceDirection = Mathf.Sign(direction);
        transform.Rotate(0f, 180f, 0f);
    }
}
