using UnityEngine;

public class SalsaStation : MonoBehaviour
{
    [Header("Referència al disc de salsa")]
    public GameObject salsaDisc;

    [Header("Referència a la llauna oberta")]
    public GameObject openCan;

    [Header("Referència a la llauna tancada original")]
    public GameObject closedCan;

    [Header("Graus totals necessaris per completar (720 = 2 voltes)")]
    public float degreesRequired = 720f;

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

        if (degreesAccumulated >= degreesRequired)
        {
            CompleteSalsa();
        }
    }

    void CompleteSalsa()
    {
        salsaDone = true;

        // Disc de salsa queda al tamany final
        if (salsaDisc != null)
        {
            salsaDisc.SetActive(true);
            salsaDisc.transform.localScale = new Vector3(0.7f, 0.01f, 0.7f);
        }

        // Amaguem la llauna oberta
        if (openCan != null)
            openCan.SetActive(false);

        // Tornem a mostrar la llauna tancada al lloc inicial
        if (closedCan != null)
        {
            closedCan.SetActive(true);

            // Resetegem també el CanOpener perquè torni a funcionar
            CanOpener canOpener = closedCan.GetComponent<CanOpener>();
            if (canOpener != null)
            {
                canOpener.ResetCan();
            }
        }

        Debug.Log("Salsa completada!");
    }
}