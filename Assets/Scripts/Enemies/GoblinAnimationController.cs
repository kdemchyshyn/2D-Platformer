using UnityEngine;

namespace DefaultNamespace.Enemies
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator), typeof(Rigidbody2D), typeof(EnvironmentNavigator))]
    public sealed class GoblinAnimationController : MonoBehaviour
    {
        private const float VerticalSpeedTolerance = 0.1f;
        private static readonly int SpeedParameter = Animator.StringToHash("Speed");
        private static readonly int VerticalSpeedParameter = Animator.StringToHash("VerticalSpeed");
        private static readonly int GroundedParameter = Animator.StringToHash("Grounded");
        private static readonly int JumpParameter = Animator.StringToHash("Jump");

        private Animator _animator;
        private Rigidbody2D _rigidbody;
        private EnvironmentNavigator _navigator;
        private bool _wasRising;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _navigator = GetComponent<EnvironmentNavigator>();
        }

        private void Update()
        {
            Vector2 velocity = _rigidbody.linearVelocity;
            bool isRising = velocity.y > VerticalSpeedTolerance;

            _animator.SetFloat(SpeedParameter, Mathf.Abs(velocity.x));
            _animator.SetFloat(VerticalSpeedParameter, velocity.y);
            // The feet may still overlap the floor during takeoff.
            _animator.SetBool(GroundedParameter, !isRising && _navigator.IsGrounded);

            if (isRising && !_wasRising)
            {
                _animator.ResetTrigger(JumpParameter);
                _animator.SetTrigger(JumpParameter);
            }

            _wasRising = isRising;
        }

        private void OnDisable()
        {
            _wasRising = false;
            if (_animator != null) _animator.ResetTrigger(JumpParameter);
        }
    }
}
