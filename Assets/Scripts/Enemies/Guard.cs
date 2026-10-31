using DefaultNamespace.Behaviors;
using System;
using UnityEngine;

namespace DefaultNamespace.Enemies
{
    [DisallowMultipleComponent]
    public sealed class Guard : BaseEnemy
    {
        [Header("Vision")]
        [SerializeField] private Transform _visionOrigin;
        [SerializeField] private LayerMask _visibilityLayers = ~0;
        [SerializeField, Min(0f)] private float _detectionRange = 8f;

        [Header("Attack Ranges")]
        [SerializeField, Min(0f)] private float _chargeRange = 5f;
        [SerializeField, Min(0f)] private float _retreatRange = 1.5f;

        [Header("Cooldowns")]
        [SerializeField, Min(0f)] private float _chargeCooldown = 2f;
        [SerializeField, Min(0f)] private float _retreatCooldown = 2f;

        private PatrolBehavior _patrol;
        private ChargeAttackBehavior _charge;
        private DashRetreatBehavior _retreat;

        private Collider2D[] _targetColliders = Array.Empty<Collider2D>();
        private readonly RaycastHit2D[] _sightHits = new RaycastHit2D[16];

        private float _nextChargeTime;
        private float _nextRetreatTime;

        public float DistanceToTarget =>
            Target == null
                ? float.PositiveInfinity
                : Vector2.Distance(transform.position, Target.position);

        protected override void Awake()
        {
            base.Awake();

            if (!enabled) return;

            _patrol = GetBehavior<PatrolBehavior>();
            _charge = GetBehavior<ChargeAttackBehavior>();
            _retreat = GetBehavior<DashRetreatBehavior>();

            if (_patrol == null || _charge == null || _retreat == null)
            {
                Debug.LogError(
                    "Guard requires registered Patrol, ChargeAttack " +
                    "and DashRetreat behaviors.",
                    this);

                enabled = false;
                return;
            }

            if (Target != null)
            {
                _targetColliders =
                    Target.GetComponentsInChildren<Collider2D>(true);
            }
        }

        protected override void EvaluateBehaviorTransitions()
        {
            // Do not interrupt a jump attack before it finishes.
            if (CurrentBehavior == _charge && !_charge.IsComplete)
                return;

            // Keep the retreat behavior active while its dash is expected
            // to be running.
            if (CurrentBehavior == _retreat && !_retreat.IsComplete)
            {
                return;
            }

            if (!CanSeeTarget())
            {
                ChangeBehavior(_patrol);
                return;
            }

            float distance = DistanceToTarget;

            // Highest priority: retreat when the target is very close.
            if (distance <= _retreatRange)
            {
                if (CurrentBehavior != _retreat &&
                    Time.time >= _nextRetreatTime)
                {
                    StartRetreat();
                }
                else
                {
                    ChangeBehavior(_patrol);
                }

                return;
            }

            // Medium range: jump toward the target.
            if (distance <= _chargeRange)
            {
                if (CurrentBehavior != _charge &&
                    Time.time >= _nextChargeTime)
                {
                    StartCharge();
                }
                else
                {
                    ChangeBehavior(_patrol);
                }

                return;
            }

            // Target is visible but outside attack range.
            ChangeBehavior(_patrol);
        }

        private void StartRetreat()
        {
            ChangeBehavior(_retreat);
            _nextRetreatTime = Time.time + _retreatCooldown;
        }

        private void StartCharge()
        {
            ChangeBehavior(_charge);

            _nextChargeTime = Time.time + _chargeCooldown;
        }

        private bool CanSeeTarget()
        {
            if (Target == null)
                return false;

            if (DistanceToTarget > _detectionRange)
                return false;

            if (!TryGetTargetAimPoint(out Vector2 targetPoint))
                return false;

            Vector2 origin = _visionOrigin != null
                ? _visionOrigin.position
                : transform.position;

            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(_visibilityLayers);
            filter.useTriggers = false;

            int hitCount = Physics2D.Linecast(
                origin,
                targetPoint,
                filter,
                _sightHits
            );

            // Results are ordered by distance.
            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = _sightHits[i].collider;

                if (hit == null)
                    continue;

                // Ignore the Guard's own colliders.
                if (hit.transform == transform ||
                    hit.transform.IsChildOf(transform))
                {
                    continue;
                }

                // The first external collider must belong to Target.
                return hit.transform == Target ||
                       hit.transform.IsChildOf(Target);
            }

            return false;
        }

        private bool TryGetTargetAimPoint(out Vector2 point)
        {
            point = default;

            foreach (Collider2D collider in _targetColliders)
            {
                if (collider == null ||
                    !collider.enabled ||
                    !collider.gameObject.activeInHierarchy ||
                    collider.isTrigger)
                {
                    continue;
                }

                point = collider.bounds.center;
                return true;
            }

            return false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _detectionRange);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _chargeRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _retreatRange);
        }
    }
}