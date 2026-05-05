using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [HideInInspector]
    public Transform target; // s'assigna per codi quan el jugador l'agafa

    [Tooltip("Offset respecte la mà (en unitats de Unity)")]
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

        // La llauna segueix la mà ignorant Y (top-down)
        Vector3 newPos = new Vector3(
            target.position.x + offset.x,
            transform.position.y, // manté la seva pròpia Y
            target.position.z + offset.z
        );
        transform.position = newPos;
    }
}