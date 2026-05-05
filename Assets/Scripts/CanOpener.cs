using UnityEngine;

public class CanOpener : MonoBehaviour
{
    [Header("References")]
    public GameObject openCanObject;

    [Header("Settings")]
    public float proximityRadius = 1.5f;  // augmentem una mica per ser més còmode
    public float holdDuration = 1f;

    private float holdTimer = 0f;
    private bool isOpen = false;

    void Update()
    {
        if (isOpen) return;

        bool anyPlayerClose = false;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            // Distància només en X i Z (ignora Y per vista top-down)
            Vector2 playerXZ = new Vector2(player.transform.position.x, player.transform.position.z);
            Vector2 canXZ = new Vector2(transform.position.x, transform.position.z);
            float distance = Vector2.Distance(playerXZ, canXZ);

            if (distance <= proximityRadius)
            {
                anyPlayerClose = true;
                break;
            }
        }

        if (anyPlayerClose)
        {
            holdTimer += Time.deltaTime;
            if (holdTimer >= holdDuration)
                OpenCan();
        }
        else
        {
            holdTimer = 0f;
        }
    }

    void OpenCan()
    {
        isOpen = true;
        gameObject.SetActive(false);

        if (openCanObject != null)
        {
            openCanObject.transform.position = transform.position;
            openCanObject.transform.rotation = transform.rotation;
            openCanObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("CanOpener: No open can object assigned!");
        }
    }
    public void ResetCan()
    {
        isOpen = false;
        holdTimer = 0f;
    }
}