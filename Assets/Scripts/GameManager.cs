using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("End-Game Panel")]
    [Tooltip("The root Panel GameObject that is hidden during play.")]
    public GameObject endGamePanel;

    [Tooltip("The big 'Time Over' or winner text inside the panel.")]
    public TMP_Text resultText;

    [Header("Score Display")]
    public TMP_Text player1ScoreLabel;
    public TMP_Text player2ScoreLabel;

    [Header("Audio Settings")]
    [Tooltip("Drag the AudioSource that has your background song here.")]
    public AudioSource backgroundMusic;

    private int scoreP1 = 0;
    private int scoreP2 = 0;
    private bool gameOver = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (endGamePanel != null)
            endGamePanel.SetActive(false);

        // Start playing music if it's assigned
        if (backgroundMusic != null)
        {
            backgroundMusic.playOnAwake = true;
            backgroundMusic.loop = true;
            if (!backgroundMusic.isPlaying) backgroundMusic.Play();
        }
    }

    public void EndGame()
    {
        if (gameOver) return;
        gameOver = true;

        // Stop music when game ends
        if (backgroundMusic != null) backgroundMusic.Stop();

        Time.timeScale = 0f;

        string result;
        if (scoreP1 > scoreP2)
            result = "Player 1 Wins!";
        else if (scoreP2 > scoreP1)
            result = "Player 2 Wins!";
        else
            result = "Time Over!\nIt's a Tie!";

        if (resultText != null) resultText.text = result;
        if (endGamePanel != null) endGamePanel.SetActive(true);
    }

    public void AddScore(int playerNumber, int points = 1)
    {
        if (gameOver) return;

        if (playerNumber == 1)
        {
            scoreP1 += points;
            if (player1ScoreLabel != null) player1ScoreLabel.text = "P1: " + scoreP1;
        }
        else if (playerNumber == 2)
        {
            scoreP2 += points;
            if (player2ScoreLabel != null) player2ScoreLabel.text = "P2: " + scoreP2;
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public bool IsGameOver => gameOver;
}