using UnityEngine;

public class PlayAgainButton : MonoBehaviour
{
    [Header("Players")]
    public Transform player1;
    public Transform player2;

    [Header("Sound")]
    public AudioSource playAgainSound;

    [Header("Configuration")]
    [Tooltip("Distance (XZ) the player activates teh button")]
    public float activationRadius = 1.5f;
    [Tooltip("Time the player needs to stay in the radius to restart")]
    public float holdTime = 1.5f;

    private float _timer = 0f;
    private bool _triggered = false;

    void OnEnable()
    {
        _triggered = false;
        _timer = 0f;
    }

    void Update()
    {
        if (_triggered) return;
        if (GameManager.Instance == null || !GameManager.Instance.IsGameOver) return;

        bool playerNearby = IsPlayerNear(player1) || IsPlayerNear(player2);

        if (playerNearby)
        {
            _timer += Time.unscaledDeltaTime;
            if (_timer >= holdTime)
            {
                _triggered = true;
                if (playAgainSound != null) playAgainSound.Play();
                GameManager.Instance.RestartGame();
            }
        }
        else
        {
            _timer = 0f;
        }
    }

    bool IsPlayerNear(Transform player)
    {
        if (player == null) return false;
        Vector2 playerXZ = new Vector2(player.position.x, player.position.z);
        Vector2 buttonXZ = new Vector2(transform.position.x, transform.position.z);
        return Vector2.Distance(playerXZ, buttonXZ) <= activationRadius;
    }
}