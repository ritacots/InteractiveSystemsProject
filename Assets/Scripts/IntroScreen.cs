using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class IntroScreen : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject introPanel;
    public GameObject countdownPanel;

    [Header("Position validation")]
    [Tooltip("Player 1")]
    public Image halfPanelRight;
    [Tooltip("Player 2")]
    public Image halfPanelLeft;

    [Tooltip("Colour when player is not in position")]
    public Color colorNotReady = new Color(0f, 0f, 0f, 0.7f);
    [Tooltip("Colour when player is in position")]
    public Color colorReady = new Color(0f, 0f, 0f, 0f);

    [Header("Players")]
    public Transform player1Transform;
    public Transform player2Transform;

    [Header("Wait before the countdown")]
    [Tooltip("Seconds the players must be at the correct position before starting")]
    public float bothReadyHoldTime = 3f;

    [Header("Countdown images")]
    public Image countdown3Image;
    public Image countdown2Image;
    public Image countdown1Image;
    public Image countdownStartImage;

    [Header("Countdown sound")]
    public AudioSource pipsAudioSource;
    public AudioSource startAudioSource;

    [Header("References of the game")]
    public GameTimer gameTimer;
    public AudioSource backgroundMusic;

    [Header("Time")]
    public float stepDuration = 1f;
    public float startDuration = 1f;

    bool _gameStarted = false;
    float _holdTimer = 0f;   

    void Start()
    {
        Time.timeScale = 1f;
        AutoFindReferences();

        if (gameTimer != null) gameTimer.PauseTimer();

        if (backgroundMusic != null)
        {
            backgroundMusic.loop = true;
            backgroundMusic.Play();
        }

        if (introPanel != null) introPanel.SetActive(true);
        if (countdownPanel != null) countdownPanel.SetActive(false);

        HideAllCountdownImages();

        SetHalfPanel(halfPanelRight, false);
        SetHalfPanel(halfPanelLeft, false);
    }

    void Update()
    {
        if (_gameStarted) return;

        bool p1OK = player1Transform != null && player1Transform.position.x > 0f;
        bool p2OK = player2Transform != null && player2Transform.position.x < 0f;

        SetHalfPanel(halfPanelRight, p1OK);
        SetHalfPanel(halfPanelLeft, p2OK);

        if (p1OK && p2OK)
        {
            _holdTimer += Time.deltaTime;
            if (_holdTimer >= bothReadyHoldTime)
                TriggerStart();
        }
        else
        {
            _holdTimer = 0f;
        }
    }

    void SetHalfPanel(Image panel, bool ready)
    {
        if (panel == null) return;
        panel.color = ready ? colorReady : colorNotReady;
    }

    public void TriggerStart()
    {
        if (_gameStarted) return;
        _gameStarted = true;
        Time.timeScale = 1f;
        StartCoroutine(StartSequence());
    }

    IEnumerator StartSequence()
    {
        if (introPanel != null && introPanel != this.gameObject)
            introPanel.SetActive(false);

        if (countdownPanel != null) countdownPanel.SetActive(true);
        HideAllCountdownImages();

        yield return ShowCountdownStep(countdown3Image, pipsAudioSource);
        yield return ShowCountdownStep(countdown2Image, pipsAudioSource);
        yield return ShowCountdownStep(countdown1Image, pipsAudioSource);
        yield return ShowCountdownStep(countdownStartImage, startAudioSource, startDuration);

        if (countdownPanel != null) countdownPanel.SetActive(false);

        if (backgroundMusic != null && !backgroundMusic.isPlaying)
        {
            backgroundMusic.loop = true;
            backgroundMusic.Play();
        }

        if (gameTimer != null) gameTimer.ResumeTimer();
        if (GameManager.Instance != null) GameManager.Instance.StartGame();
    }

    IEnumerator ShowCountdownStep(Image img, AudioSource source, float duration = -1f)
    {
        if (duration < 0f) duration = stepDuration;

        HideAllCountdownImages();

        if (img != null) img.gameObject.SetActive(true);
        if (source != null) source.Play();

        yield return new WaitForSecondsRealtime(duration);

        if (img != null) img.gameObject.SetActive(false);
    }

    void HideAllCountdownImages()
    {
        if (countdown3Image != null) countdown3Image.gameObject.SetActive(false);
        if (countdown2Image != null) countdown2Image.gameObject.SetActive(false);
        if (countdown1Image != null) countdown1Image.gameObject.SetActive(false);
        if (countdownStartImage != null) countdownStartImage.gameObject.SetActive(false);
    }

    void AutoFindReferences()
    {
        if (introPanel == null)
            introPanel = GameObject.Find("IntroPanel");

        if (countdownPanel == null)
            countdownPanel = GameObject.Find("CountDownPanel");

        if (backgroundMusic == null)
        {
            GameObject go = GameObject.Find("background");
            if (go != null) backgroundMusic = go.GetComponent<AudioSource>();
        }

        if (pipsAudioSource == null)
        {
            GameObject go = GameObject.Find("pips");
            if (go != null) pipsAudioSource = go.GetComponent<AudioSource>();
        }

        if (startAudioSource == null)
        {
            GameObject go = GameObject.Find("start");
            if (go != null) startAudioSource = go.GetComponent<AudioSource>();
        }

        if (countdown3Image == null)
        {
            GameObject go = GameObject.Find("3");
            if (go != null) countdown3Image = go.GetComponent<Image>();
        }

        if (countdown2Image == null)
        {
            GameObject go = GameObject.Find("2");
            if (go != null) countdown2Image = go.GetComponent<Image>();
        }

        if (countdown1Image == null)
        {
            GameObject go = GameObject.Find("1");
            if (go != null) countdown1Image = go.GetComponent<Image>();
        }

        if (countdownStartImage == null)
        {
            GameObject go = GameObject.Find("START");
            if (go != null) countdownStartImage = go.GetComponent<Image>();
        }

        if (gameTimer == null)
            gameTimer = FindObjectOfType<GameTimer>();

        if (player1Transform == null)
        {
            GameObject go = GameObject.Find("Player1");
            if (go != null) player1Transform = go.transform;
        }

        if (player2Transform == null)
        {
            GameObject go = GameObject.Find("Player2");
            if (go != null) player2Transform = go.transform;
        }
    }
}