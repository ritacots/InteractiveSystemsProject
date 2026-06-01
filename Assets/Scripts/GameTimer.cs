using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("Duration")]
    public float matchDuration = 180f;

    [Header("UI References")]
    public TMP_Text[] timerLabels;

    [Header("Timer Panel Sprites")]
    [Tooltip("SpriteRenderer green; player 1")]
    public SpriteRenderer timerPanel1Normal;
    [Tooltip("SpriteRenderer red; player 1")]
    public SpriteRenderer timerPanel1Warning;

    [Tooltip("SpriteRenderer green; player 2")]
    public SpriteRenderer timerPanel2Normal;
    [Tooltip("SpriteRenderer red; player 2")]
    public SpriteRenderer timerPanel2Warning;

    [Header("Settings")]
    public bool triggersEndGame = true;

    [Header("Countdown Audio")]
    public AudioClip tickSound;
    public AudioClip finalSound;
    [Range(0f, 1f)]
    public float beepVolume = 1f;

    private float timeRemaining;
    private bool timerRunning = false;
    private int lastWholeSecond = -1;
    private bool finalSoundPlayed = false;
    private AudioSource audioSource;
    private Color normalTextColor;

    void Start()
    {
        if (timerLabels == null || timerLabels.Length == 0)
            AutoFindTimerLabels();

        AutoFindTimerPanels();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (timerLabels != null && timerLabels.Length > 0 && timerLabels[0] != null)
            normalTextColor = timerLabels[0].faceColor;
        else
            normalTextColor = Color.white;

        timeRemaining = matchDuration;
        timerRunning = false;
        lastWholeSecond = Mathf.CeilToInt(matchDuration);
        finalSoundPlayed = false;

        SetWarningPanels(false);
        UpdateDisplay(timeRemaining);
    }

    void AutoFindTimerLabels()
    {
        var found = new System.Collections.Generic.List<TMP_Text>();
        string[] candidateNames = {"TimerText1", "TimerText2" };
        foreach (string name in candidateNames)
        {
            GameObject g = GameObject.Find(name);
            if (g != null) { TMP_Text t = g.GetComponent<TMP_Text>(); if (t != null && !found.Contains(t)) found.Add(t); }
        }

        if (found.Count > 0) timerLabels = found.ToArray();
    }

    void AutoFindTimerPanels()
    {
        // Panel 1
        if (timerPanel1Normal == null)
        {
            GameObject g = GameObject.Find("timer 1");
            if (g != null) timerPanel1Normal = g.GetComponent<SpriteRenderer>();
        }
        if (timerPanel1Warning == null)
        {
            GameObject g = GameObject.Find("timer 1 warning");
            if (g != null) timerPanel1Warning = g.GetComponent<SpriteRenderer>();
        }

        // Panel 2
        if (timerPanel2Normal == null)
        {
            GameObject g = GameObject.Find("timer 2");
            if (g != null) timerPanel2Normal = g.GetComponent<SpriteRenderer>();
        }
        if (timerPanel2Warning == null)
        {
            GameObject g = GameObject.Find("timer 2 warning");
            if (g != null) timerPanel2Warning = g.GetComponent<SpriteRenderer>();
        }
    }

    void SetWarningPanels(bool warning)
    {
        if (timerPanel1Normal != null)  timerPanel1Normal.gameObject.SetActive(!warning);
        if (timerPanel1Warning != null) timerPanel1Warning.gameObject.SetActive(warning);
        if (timerPanel2Normal != null)  timerPanel2Normal.gameObject.SetActive(!warning);
        if (timerPanel2Warning != null) timerPanel2Warning.gameObject.SetActive(warning);
    }

    void Update()
    {
        if (!timerRunning) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            timerRunning = false;
            UpdateDisplay(0f);

            if (triggersEndGame && GameManager.Instance != null)
                GameManager.Instance.EndGame();
        }
        else
        {
            UpdateDisplay(timeRemaining);

            if (timeRemaining <= 11f)
            {
                int currentSecond = Mathf.CeilToInt(timeRemaining);

                if (currentSecond == 1 && !finalSoundPlayed)
                {
                    finalSoundPlayed = true;
                    lastWholeSecond = 1;
                    PlaySound(finalSound != null ? finalSound : tickSound);
                }
                else if (currentSecond != lastWholeSecond && currentSecond > 1)
                {
                    lastWholeSecond = currentSecond;
                    PlaySound(tickSound);
                }
            }
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        audioSource.PlayOneShot(clip, beepVolume);
    }

    private void UpdateDisplay(float seconds)
    {
        bool warning = seconds <= 11f;

        if (timerLabels != null)
        {
            int mins = Mathf.FloorToInt(seconds / 60f);
            int secs = Mathf.FloorToInt(seconds % 60f);
            string text = string.Format("{0}:{1:00}", mins, secs);
            Color textCol = warning ? Color.red : normalTextColor;

            foreach (TMP_Text label in timerLabels)
            {
                if (label == null) continue;
                label.text = text;
                label.faceColor = textCol;
            }
        }

        SetWarningPanels(warning);
    }

    public void PauseTimer() => timerRunning = false;
    public void ResumeTimer() => timerRunning = true;
    public bool IsRunning => timerRunning;
    public float TimeLeft => timeRemaining;
}