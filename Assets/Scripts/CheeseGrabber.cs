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

    [Header("Offset del formatge respecte al jugador")]
    public Vector3 cheeseOffset = new Vector3(0.5f, 0f, 0.5f);

    private GameObject heldCheese = null;
    private bool isSpreading = false;
    private Vector3 lastPosition;


    void Update()
    {

        if (heldCheese != null)
        {
            heldCheese.transform.position = new Vector3(
                transform.position.x + cheeseOffset.x,
                heldCheese.transform.position.y,
                transform.position.z + cheeseOffset.z
            );
        }

        if (heldCheese == null)
        {
            TryGrabCheese();
            return;
        }

        if (myCheeseSpreader == null)
            return;

        // Spreading already finished: hide the cheese now that spreading is done
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
                {
                    myCheeseSpreader.ResetSpreader();
                }

                isSpreading = false;

                if (maOberta != null) maOberta.SetActive(false);
                if (maTancada != null) maTancada.SetActive(true);
                return;
            }
        }
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
            isSpreading = true;
            lastPosition = transform.position;
        }
    }


    void DetectCircularMotion()
    {
        Vector3 centre = myCheeseSpreader.transform.position;

        Vector2 prevDir = new Vector2(
            lastPosition.x - centre.x,
            lastPosition.z - centre.z).normalized;

        Vector2 currDir = new Vector2(
            transform.position.x - centre.x,
            transform.position.z - centre.z).normalized;

        float angle = Vector2.Angle(prevDir, currDir);
        if (angle > 0.5f)
            myCheeseSpreader.AddRotation(angle);

        lastPosition = transform.position;
    }

}

public class CheeseBeingHeld : MonoBehaviour { }