using UnityEngine;

public class Jump : IMovement
{
    public bool CanExecute(EnvironmentSensors sensors)
    {
        return sensors.IsGrounded;
    }

    public void Execute(Rigidbody2D rb, ColliderState colliderState, MovementIntent intent)
    {
        throw new System.NotImplementedException();
    }
}
