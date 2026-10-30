using System;
using System.Collections.Generic;
using DefaultNamespace.Behaviours;
using UnityEngine;

namespace DefaultNamespace
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MovementComponent))]
    [RequireComponent(typeof(EnvironmentNavigator))]
    public abstract class BaseEnemy : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField]
        private Transform _target;

        [Header("Behaviors")]
        [SerializeField]
        private List<MonoBehaviour> _behaviorComponents = new();

        private readonly List<IEnemyBehavior> _behaviors = new();

        protected MovementComponent Movement { get; private set; }
        protected EnvironmentNavigator Navigator { get; private set; }

        public Transform Target => _target;
        public IEnemyBehavior CurrentBehavior { get; private set; }

        protected IReadOnlyList<IEnemyBehavior> Behaviors => _behaviors;

        protected virtual void Awake()
        {
            Movement = GetComponent<MovementComponent>();
            Navigator = GetComponent<EnvironmentNavigator>();

            foreach (MonoBehaviour component in _behaviorComponents)
            {
                if (component is not IEnemyBehavior behavior)
                {
                    Debug.LogError("BaseEnemy: All behavior components must implement IEnemyBehavior.",this);
                    enabled = false;
                    return;
                }

                if (_behaviors.Contains(behavior))
                {
                    Debug.LogError("BaseEnemy: Duplicate behavior.",this );
                    enabled = false;
                    return;
                }
                _behaviors.Add(behavior);
            }

            foreach (IEnemyBehavior behavior in _behaviors)
            {
                behavior.Initialize(Movement, Navigator);
            }
        }

        protected virtual void FixedUpdate()
        {
            EvaluateBehaviorTransitions();

            CurrentBehavior?.Tick(Time.fixedDeltaTime);
        }

        // Implemented by each concrete enemy.
        protected abstract void EvaluateBehaviorTransitions();

        protected void ChangeBehavior(IEnemyBehavior nextBehavior)
        {
            if (CurrentBehavior == nextBehavior) return;
            if (nextBehavior != null && !_behaviors.Contains(nextBehavior))
            {
                throw new InvalidOperationException("BaseEnemy: Behavior is not registered.");
            }

            CurrentBehavior?.Deactivate();
            CurrentBehavior = nextBehavior;
            CurrentBehavior?.Activate();
        }

        protected T GetBehavior<T>() where T : class, IEnemyBehavior
        {
            foreach (IEnemyBehavior behavior in _behaviors)
            {
                if (behavior is T result) return result;
            }
            return null;
        }

        protected virtual void OnDisable()
        {
            CurrentBehavior?.Deactivate();
            CurrentBehavior = null;
        }
    }
}