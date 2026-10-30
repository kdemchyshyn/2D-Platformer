using System;
using UnityEngine;

namespace DefaultNamespace.Behaviors
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MovementComponent))]
    [RequireComponent(typeof(EnvironmentNavigator))]
    public sealed class DashRetreatBehavior : MonoBehaviour, IEnemyBehavior
    {
        [Header("Retreat")]
        [SerializeField, Min(0.01f)] private float _retreatDistance = 3f;

        [Header("Optional Platform Safety")]
        [SerializeField] private bool _limitToSafeGround = true;
        [SerializeField, Min(0.02f)] private float _sampleSpacing = 0.1f;
        [SerializeField, Min(0f)] private float _edgeMargin = 0.1f;
        [SerializeField, Min(0.01f)] private float _minimumDashDistance = 0.25f;

        private MovementComponent _movement;
        private EnvironmentNavigator _navigator;
        private BaseEnemy _enemy;
        private bool _active;
        private bool _attempted;

        public bool DashStarted { get; private set; }

        public void Initialize(MovementComponent movement, EnvironmentNavigator navigator)
        {
            _movement = movement != null ? movement : throw new ArgumentNullException(nameof(movement));
            _navigator = navigator != null ? navigator : throw new ArgumentNullException(nameof(navigator));

            _enemy = GetComponent<BaseEnemy>();
            if (_enemy == null)
            {
                throw new InvalidOperationException("DashRetreatBehavior requires a BaseEnemy on the same GameObject.");
            }
        }

        public void Activate()
        {
            if (_movement == null || _navigator == null || _enemy == null)
            {
                throw new InvalidOperationException("DashRetreatBehavior must be initialized before activation.");
            }

            _active = true;
            _attempted = false;
            DashStarted = false;
        }

        public void Tick(float deltaTime)
        {
            if (!_active || _attempted) return;

            Transform target = _enemy.Target;
            if (target == null) return; 

            float awayX = transform.position.x - target.position.x;
            if (Mathf.Abs(awayX) <= 0.01f) return; 

            int direction = awayX > 0f ? 1 : -1;
            float distance = _retreatDistance;

            if (_limitToSafeGround)
            {
                if (!_navigator.IsGrounded) return;

                distance = _navigator.GetSafeHorizontalDistance( direction, _retreatDistance, _sampleSpacing);

                // Keep an additional gap from a detected edge/wall.
                if (distance < _retreatDistance)
                {
                    distance = Mathf.Max(0f, distance - _edgeMargin);
                }
            }

            if (distance < _minimumDashDistance) return;

            _attempted = true;
            DashStarted = _movement.TryDash(direction, distance);
        }

        public void Deactivate()
        {
            _active = false;
            _attempted = false;
        }
    }
}