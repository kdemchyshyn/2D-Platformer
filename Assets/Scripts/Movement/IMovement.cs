using UnityEngine;

public interface IMovement
{
    bool CanExecute(EnvironmentSensors sensors);
    void Execute(Rigidbody2D rb, ColliderState colliderState, MovementIntent intent);
}
