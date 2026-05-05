using UnityEngine;

public class CheeseSpreader : MonoBehaviour
{
    [Header("Disc visual del formatge fos")]
    public GameObject cheeseDisc;

    [Header("Graus totals per completar l'extensió (720 = 2 voltes)")]
    public float degreesRequired = 720f;

    private float degreesAccumulated = 0f;
    private bool spreadDone = false;

    public bool IsSpreadDone => spreadDone;

    public void AddRotation(float degrees)
    {
        // If it's already done, we don't need to do math, 
        // but we keep the disc active.
        if (spreadDone) return;

        degreesAccumulated += degrees;

        if (cheeseDisc != null)
        {
            cheeseDisc.SetActive(true);
            float progress = Mathf.Clamp01(degreesAccumulated / degreesRequired);
            // Start from 0.05 or the current scale to avoid "shrinking" 
            float scale = Mathf.Lerp(0.05f, 0.6f, progress);
            cheeseDisc.transform.localScale = new Vector3(scale, 0.01f, scale);
        }

        if (degreesAccumulated >= degreesRequired)
            CompleteSpread();
    }

    public void ResetSpreader()
    {
        // We reset the logic so the player can "spread" again if they pick up more,
        // but we DO NOT hide the cheeseDisc anymore.
        degreesAccumulated = 0f;

        // Only reset spreadDone if you want them to be able to spread a SECOND layer.
        // If you only ever want one layer of cheese, remove the line below.
        spreadDone = false;
    }

    void CompleteSpread()
    {
        spreadDone = true;

        if (cheeseDisc != null)
        {
            cheeseDisc.SetActive(true);
            cheeseDisc.transform.localScale = new Vector3(0.6f, 0.01f, 0.6f);
        }

        Debug.Log("[CheeseSpreader] Formatge estès correctament!");
    }
}