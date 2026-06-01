using UnityEngine;

public class SalsaStation : MonoBehaviour
{
    [Header("Reference salsa disc")]
    public GameObject salsaDisc;

    [Header("Reference open can")]
    public GameObject openCan;

    [Header("Reference closed can")]
    public GameObject closedCan;

    [Header("Total degrees (720 = 2 laps)")]
    public float degreesRequired = 720f;

    [Header("Spreading sound")]
    public AudioSource audioSource;
    public AudioClip spreadingClip;
    public AudioClip completeClip;

    private float degreesAccumulated = 0f;
    private bool salsaDone = false;

    public bool IsSalsaDone => salsaDone;

    public void AddRotation(float degrees)
    {
        if (salsaDone) return;

        degreesAccumulated += degrees;

        if (salsaDisc != null)
        {
            salsaDisc.SetActive(true);
            float progress = Mathf.Clamp01(degreesAccumulated / degreesRequired);
            float scale = Mathf.Lerp(0.1f, 0.7f, progress);
            salsaDisc.transform.localScale = new Vector3(scale, 0.01f, scale);
        }

        if (audioSource != null && spreadingClip != null && !audioSource.isPlaying)
        {
            audioSource.clip = spreadingClip;
            audioSource.loop = true;
            audioSource.Play();
        }

        if (degreesAccumulated >= degreesRequired)
            CompleteSalsa();
    }

    public void StopSpreadingSound()
    {
        if (audioSource != null && audioSource.isPlaying && audioSource.clip == spreadingClip)
            audioSource.Stop();
    }

    void CompleteSalsa()
    {
        salsaDone = true;

        if (audioSource != null)
        {
            audioSource.loop = false;
            audioSource.Stop();
            if (completeClip != null) { audioSource.clip = completeClip; audioSource.Play(); }
        }

        if (salsaDisc != null)
        {
            salsaDisc.SetActive(true);
            salsaDisc.transform.localScale = new Vector3(0.7f, 0.01f, 0.7f);
        }

        if (openCan != null) openCan.SetActive(false);

        if (closedCan != null)
        {
            closedCan.SetActive(true);
            CanOpener canOpener = closedCan.GetComponent<CanOpener>();
            if (canOpener != null) canOpener.ResetCan();
        }

        Debug.Log("Complete salsa!");
    }

    public void ResetStation()
    {
        degreesAccumulated = 0f;
        salsaDone = false;

        if (audioSource != null) { audioSource.loop = false; audioSource.Stop(); }

        if (salsaDisc != null)
        {
            salsaDisc.SetActive(false);
            salsaDisc.transform.localScale = new Vector3(0.1f, 0.01f, 0.1f);
        }

        if (openCan != null) openCan.SetActive(false);

        if (closedCan != null)
        {
            closedCan.SetActive(true);
            CanOpener canOpener = closedCan.GetComponent<CanOpener>();
            if (canOpener != null) canOpener.ResetCan();
        }

        Debug.Log("[SalsaStation] Reset for new pizza.");
    }
}