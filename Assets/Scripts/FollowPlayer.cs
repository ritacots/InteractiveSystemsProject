using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [HideInInspector]
    public Transform target; 

    [Tooltip("Offset with respect to the hand")]
    public Vector3 offset = new Vector3(0.5f, 0f, 0.5f);

    private bool following = false;

    public void StartFollowing(Transform playerTransform)
    {
        target = playerTransform;
        following = true;
    }

    public void StopFollowing()
    {
        following = false;
        target = null;
    }

    public bool IsFollowing => following;

    void Update()
    {
        if (!following || target == null) return;

     
        Vector3 newPos = new Vector3(
            target.position.x + offset.x,
            transform.position.y, 
            target.position.z + offset.z
        );
        transform.position = newPos;
    }
}