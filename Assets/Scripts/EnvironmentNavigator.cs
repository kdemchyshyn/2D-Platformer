using System;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnvironmentNavigator : MonoBehaviour
    {
        [Header("Environment Checks")]
        [SerializeField]
        private List<OverlapBoxCheck> _checks = new();

        private Rigidbody2D _rigidbody;

        private readonly List<Collider2D> _results = new(8);

        public bool IsGrounded => HasOverlap(EnvironmentCheckType.Ground);

        public bool IsNearLadder => HasOverlap(EnvironmentCheckType.Ladder, optional: true);

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        public bool CanStand()
        {
            return !HasOverlap(EnvironmentCheckType.Stand);
        }

        public bool IsNearEdge(int direction)
        {
            ValidateDirection(direction);

            return !HasOverlap(
                EnvironmentCheckType.Edge,
                direction
            );
        }

        public bool IsWallAhead(int direction)
        {
            ValidateDirection(direction);

            return HasOverlap(
                EnvironmentCheckType.Wall,
                direction,
                optional: true
            );
        }

        private bool HasOverlap(EnvironmentCheckType type, int direction = 0, bool optional = false)
        {
            OverlapBoxCheck check = FindCheck(type);

            if (check == null)
            {
                if (optional)return false;

                throw new InvalidOperationException($"EnvironmentNavigator: missing {type} check.");
            }

            if (!check.IsValid)
            {
                throw new InvalidOperationException($"EnvironmentNavigator: invalid size for {type} check.");
            }

            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(check.Layers);
            filter.useTriggers = check.IncludeTriggers;

            _results.Clear();

            Physics2D.OverlapBox(
                check.GetCenter(transform, direction),
                check.Size,
                0f,
                filter,
                _results
            );

            foreach (Collider2D hit in _results)
            {
                if (hit == null)continue;
                // Ignore the character's own colliders.
                if (hit.attachedRigidbody == _rigidbody) continue;
                if (hit.transform.IsChildOf(transform)) continue;

                return true;
            }

            return false;
        }

        private OverlapBoxCheck FindCheck(EnvironmentCheckType type)
        {
            foreach (OverlapBoxCheck check in _checks)
            {
                if (check != null && check.Type == type) return check;
            }

            return null;
        }

        private static void ValidateDirection(int direction)
        {
            if (direction != -1 && direction != 1)
            {
                throw new ArgumentOutOfRangeException( nameof(direction), "Direction must be -1 or 1." );
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_checks == null)
                return;

            foreach (OverlapBoxCheck check in _checks)
            {
                if (check == null || !check.IsValid) continue;

                switch (check.Type)
                {
                    case EnvironmentCheckType.Ground:
                        Gizmos.color = Color.green;
                        break;

                    case EnvironmentCheckType.Stand:
                        Gizmos.color = Color.magenta;
                        break;

                    case EnvironmentCheckType.Ladder:
                        Gizmos.color = Color.cyan;
                        break;

                    case EnvironmentCheckType.Edge:
                        Gizmos.color = Color.yellow;
                        break;

                    case EnvironmentCheckType.Wall:
                        Gizmos.color = Color.red;
                        break;
                }

                if (check.Type == EnvironmentCheckType.Edge ||
                    check.Type == EnvironmentCheckType.Wall)
                {
                    Gizmos.DrawWireCube(
                        check.GetCenter(transform, -1),
                        check.Size
                    );

                    Gizmos.DrawWireCube(
                        check.GetCenter(transform, 1),
                        check.Size
                    );
                }
                else
                {
                    Gizmos.DrawWireCube(
                        check.GetCenter(transform),
                        check.Size
                    );
                }
            }
        }
    }
}