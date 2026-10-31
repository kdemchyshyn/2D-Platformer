using UnityEngine;

namespace DefaultNamespace
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 1f, -10f);
        [SerializeField, Min(0f)] private float smoothTime = 0.2f;

        private Vector3 _velocity;

        private void Start()
        {
            if (target != null) transform.position = target.position + offset;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 targetPosition = target.position + offset;
            transform.position = Vector3.SmoothDamp(
                transform.position, targetPosition, ref _velocity, smoothTime);
        }
    }
}
