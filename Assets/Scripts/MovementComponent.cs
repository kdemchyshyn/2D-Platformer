using System;
using UnityEngine;

namespace DefaultNamespace
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(EnvironmentNavigator))]
    public class MovementComponent : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Min(0f)] private float moveSpeed = 6f;

        [Header("Ground Movement")]
        [SerializeField, Min(0f)] private float groundAcceleration = 50f;
        [SerializeField, Min(0f)] private float groundDeceleration = 60f;

        [Header("Air Movement")]
        [SerializeField, Range(0f, 1f)] private float airMoveSpeedMultiplier = 0.5f;
        [SerializeField, Min(0f)] private float airAcceleration = 25f;
        [SerializeField, Min(0f)] private float airDeceleration = 15f;

        [Header("Jump")]
        [SerializeField, Min(0f)] private float jumpSpeed = 10f;

        [Header("Dash")]
        [SerializeField, Min(0f)] private float dashSpeed = 12f;
        [SerializeField, Min(0f)] private float dashDuration = 0.28f;
        [SerializeField, Min(0f)] private float dashCooldown = 0.7f;
        
        [Header("Ladder")]
        [SerializeField, Min(0f)] private float climbSpeed = 5f;

        private Rigidbody2D _rigidbody;
        private EnvironmentNavigator _navigator;

        private int _jumpsRemaining = 2;

        private float _dashStartX;
        private float _dashDistance;
        private float _dashDirection;
        private float _dashElapsed;
        private float _dashLastX;
        private int _dashStallFrames;
        private float _nextDashTime;

        private float _gravityBeforeClimbing;
        
        public bool IsDashing { get; private set; }
        public bool IsClimbing { get; private set; }
        
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _navigator = GetComponent<EnvironmentNavigator>();
        }

        private void FixedUpdate()
        {
            if (_navigator.IsGrounded && _rigidbody.linearVelocity.y <= 0f)
            {
                ResetJumps();
            }

            if (IsDashing)
            {
                UpdateDash(Time.fixedDeltaTime);
            }
        }
        
        private void OnDisable()
        {
            if (IsDashing) EndDash();
            StopClimbing();
        }
        
        /// <summary>Ground horizontal movement. Direction: -1 to +1; 0 means no input.</summary>
        public void Move(float direction)
        {
            if (IsDashing || IsClimbing) return;
            
            direction = Mathf.Clamp(direction, -1f, 1f);
            Vector2 velocity = _rigidbody.linearVelocity;
            velocity.x = Mathf.MoveTowards(velocity.x, GetGroundTargetSpeed(direction),
                GetGroundAcceleration(direction) * Time.fixedDeltaTime);
            _rigidbody.linearVelocity = velocity;
        }

        /// <summary>Horizontal control while airborne, with separate speed and acceleration.</summary>
        public void AirControl(float direction)
        {
            if (IsDashing || IsClimbing) return;
            
            direction = Mathf.Clamp(direction, -1f, 1f);
            Vector2 velocity = _rigidbody.linearVelocity;
            velocity.x = Mathf.MoveTowards(velocity.x, GetAirTargetSpeed(direction),
                GetAirAcceleration(direction) * Time.fixedDeltaTime);
            _rigidbody.linearVelocity = velocity;
        }

        /// <summary>Attempts the first or second jump. True only if a jump actually started.</summary>
        public bool TryJump()
        {
            if (IsDashing || IsClimbing) return false;
            
            if (_navigator.IsGrounded && _rigidbody.linearVelocity.y <= 0f)
            {
                ResetJumps();
            }

            if (!CanJump()) return false;

            if (_jumpsRemaining >= 1) StartJump();

            return true;
        }

        /// <summary>Attempts a dash in horizontal direction -1 or +1.</summary>
        public bool TryDash(float direction, float distance = -1f)
        {
            if (!CanDash() || float.IsNaN(direction) || float.IsInfinity(direction) ||
                Mathf.Approximately(direction, 0f))
                return false;

            float requestedDistance = distance == -1f
                ? dashSpeed * dashDuration
                : distance;

            if (float.IsNaN(requestedDistance) || float.IsInfinity(requestedDistance) ||
                requestedDistance <= 0f)
                return false;

            StartDash(Mathf.Sign(direction), requestedDistance);
            return true;
        }

        /// <summary>Climbs on a ladder. Vertical input: -1 to +1, or 0 to stay still.</summary>
        public void Climb(float verticalDirection, bool active = true)
        {
            if (!active)
            {
                StopClimbing();
                return;
            }

            if (IsDashing)
                return;

            if (!IsClimbing)
                StartClimbing();

            ApplyClimbing(Mathf.Clamp(verticalDirection, -1f, 1f));
        }

        // Movement
        private float GetGroundTargetSpeed(float direction)
        {
            return direction * moveSpeed;
        }

        private float GetGroundAcceleration(float direction)
        {
            return direction == 0f ? groundDeceleration : groundAcceleration;
        }

        // Air Movement
        private float GetAirTargetSpeed(float direction)
        {
            return direction * moveSpeed * airMoveSpeedMultiplier;
        }

        private float GetAirAcceleration(float direction)
        {
            return direction == 0f ? airDeceleration : airAcceleration;
        }

        // Jump
        private bool CanJump()
        {
            return _jumpsRemaining > 0;
        }

        private void StartJump()
        {
            Vector2 velocity = _rigidbody.linearVelocity;
            velocity.y = jumpSpeed;
            _rigidbody.linearVelocity = velocity;
            _jumpsRemaining--;
        }

        private void ResetJumps()
        {
            _jumpsRemaining = 2;
        }

        // Dash
        private bool CanDash()
        {
            return _rigidbody != null && !IsDashing && !IsClimbing &&
                   dashSpeed > 0f && Time.time >= _nextDashTime;
        }

        private void StartDash(float direction, float distance)
        {
            _dashDirection = direction;
            _dashDistance = distance;
            _dashStartX = _rigidbody.position.x;
            _dashLastX = _dashStartX;
            _dashElapsed = 0f;
            _dashStallFrames = 0;

            IsDashing = true;
            ApplyDashVelocity(Time.fixedDeltaTime, distance);
        }

        private void UpdateDash(float fixedDeltaTime)
        {
            float traveled = Mathf.Max(0f,
                (_rigidbody.position.x - _dashStartX) * _dashDirection);
            float remaining = _dashDistance - traveled;

            if (remaining <= 0.005f)
            {
                EndDash();
                return;
            }

            // If a wall stops the Rigidbody2D, do not keep pushing indefinitely.
            if (_dashElapsed > 0f &&
                Mathf.Abs(_rigidbody.position.x - _dashLastX) < 0.001f)
            {
                _dashStallFrames++;
                if (_dashStallFrames >= 2)
                {
                    EndDash();
                    return;
                }
            }
            else
            {
                _dashStallFrames = 0;
            }

            // Allow a little physics tolerance beyond the ideal dash travel time.
            if (_dashElapsed >= _dashDistance / dashSpeed + 3f * fixedDeltaTime)
            {
                EndDash();
                return;
            }

            _dashLastX = _rigidbody.position.x;
            ApplyDashVelocity(fixedDeltaTime, remaining);
            _dashElapsed += fixedDeltaTime;
        }
        
        private void ApplyDashVelocity(float fixedDeltaTime, float remaining)
        {
            if (fixedDeltaTime <= 0f)
                return;

            // Slow the final physics step to avoid passing the requested distance.
            float speed = Mathf.Min(dashSpeed, remaining / fixedDeltaTime);
            Vector2 velocity = _rigidbody.linearVelocity;
            velocity.x = _dashDirection * speed;
            _rigidbody.linearVelocity = velocity;
        }

        private void EndDash()
        {
            if (!IsDashing) return;

            IsDashing = false;
            Vector2 velocity = _rigidbody.linearVelocity;
            velocity.x = 0f;
            _rigidbody.linearVelocity = velocity;
            _nextDashTime = Time.time + dashCooldown;
        }

        // Ladder
        private void StartClimbing()
        {
            if (IsClimbing)
                return;

            _gravityBeforeClimbing = _rigidbody.gravityScale;

            IsClimbing = true;

            _rigidbody.gravityScale = 0f;
            _rigidbody.linearVelocity = Vector2.zero;
        }

        private void ApplyClimbing(float verticalDirection)
        {
            _rigidbody.linearVelocity = new Vector2(
                0f,
                verticalDirection * climbSpeed
            );
        }

        private void StopClimbing()
        {
            if (!IsClimbing)
                return;

            IsClimbing = false;

            _rigidbody.gravityScale = _gravityBeforeClimbing;

            Vector2 velocity = _rigidbody.linearVelocity;
            velocity.y = 0f;
            _rigidbody.linearVelocity = velocity;
        }

    }
}
