using UnityEngine;

public class Move : IMovement
{
    public bool CanExecute(EnvironmentSensors sensors)
    {
        throw new System.NotImplementedException();
    }

    public void Execute(Rigidbody2D rb, ColliderState colliderState, MovementIntent intent)
    {
        throw new System.NotImplementedException();
    }
}
