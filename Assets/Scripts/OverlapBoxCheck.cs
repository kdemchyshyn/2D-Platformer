using System;
using UnityEngine;

namespace DefaultNamespace
{
    public enum EnvironmentCheckType
    {
        Ground,
        Stand,
        Ladder,
        Edge,
        Wall
    }

    [Serializable]
    public class OverlapBoxCheck
    {
        [SerializeField] private EnvironmentCheckType _type;
        [SerializeField] private Transform _origin;
        [SerializeField] private Vector2 _offset;
        [SerializeField] private Vector2 _size = new Vector2(0.5f, 0.2f);
        [SerializeField] private LayerMask _layers;
        [SerializeField] private bool _includeTriggers;

        public EnvironmentCheckType Type => _type;
        public Vector2 Size => _size;
        public LayerMask Layers => _layers;
        public bool IncludeTriggers => _includeTriggers || _type == EnvironmentCheckType.Ladder;
        public bool IsValid => _size.x > 0f && _size.y > 0f;

        public Vector2 GetCenter( Transform defaultOrigin, int direction = 0)
        {
            Transform origin = _origin != null ? _origin : defaultOrigin;
            float offsetX = direction == 0 ? _offset.x : Mathf.Abs(_offset.x) * direction;
            return (Vector2)origin.position + new Vector2(offsetX, _offset.y);
        }
    }
}