using DefaultNamespace.Behaviors;
using UnityEngine;

namespace DefaultNamespace.Enemies
{
    [DisallowMultipleComponent]
    public sealed class GoblinEnemy : BaseEnemy
    {
        [Header("Detection")]
        [SerializeField, Min(0f)] private float _detectionRange = 7f;
        [SerializeField] private Transform _visionOrigin;
        [SerializeField] private LayerMask _visibilityLayers = ~0; //all layers

        [Header("Charge Attack Behavior")]
        [SerializeField, Min(0f)] private float _jumpRange = 2.5f;
        [SerializeField, Min(0f)] private float _jumpCooldown = 2f;

        private PatrolBehavior _patrol;
        private ChaseBehavior _chase;
        private ChargeAttackBehavior _jump;
        private float _nextJumpTime;

        private readonly RaycastHit2D[] _sightHits = new RaycastHit2D[16];

        protected override void Awake()
        {
            base.Awake();
            if (!enabled) return;

            _patrol = GetBehavior<PatrolBehavior>();
            _chase = GetBehavior<ChaseBehavior>();
            _jump = GetBehavior<ChargeAttackBehavior>();

            if (_patrol == null || _chase == null || _jump == null)
            {
                Debug.LogError(
                    "GoblinEnemy: register PatrolBehavior, ChaseBehavior and ChargeAttackBehavior in the behavior list.",
                    this);
                enabled = false;
            }
        }

        protected override void EvaluateBehaviorTransitions()
        {
            // Let an active jump finish, even if the target leaves sight.
            if (ReferenceEquals(CurrentBehavior, _jump) && !_jump.IsComplete) return;

            if (!CanSeeTarget())
            {
                ChangeBehavior(_patrol);
                return;
            }

            float distance = Vector2.Distance(transform.position, Target.position);
            if (distance <= _jumpRange && Navigator.IsGrounded && Time.time >= _nextJumpTime)
            {
                if (!ReferenceEquals(CurrentBehavior, _jump))
                {
                    ChangeBehavior(_jump);
                    _nextJumpTime = Time.time + _jumpCooldown;
                    return;
                }
            }

            ChangeBehavior(_chase);
        }

        private bool CanSeeTarget()
        {
            if (Target == null || !Target.gameObject.activeInHierarchy) return false;
            if (Vector2.Distance(transform.position, Target.position) > _detectionRange) return false;

            Vector2 origin = _visionOrigin != null ? _visionOrigin.position : transform.position;
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(_visibilityLayers);
            filter.useTriggers = false;

            int count = Physics2D.Linecast(origin, Target.position, filter, _sightHits);
            for (int i = 0; i < count; i++)
            {
                Collider2D hit = _sightHits[i].collider;
                if (hit == null) continue;
                if (hit.transform == transform || hit.transform.IsChildOf(transform)) continue;
                if (hit.transform == Target || hit.transform.IsChildOf(Target)) return true;

                // The first external collider blocks sight of the target.
                return false;
            }

            // The target need not have a collider on the visibility layers.
            // If the hit buffer is full, there may be unchecked obstacles.
            return count < _sightHits.Length;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _detectionRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _jumpRange);
        }
    }
}
