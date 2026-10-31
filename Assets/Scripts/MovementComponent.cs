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

        [Header("Crouch")]
        [SerializeField, Range(0f, 1f)] private float crouchSpeedMultiplier = 0.5f;
        [SerializeField] private StateCollider[] _stateColliders = Array.Empty<StateCollider>();

        [Header("Ladder")]
        [SerializeField, Min(0f)] private float climbSpeed;

        private Rigidbody2D _rigidbody;
        private EnvironmentNavigator _navigator;

        private int _jumpsRemaining = 2;
        private bool _canCrouch;

        public bool IsCrouching { get; private set; }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _navigator = GetComponent<EnvironmentNavigator>();

            Collider2D standingCollider = FindCollider(ColliderState.Stand);
            Collider2D crouchingCollider = FindCollider(ColliderState.Crouch);
            _canCrouch = standingCollider != null && crouchingCollider != null &&
                         standingCollider != crouchingCollider;

            if (_canCrouch) SetColliderState(ColliderState.Stand);
            else if (_stateColliders.Length > 0)
            {
                Debug.LogError("MovementComponent: crouching requires separate Stand and Crouch colliders.", this);
            }
        }

        private void FixedUpdate()
        {
            if (_navigator.IsGrounded && _rigidbody.linearVelocity.y <= 0f)
            {
                ResetJumps();
            }
        }
        
        /// <summary>Ground horizontal movement. Direction: -1 to +1; 0 means no input.</summary>
        public void Move(float direction)
        {
            direction = Mathf.Clamp(direction, -1f, 1f);
            Vector2 velocity = _rigidbody.linearVelocity;
            velocity.x = Mathf.MoveTowards(velocity.x, GetGroundTargetSpeed(direction),
                GetGroundAcceleration(direction) * Time.fixedDeltaTime);
            _rigidbody.linearVelocity = velocity;
        }

        /// <summary>Horizontal control while airborne, with separate speed and acceleration.</summary>
        public void AirControl(float direction)
        {
            direction = Mathf.Clamp(direction, -1f, 1f);
            Vector2 velocity = _rigidbody.linearVelocity;
            velocity.x = Mathf.MoveTowards(velocity.x, GetAirTargetSpeed(direction),
                GetAirAcceleration(direction) * Time.fixedDeltaTime);
            _rigidbody.linearVelocity = velocity;
        }

        /// <summary>Attempts the first or second jump. True only if a jump actually started.</summary>
        public bool TryJump()
        {
            if (_navigator.IsGrounded && _rigidbody.linearVelocity.y <= 0f)
            {
                ResetJumps();
            }

            if (!CanJump()) return false;
            if (IsCrouching && !TryStand()) return false;

            if (_jumpsRemaining >= 1) StartJump();

            return true;
        }

        /// <summary>Attempts to crouch on the ground. Requires configured Stand and Crouch colliders.</summary>
        public bool TryCrouch()
        {
            if (IsCrouching) return true;
            if (!_canCrouch || !_navigator.IsGrounded || _rigidbody.linearVelocity.y > 0f) return false;

            SetColliderState(ColliderState.Crouch);
            IsCrouching = true;
            return true;
        }

        /// <summary>Stands up only if the configured Stand check is clear.</summary>
        public bool TryStand()
        {
            if (!IsCrouching) return true;
            if (!_navigator.CanStand()) return false;

            SetColliderState(ColliderState.Stand);
            IsCrouching = false;
            return true;
        }

        /// <summary>Attempts a dash in horizontal direction -1 or +1.</summary>
        public bool TryDash(float direction)
        {
            throw new NotImplementedException();
        }

        /// <summary>Climbs on a ladder. Vertical input: -1 to +1, or 0 to stay still.</summary>
        public void Climb(float verticalDirection)
        {
            throw new NotImplementedException();
        }

        // Movement
        private float GetGroundTargetSpeed(float direction)
        {
            return direction * moveSpeed * (IsCrouching ? crouchSpeedMultiplier : 1f);
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

        // Crouch
        private Collider2D FindCollider(ColliderState state)
        {
            foreach (StateCollider entry in _stateColliders)
            {
                if (entry != null && entry.State == state) return entry.Collider;
            }

            return null;
        }

        private void SetColliderState(ColliderState state)
        {
            foreach (StateCollider entry in _stateColliders)
            {
                if (entry != null) entry.SetActive(state);
            }
        }

        // Dash
        private bool CanDash()
        {
            throw new NotImplementedException();
        }

        private void StartDash(float direction)
        {
            throw new NotImplementedException();
        }

        private void UpdateDash(float fixedDeltaTime)
        {
            throw new NotImplementedException();
        }

        private void EndDash()
        {
            throw new NotImplementedException();
        }

        // Ladder
        private void StartClimbing()
        {
            throw new NotImplementedException();
        }

        private void ApplyClimbing(float verticalDirection)
        {
            throw new NotImplementedException();
        }

        private void StopClimbing()
        {
            throw new NotImplementedException();
        }

    }
}
