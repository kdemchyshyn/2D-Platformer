using UnityEngine;

namespace DefaultNamespace
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator), typeof(Rigidbody2D), typeof(MovementComponent))]
    public class PlayerAnimationController : MonoBehaviour
    {
        private static readonly int SpeedParameter = Animator.StringToHash("Speed");
        private static readonly int VerticalSpeedParameter = Animator.StringToHash("VerticalSpeed");
        private static readonly int GroundedParameter = Animator.StringToHash("Grounded");
        private static readonly int JumpParameter = Animator.StringToHash("Jump");

        private Animator _animator;
        private Rigidbody2D _rigidbody;
        private MovementComponent _movement;
        private EnvironmentNavigator _navigator;
        private bool _jumpQueued;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _movement = GetComponent<MovementComponent>();
            _navigator = GetComponent<EnvironmentNavigator>();
        }

        private void OnEnable()
        {
            _movement.JumpStarted += OnJumpStarted;
        }

        private void OnDisable()
        {
            _movement.JumpStarted -= OnJumpStarted;
            _jumpQueued = false;
        }

        private void Update()
        {
            Vector2 velocity = _rigidbody.linearVelocity;
            _animator.SetFloat(SpeedParameter, Mathf.Abs(velocity.x));
            _animator.SetFloat(VerticalSpeedParameter, velocity.y);

            // The feet may still overlap the floor on the first frame of takeoff.
            bool isGrounded = !_jumpQueued && _navigator.IsGrounded && velocity.y <= 0.1f;
            _animator.SetBool(GroundedParameter, isGrounded);

            if (_jumpQueued)
            {
                _animator.ResetTrigger(JumpParameter);
                _animator.SetTrigger(JumpParameter);
                _jumpQueued = false;
            }
        }

        private void OnJumpStarted()
        {
            // Restart Jump for every successful jump, including the second one.
            _jumpQueued = true;
        }
    }
}
