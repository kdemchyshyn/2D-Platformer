using System;
using UnityEngine;

namespace DefaultNamespace
{
    public enum ColliderState
    {
        Stand = 0,
        Crouch = 1,
        Dash = 2
    }

    [Serializable]
    public class StateCollider
    {
        [SerializeField]
        private ColliderState _state;

        [SerializeField]
        private Collider2D _collider;

        public ColliderState State => _state;
        public Collider2D Collider => _collider;

        public bool SetActive(ColliderState state)
        {
            if (_collider == null)
                return false;

            _collider.enabled = _state == state;

            return _collider.enabled;
        }
    }
}