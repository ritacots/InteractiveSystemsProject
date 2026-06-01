using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("End-Game Panel")]
    [Tooltip("The root Panel GameObject that is hidden during play.")]
    public GameObject endGamePanel;
    [Tooltip("Image shown when Player 1 wins.")]
    public Image player1WinsImage;
    [Tooltip("Image shown when Player 2 wins.")]
    public Image player2WinsImage;
    [Tooltip("Image shown when it's a tie.")]
    public Image tieImage;

    [Header("Score Display")]
    public TMP_Text player1ScoreLabel;
    public TMP_Text player2ScoreLabel;

    [Header("Audio Settings")]
    [Tooltip("Drag the AudioSource that has your background song here.")]
    public AudioSource backgroundMusic;

    [Header("End Game Sounds")]
    public AudioSource endGameAudioSource;
    public AudioClip winSound;
    public AudioClip tieSound;

    private int scoreP1 = 0;
    private int scoreP2 = 0;
    private bool gameOver = false;
    private bool gameStarted = false;

    public bool IsGameStarted => gameStarted;

    public void StartGame()
    {
        gameStarted = true;
    }

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

        if (player1ScoreLabel != null) player1ScoreLabel.gameObject.SetActive(false);
        if (player2ScoreLabel != null) player2ScoreLabel.gameObject.SetActive(false);

        if (backgroundMusic != null)
        {
            backgroundMusic.playOnAwake = false;
            backgroundMusic.loop = true;
            backgroundMusic.Stop();
        }


        if (endGameAudioSource == null)
        {
            endGameAudioSource = gameObject.AddComponent<AudioSource>();
            endGameAudioSource.playOnAwake = false;
            Debug.LogWarning("[GameManager] endGameAudioSource wasn't assigned");
        }

        endGameAudioSource.ignoreListenerPause = true;
    }

    public void EndGame()
    {
        if (gameOver) return;
        gameOver = true;

        if (backgroundMusic != null) backgroundMusic.Stop();
        if (player1WinsImage != null) player1WinsImage.gameObject.SetActive(false);
        if (player2WinsImage != null) player2WinsImage.gameObject.SetActive(false);
        if (tieImage != null) tieImage.gameObject.SetActive(false);

        AudioClip clipToPlay = null;
        if (scoreP1 > scoreP2)
        {
            if (player1WinsImage != null) player1WinsImage.gameObject.SetActive(true);
            clipToPlay = winSound;
        }
        else if (scoreP2 > scoreP1)
        {
            if (player2WinsImage != null) player2WinsImage.gameObject.SetActive(true);
            clipToPlay = winSound;
        }
        else
        {
            if (tieImage != null) tieImage.gameObject.SetActive(true);
            clipToPlay = tieSound;
        }

        AudioListener.pause = false;
        PlayEndSound(clipToPlay);

        if (player1ScoreLabel != null) player1ScoreLabel.text = "P1: " + scoreP1;
        if (player2ScoreLabel != null) player2ScoreLabel.text = "P2: " + scoreP2;

        if (endGamePanel != null) endGamePanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void AddScore(int playerNumber, int points = 1)
    {
        if (gameOver) return;
        if (playerNumber == 1)
        {
            scoreP1 += points;
            if (player1ScoreLabel != null)
            {
                player1ScoreLabel.gameObject.SetActive(true);
                player1ScoreLabel.text = scoreP1.ToString();
            }
        }
        else if (playerNumber == 2)
        {
            scoreP2 += points;
            if (player2ScoreLabel != null)
            {
                player2ScoreLabel.gameObject.SetActive(true);
                player2ScoreLabel.text = scoreP2.ToString();
            }
        }
    }

    void PlayEndSound(AudioClip clip)
    {
        if (endGameAudioSource == null)
        {
            Debug.LogError("[GameManager] endGameAudioSource is null.");
            return;
        }
        if (clip == null)
        {
            Debug.LogWarning("[GameManager] AudioClip hasn't been assigned");
            return;
        }
        endGameAudioSource.ignoreListenerPause = true;
        endGameAudioSource.Stop();
        endGameAudioSource.clip = clip;
        endGameAudioSource.Play();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public bool IsGameOver => gameOver;
}