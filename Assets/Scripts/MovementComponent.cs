using System;
using UnityEngine;

namespace DefaultNamespace
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovementComponent : MonoBehaviour
    {
        [Header("Ladder")]
        [SerializeField, Min(0f)] private float climbSpeed;

        private Rigidbody2D _rigidbody;
        private EnvironmentNavigator _navigator;

        [SerializeField] private int _jumpsRemaining = 2;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _navigator = GetComponent<EnvironmentNavigator>();
        }
        
        /// <summary>Ground horizontal movement. Direction: -1 to +1; 0 means no input.</summary>
        public void Move(float direction)
        {
            throw new NotImplementedException();
        }

        /// <summary>Horizontal control while airborne, with separate speed and acceleration.</summary>
        public void AirControl(float direction)
        {
            throw new NotImplementedException();
        }

        /// <summary>Attempts the first or second jump. True only if a jump actually started.</summary>
        public bool TryJump()
        {
            throw new NotImplementedException();
        }

        /// <summary>Attempts a dash in horizontal direction -1 or +1.</summary>
        public bool TryDash(float direction, float distance = -1f)
        {
            throw new NotImplementedException();
        }

        /// <summary>Climbs on a ladder. Vertical input: -1 to +1, or 0 to stay still.</summary>
        public void Climb(float verticalDirection)
        {
            throw new NotImplementedException();
        }

        // Movement
        private float GetGroundTargetSpeed(float direction)
        {
            throw new NotImplementedException();
        }

        private float GetGroundAcceleration(float direction)
        {
            throw new NotImplementedException();
        }

        // Air Movement
        private float GetAirTargetSpeed(float direction)
        {
            throw new NotImplementedException();
        }

        private float GetAirAcceleration(float direction)
        {
            throw new NotImplementedException();
        }

        // Jump
        private bool CanJump()
        {
            throw new NotImplementedException();
        }

        private void StartJump()
        {
            throw new NotImplementedException();
        }

        private void StartDoubleJump()
        {
            throw new NotImplementedException();
        }

        private void ResetJumps()
        {
            throw new NotImplementedException();
        }

        // Dash
        private bool CanDash()
        {
            throw new NotImplementedException();
        }

        private void StartDash(float direction)
        {
            throw new NotImplementedException();
        }

        private void UpdateDash(float fixedDeltaTime)
        {
            throw new NotImplementedException();
        }

        private void EndDash()
        {
            throw new NotImplementedException();
        }

        // Ladder
        private void StartClimbing()
        {
            throw new NotImplementedException();
        }

        private void ApplyClimbing(float verticalDirection)
        {
            throw new NotImplementedException();
        }

        private void StopClimbing()
        {
            throw new NotImplementedException();
        }

    }
}