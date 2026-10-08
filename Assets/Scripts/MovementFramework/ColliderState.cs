using UnityEngine;

public enum ColliderStateType
{

}

public class ColliderState : MonoBehaviour
{
    [SerializeField] private Collider2D _mainCollider;

    public void SetState(ColliderStateType stateType)
    {
        throw new System.NotImplementedException();
    }
}
