using UnityEngine;

public class MovementComponent : MonoBehaviour
{
    private Rigidbody2D _rigidbody2D;
    private EnvironmentSensors _environmentSensors;
    private ColliderState _colliderState;
    
    private IBrain _brain;
    private IMovement _currentMovement;

    private void Update()
    {

    }
}
