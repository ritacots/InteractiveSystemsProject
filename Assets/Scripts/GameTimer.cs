using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    
    [Header("Duration")]
    [Tooltip("Match length in seconds. 180 = 3 minutes.")]
    public float matchDuration = 180f;

    [Header("UI Reference")]
    [Tooltip("Drag your TMP_Text 'TimerText' object here.")]
    public TMP_Text timerLabel;

    private float timeRemaining;
    private bool timerRunning = false;


    void Start()
    {
        timeRemaining = matchDuration;
        timerRunning = true;       
        UpdateDisplay(timeRemaining); 
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

            GameManager.Instance.EndGame();
        }
        else
        {
            UpdateDisplay(timeRemaining);
        }
    }

   
    private void UpdateDisplay(float seconds)
    {
        if (timerLabel == null) return;

        int mins = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        timerLabel.text = string.Format("{0}:{1:00}", mins, secs);

      
        timerLabel.color = (seconds <= 10f) ? Color.red : Color.white;
    }



    public void PauseTimer() => timerRunning = false;
    public void ResumeTimer() => timerRunning = true;
    public bool IsRunning => timerRunning;
    public float TimeLeft => timeRemaining;
}