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
        private static readonly int DashingParameter = Animator.StringToHash("IsDashing");
        private static readonly int ClimbingParameter = Animator.StringToHash("IsClimbing");
        private static readonly int CrouchingParameter = Animator.StringToHash("IsCrouching");
        private static readonly int ClimbSpeedParameter = Animator.StringToHash("ClimbSpeed");

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

            bool isDashing = _movement.IsDashing;
            bool isClimbing = _movement.IsClimbing;
            _animator.SetBool(DashingParameter, isDashing);
            _animator.SetBool(ClimbingParameter, isClimbing);
            _animator.SetBool(CrouchingParameter, _movement.IsCrouching);
            // Pause the ladder cycle while hanging still; climbing down uses the same cycle.
            _animator.SetFloat(ClimbSpeedParameter, isClimbing && Mathf.Abs(velocity.y) > 0.01f ? 1f : 0f);

            // The feet may still overlap the floor on the first frame of takeoff.
            bool isGrounded = !_jumpQueued && _navigator.IsGrounded && velocity.y <= 0.1f;
            _animator.SetBool(GroundedParameter, isGrounded);

            if (_jumpQueued && !isDashing && !isClimbing)
            {
                _animator.ResetTrigger(JumpParameter);
                _animator.SetTrigger(JumpParameter);
            }
            else if (isDashing || isClimbing) _animator.ResetTrigger(JumpParameter);

            _jumpQueued = false;
        }

        private void OnJumpStarted()
        {
            // Restart Jump for every successful jump, including the second one.
            _jumpQueued = true;
        }
    }
}
