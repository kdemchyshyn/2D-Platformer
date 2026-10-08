using UnityEngine;

public abstract class BaseEnemy : MonoBehaviour, IBrain
{
    public Transform Target { get; protected set; }

    protected IEnemyBehaviour _currentBehaviour;

    public virtual MovementIntent GetIntent()
    {
        throw new System.NotImplementedException();
    }
}
