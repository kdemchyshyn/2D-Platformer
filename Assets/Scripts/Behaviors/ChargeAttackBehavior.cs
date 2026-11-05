using System;
using UnityEngine;

namespace DefaultNamespace.Behaviors
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MovementComponent))]
    [RequireComponent(typeof(EnvironmentNavigator))]
    public sealed class ChargeAttackBehavior : MonoBehaviour, IEnemyBehavior
    {
        [Header("Jump Targeting")]
        [SerializeField, Min(0f)]
        private float _horizontalTolerance = 0.2f;

        private MovementComponent _movement;
        private EnvironmentNavigator _navigator;
        private Rigidbody2D _rigidbody;
        private ContactFilter2D _supportFilter;
        private ContactFilter2D _blockingContactFilter;
        private BaseEnemy _enemy;

        private bool _active;
        private bool _jumpAttempted;
        private bool _leftGround;
        private bool _stopHorizontalMotion;

        private int _direction;
        private float _targetX;

        public bool JumpStarted { get; private set; }
        public bool IsComplete { get; private set; }
        public float HorizontalTolerance => _horizontalTolerance;

        public void Initialize(
            MovementComponent movement,
            EnvironmentNavigator navigator)
        {
            _movement = movement != null ? movement : throw new ArgumentNullException(nameof(movement));
            _navigator = navigator != null ? navigator : throw new ArgumentNullException(nameof(navigator));
            _rigidbody = _movement.GetComponent<Rigidbody2D>();
            _supportFilter.SetNormalAngle(45f, 135f);

            _enemy = GetComponent<BaseEnemy>();
            if (_enemy == null)
            {
                throw new InvalidOperationException("ChargeAttackBehavior requires BaseEnemy on the same GameObject.");
            }
        }

        public void Activate()
        {
            if (_movement == null || _navigator == null || _enemy == null)
            {
                throw new InvalidOperationException("ChargeAttackBehavior must be initialized before activation.");
            }

            _active = true;
            _jumpAttempted = false;
            _leftGround = false;
            _stopHorizontalMotion = false;
            _direction = 0;

            JumpStarted = false;
            IsComplete = false;
        }

        public void Tick(float deltaTime)
        {
            if (!_active) return;

            if (IsComplete)
            {
                if (_navigator.IsGrounded)
                {
                    _movement.Move(0f);
                }
                return;
            }

            if (!_jumpAttempted)
            {
                _jumpAttempted = true;

                // Attack range is checked by the concrete enemy.
                Transform target = _enemy.Target;
                if (target == null || !_navigator.IsGrounded)
                {
                    IsComplete = true;
                    return;
                }

                // Capture target position once.
                _targetX = target.position.x;

                float deltaX = _targetX - transform.position.x;

                _direction = deltaX > _horizontalTolerance ? 1
                           : deltaX < -_horizontalTolerance ? -1
                           : 0;
                bool movingRight = _direction > 0;
                _blockingContactFilter.SetNormalAngle(movingRight ? 135f : 45f, movingRight ? 225f : 315f);
                _blockingContactFilter.useOutsideNormalAngle = !movingRight;

                JumpStarted = _movement.TryJump();

                if (!JumpStarted)
                {
                    IsComplete = true;
                    return;
                }

                // Face the target.
                if (_direction != 0 && Mathf.Sign(transform.forward.z) != _direction)
                {
                    transform.Rotate(0f, 180f, 0f);
                }
            }

            bool grounded = _navigator.IsGrounded;
            if (!grounded)
            {
                _leftGround = true;
            }

            // Upward contacts also count as landings on colliders outside the ground mask.
            if (_leftGround && _rigidbody.linearVelocity.y <= 0f &&
                (grounded || _rigidbody.IsTouching(_supportFilter)))
            {
                IsComplete = true;
                _movement.Move(0f);
                return;
            }

            // Stop pushing into a wall so friction cannot hold the enemy in midair.
            if (!_stopHorizontalMotion && _direction != 0)
            {
                _stopHorizontalMotion = _direction * (_targetX - transform.position.x) <= _horizontalTolerance ||
                                        _rigidbody.IsTouching(_blockingContactFilter);
            }

            _movement.AirControl(_stopHorizontalMotion ? 0f : _direction);
        }

        public void Deactivate()
        {
            _active = false;
        }
    }
}
