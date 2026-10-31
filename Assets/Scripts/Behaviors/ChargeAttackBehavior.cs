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
        private BaseEnemy _enemy;

        private bool _active;
        private bool _jumpAttempted;
        private bool _leftGround;
        private bool _reachedTargetX;

        private int _direction;
        private float _targetX;

        public bool JumpStarted { get; private set; }
        public bool IsComplete { get; private set; }

        public void Initialize(
            MovementComponent movement,
            EnvironmentNavigator navigator)
        {
            _movement = movement != null ? movement : throw new ArgumentNullException(nameof(movement));
            _navigator = navigator != null ? navigator : throw new ArgumentNullException(nameof(navigator));
            
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
            _reachedTargetX = false;
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

            // Wait until the enemy has actually left the ground.
            if (!_navigator.IsGrounded)
            {
                _leftGround = true;
            }

            // The jump finishes after landing.
            if (_leftGround && _navigator.IsGrounded)
            {
                IsComplete = true;
                _movement.Move(0f);
                return;
            }

            // Move toward the captured target position.
            if (_direction != 0 && _direction * (_targetX - transform.position.x) <= _horizontalTolerance)
            {
                _reachedTargetX = true;
            }

            _movement.AirControl(_reachedTargetX ? 0f : _direction);
        }

        public void Deactivate()
        {
            _active = false;
        }
    }
}