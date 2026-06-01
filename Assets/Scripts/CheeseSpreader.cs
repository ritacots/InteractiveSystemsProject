using UnityEngine;

public class CheeseSpreader : MonoBehaviour
{
    [Header("Disc cheese")]
    public GameObject cheeseDisc;

    [Header("Total degrees (720 = 2 laps)")]
    public float degreesRequired = 720f;

    [Header("Spreading sound")]
    [Tooltip("AudioSource with the spreading/squish clip assigned — Play On Awake off")]
    public AudioSource spreadingAudioSource;
    public AudioClip spreadingClip;
    [Tooltip("Short sound played when cheese is fully spread")]
    public AudioSource completeAudioSource;
    public AudioClip completeClip;

    private float degreesAccumulated = 0f;
    private bool spreadDone = false;

    public bool IsSpreadDone => spreadDone;

    public void AddRotation(float degrees)
    {
        if (spreadDone) return;

        degreesAccumulated += degrees;

        if (cheeseDisc != null)
        {
            cheeseDisc.SetActive(true);
            float progress = Mathf.Clamp01(degreesAccumulated / degreesRequired);
            float scale = Mathf.Lerp(0.05f, 0.6f, progress);
            cheeseDisc.transform.localScale = new Vector3(scale, 0.01f, scale);
        }

        if (spreadingAudioSource != null && spreadingClip != null && !spreadingAudioSource.isPlaying)
        {
            spreadingAudioSource.clip = spreadingClip;
            spreadingAudioSource.loop = true;
            spreadingAudioSource.Play();
        }

        if (degreesAccumulated >= degreesRequired)
            CompleteSpread();
    }

    public void StopSpreadingSound()
    {
        if (spreadingAudioSource != null && spreadingAudioSource.isPlaying
            && spreadingAudioSource.clip == spreadingClip)
        {
            spreadingAudioSource.loop = false;
            spreadingAudioSource.Stop();
        }
    }

    void CompleteSpread()
    {
        spreadDone = true;

        if (spreadingAudioSource != null)
        {
            spreadingAudioSource.loop = false;
            spreadingAudioSource.Stop();
        }
        if (completeAudioSource != null && completeClip != null)
        {
            completeAudioSource.clip = completeClip;
            completeAudioSource.Play();
        }

        if (cheeseDisc != null)
        {
            cheeseDisc.SetActive(true);
            cheeseDisc.transform.localScale = new Vector3(0.9f, 0.01f, 0.9f);
        }

        Debug.Log("[CheeseSpreader] Cheese spread correctly!");
    }

    public void ResetSpreader()
    {
        degreesAccumulated = 0f;
        spreadDone = false;

        if (spreadingAudioSource != null) { spreadingAudioSource.loop = false; spreadingAudioSource.Stop(); }

        if (cheeseDisc != null)
        {
            cheeseDisc.SetActive(false);
            cheeseDisc.transform.localScale = new Vector3(0.05f, 0.01f, 0.05f);
        }
    }
}