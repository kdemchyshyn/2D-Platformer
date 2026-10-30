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
            throw new NotImplementedException(); 
        }

        // Implemented by each concrete enemy.
        protected abstract void EvaluateBehaviorTransitions();

        protected void ChangeBehavior(IEnemyBehavior nextBehavior)
        {
            throw new NotImplementedException();
        }

        protected T GetBehavior<T>() where T : class, IEnemyBehavior
        {
            throw new NotImplementedException();
        }

        protected virtual void OnDisable()
        {
            throw new NotImplementedException();
        }
    }
}