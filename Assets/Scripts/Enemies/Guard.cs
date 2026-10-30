using DefaultNamespace.Behaviors;
using UnityEngine;

namespace DefaultNamespace.Enemies
{
    [DisallowMultipleComponent]
    public sealed class Guard : BaseEnemy
    {
        private PatrolBehavior _patrol;

        protected override void Awake()
        {
            base.Awake();

            if (!enabled) return;

            _patrol = GetBehavior<PatrolBehavior>();
            if (_patrol == null)
            {
                Debug.LogError("Guard: register PatrolBehavior in the BaseEnemy behavior list.", this);
                enabled = false;
            }
        }

        protected override void EvaluateBehaviorTransitions()
        {
            if (CurrentBehavior == null)
            {
                ChangeBehavior(_patrol);
            }
        }
    }
}