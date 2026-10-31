using System;
using UnityEngine;

namespace DefaultNamespace.Behaviors
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MovementComponent))]
    [RequireComponent(typeof(EnvironmentNavigator))]
    public sealed class PatrolBehavior : MonoBehaviour, IEnemyBehavior
    {
        [Header("Patrol Limits (Optional)")]
        [SerializeField] private Transform _leftPoint;
        [SerializeField] private Transform _rightPoint;
        [SerializeField, Min(0f)]
        private float _pointTolerance = 0.1f;

        [Header("Starting Direction")]
        [SerializeField] private bool _startMovingRight = true;

        private MovementComponent _movement;
        private EnvironmentNavigator _navigator;
        private int _direction;
        private bool _isActive;

        public void Initialize( MovementComponent movement, EnvironmentNavigator navigator)
        {
            _movement = movement != null ? movement : throw new ArgumentNullException(nameof(movement));
            _navigator = navigator != null ? navigator : throw new ArgumentNullException(nameof(navigator));
            _direction = _startMovingRight ? 1 : -1;

            if ((_leftPoint == null) != (_rightPoint == null))
            {
                Debug.LogWarning("PatrolBehavior: assign both patrol points or neither. The single assigned point will be ignored.", this);
            }
        }

        public void Activate()
        {
            if (_movement == null || _navigator == null)
            {
                throw new InvalidOperationException("PatrolBehavior was not initialized.");
            }
            _isActive = true;
            Face(_direction);
        }

        public void Tick(float deltaTime)
        {
            if (!_isActive) return;

            if (!_navigator.IsGrounded)
            {
                _movement.Move(0f);
                return;
            }

            Debug.Log($"CanMove {CanMove(_direction)} direction: {_direction}");
            if (!CanMove(_direction))
            {
                int oppositeDirection = -_direction;
                if (!CanMove(oppositeDirection))
                {
                    _movement.Move(0f);
                    return;
                }

                Face(oppositeDirection);
            }

            _movement.Move(_direction);
        }

        public void Deactivate()
        {
            if (!_isActive) return;

            _isActive = false;
            if (_movement != null) _movement.Move(0f);
        }

        private bool CanMove(int direction)
        {
            if (_navigator.IsNearEdge(direction)) return false;
            if (_navigator.IsWallAhead(direction)) return false;
            if (ReachedPatrolLimit(direction)) return false;
            return true;
        }

        private bool ReachedPatrolLimit(int direction)
        {
            // Patrol points are optional, but must be assigned as a pair.
            if (_leftPoint == null || _rightPoint == null) return false;

            float leftX = Mathf.Min(_leftPoint.position.x, _rightPoint.position.x);
            float rightX = Mathf.Max( _leftPoint.position.x, _rightPoint.position.x);
            float x = transform.position.x;
            return direction > 0 ? x >= rightX - _pointTolerance : x <= leftX + _pointTolerance;
        }

        private void Face(int direction)
        {
            _direction = direction;
            if (Mathf.Sign(transform.forward.z) != direction)
            {
                transform.Rotate(0f, 180f, 0f);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_leftPoint == null || _rightPoint == null) return;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(_leftPoint.position, _rightPoint.position);
            Gizmos.DrawWireSphere(_leftPoint.position, 0.12f);
            Gizmos.DrawWireSphere(_rightPoint.position, 0.12f);
        }
    }
}