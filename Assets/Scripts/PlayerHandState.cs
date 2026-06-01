using UnityEngine;

public class PlayerHandState : MonoBehaviour
{
    private MonoBehaviour _occupiedBy = null;

    public bool IsHoldingFor(MonoBehaviour requester)
        => _occupiedBy == requester;
    public bool IsFree => _occupiedBy == null;

    public bool Occupy(MonoBehaviour requester)
    {
        if (_occupiedBy == null || _occupiedBy == requester)
        {
            _occupiedBy = requester;
            return true;
        }
        return false; 
    }

    public void Release(MonoBehaviour requester)
    {
        if (_occupiedBy == requester)
            _occupiedBy = null;
    }
}
