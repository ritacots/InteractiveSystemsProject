using UnityEngine;

public class CheeseGrabber : MonoBehaviour
{
    [Header("Radis de deteccio")]
    public float grabRadius = 1.5f;
    public float spreadRadius = 3f;

    [Header("Models de la ma")]
    public GameObject maOberta;
    public GameObject maTancada;

    [Header("Referencia al CheeseSpreader (Dough)")]
    public CheeseSpreader myCheeseSpreader;

    [Header("Referencia a la SalsaStation (requerida per posar el formatge)")]
    public SalsaStation mySalsaStation;

    [Header("Indicador visual de bloqueig (creu o similar)")]
    public GameObject blockedIndicator;

    [Header("Offset del formatge respecte al jugador")]
    public Vector3 cheeseOffset = new Vector3(0.5f, 0f, 0.5f);

    [Header("Soltar el formatge - moviment cap avall brusc")]
    [Tooltip("Velocitat cap avall (m/s) necessaria per soltar el formatge.")]
    public float dropVelocityThreshold = 0.8f;

    private GameObject heldCheese = null;
    private bool isSpreading = false;
    private Vector3 lastPosition;
    private float lastY;
    private bool wasBlocked = false;

    void Update()
    {
        if (heldCheese != null)
        {
            float velocityY = (transform.position.y - lastY) / Time.deltaTime;

            if (velocityY < -dropVelocityThreshold)
            {
                DropCheese();
                lastY = transform.position.y;
                return;
            }

            heldCheese.transform.position = new Vector3(
                transform.position.x + cheeseOffset.x,
                transform.position.y + cheeseOffset.y,
                transform.position.z + cheeseOffset.z
            );
        }

        lastY = transform.position.y;

        if (heldCheese == null)
        {
            TryGrabCheese();
            return;
        }

        if (myCheeseSpreader == null)
            return;

        if (myCheeseSpreader.IsSpreadDone)
        {
            ReleaseCheese();
            return;
        }

        if (!isSpreading)
        {
            TryStartSpreading();
            return;
        }

        DetectCircularMotion();

        if (myCheeseSpreader.IsSpreadDone)
            ReleaseCheese();
    }

    void TryGrabCheese()
    {
        GameObject[] toppings = GameObject.FindGameObjectsWithTag("Topping");
        foreach (GameObject obj in toppings)
        {
            if (!obj.name.ToLower().Contains("cheese")) continue;
            if (!obj.activeInHierarchy) continue;
            if (obj.GetComponent<CheeseBeingHeld>() != null) continue;

            Vector2 playerXZ = new Vector2(transform.position.x, transform.position.z);
            Vector2 cheeseXZ = new Vector2(obj.transform.position.x, obj.transform.position.z);

            if (Vector2.Distance(playerXZ, cheeseXZ) <= grabRadius)
            {
                heldCheese = obj;
                heldCheese.SetActive(true);
                heldCheese.AddComponent<CheeseBeingHeld>();

                if (myCheeseSpreader != null && myCheeseSpreader.IsSpreadDone)
                    myCheeseSpreader.ResetSpreader();

                isSpreading = false;
                lastY = transform.position.y;
                wasBlocked = false;

                if (maOberta != null) maOberta.SetActive(false);
                if (maTancada != null) maTancada.SetActive(true);
                return;
            }
        }
    }

    void DropCheese()
    {
        if (heldCheese != null)
        {
            CheeseBeingHeld marker = heldCheese.GetComponent<CheeseBeingHeld>();
            if (marker != null) Destroy(marker);

            heldCheese.transform.position = new Vector3(
                transform.position.x + cheeseOffset.x,
                heldCheese.transform.position.y,
                transform.position.z + cheeseOffset.z
            );

            heldCheese.SetActive(true);
            heldCheese = null;
        }

        isSpreading = false;
        wasBlocked = false;
        HideBlockedIndicator();

        if (maOberta != null) maOberta.SetActive(true);
        if (maTancada != null) maTancada.SetActive(false);
    }

    void ReleaseCheese()
    {
        if (heldCheese != null)
        {
            CheeseBeingHeld marker = heldCheese.GetComponent<CheeseBeingHeld>();
            if (marker != null) Destroy(marker);
            heldCheese.SetActive(false);
            heldCheese = null;
        }

        isSpreading = false;
        wasBlocked = false;
        HideBlockedIndicator();

        if (maOberta != null) maOberta.SetActive(true);
        if (maTancada != null) maTancada.SetActive(false);
    }

    void TryStartSpreading()
    {
        Vector2 playerXZ = new Vector2(transform.position.x, transform.position.z);
        Vector2 spreaderXZ = new Vector2(myCheeseSpreader.transform.position.x,
                                         myCheeseSpreader.transform.position.z);

        if (Vector2.Distance(playerXZ, spreaderXZ) <= spreadRadius)
        {
            if (mySalsaStation != null && !mySalsaStation.IsSalsaDone)
            {
                ShowBlockedIndicator();
                return;
            }

            HideBlockedIndicator();
            isSpreading = true;
            lastPosition = transform.position;
        }
        else
        {
            HideBlockedIndicator();
        }
    }

    void ShowBlockedIndicator()
    {
        if (blockedIndicator != null)
            blockedIndicator.SetActive(true);

        wasBlocked = true;
    }

    void HideBlockedIndicator()
    {
        if (blockedIndicator != null)
            blockedIndicator.SetActive(false);

        wasBlocked = false;
    }

    void DetectCircularMotion()
    {
        Vector3 centre = myCheeseSpreader.transform.position;

        Vector2 prevDir = new Vector2(lastPosition.x - centre.x, lastPosition.z - centre.z);
        Vector2 currDir = new Vector2(transform.position.x - centre.x, transform.position.z - centre.z);

        float signedAngle = Vector2.SignedAngle(prevDir, currDir);
        float absDeg = Mathf.Abs(signedAngle);

        if (absDeg > 0.1f)
            myCheeseSpreader.AddRotation(absDeg);

        lastPosition = transform.position;
    }
}

public class CheeseBeingHeld : MonoBehaviour { }