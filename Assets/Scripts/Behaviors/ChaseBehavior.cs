using System;
using UnityEngine;

namespace DefaultNamespace.Behaviors
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MovementComponent))]
    [RequireComponent(typeof(EnvironmentNavigator))]
    public sealed class ChaseBehavior : MonoBehaviour, IEnemyBehavior
    {
        [Header("Chase")]
        [SerializeField, Min(0f)] private float _stoppingDistance = 0.75f;

        private MovementComponent _movement;
        private EnvironmentNavigator _navigator;
        private BaseEnemy _enemy;
        private bool _active;

        public void Initialize(MovementComponent movement, EnvironmentNavigator navigator)
        {
            _movement = movement != null ? movement : throw new ArgumentNullException(nameof(movement));
            _navigator = navigator != null ? navigator : throw new ArgumentNullException(nameof(navigator));
            _enemy = GetComponent<BaseEnemy>();

            if (_enemy == null)
            {
                throw new InvalidOperationException("ChaseBehavior requires BaseEnemy on the same GameObject.");
            }
        }

        public void Activate()
        {
            if (_movement == null || _navigator == null || _enemy == null)
            {
                throw new InvalidOperationException("ChaseBehavior must be initialized before activation.");
            }

            _active = true;
        }

        public void Tick(float deltaTime)
        {
            if (!_active) return;

            Transform target = _enemy.Target;
            if (target == null || !_navigator.IsGrounded)
            {
                _movement.Move(0f);
                return;
            }

            // Read the current target position each tick instead of locking it at activation.
            float deltaX = target.position.x - transform.position.x;
            if (Mathf.Abs(deltaX) <= _stoppingDistance)
            {
                _movement.Move(0f);
                return;
            }

            int direction = deltaX > 0f ? 1 : -1;
            if (Mathf.Sign(transform.forward.z) != direction)
            {
                transform.Rotate(0f, 180f, 0f);
            }

            // Reuse the same platform checks as PatrolBehavior.
            if (_navigator.IsNearEdge(direction) || _navigator.IsWallAhead(direction))
            {
                _movement.Move(0f);
                return;
            }

            _movement.Move(direction);
        }

        public void Deactivate()
        {
            if (!_active) return;

            _active = false;
            if (_movement != null) _movement.Move(0f);
        }
    }
}
