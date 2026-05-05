using UnityEngine;

public class CanGrabber : MonoBehaviour
{
    [Header("Radi per agafar la llauna")]
    public float grabRadius = 1.5f;

    [Header("Radi per considerar que gires al voltant de la massa")]
    public float salsaRadius = 3f;

    [Header("Models de la Mà")]
    public GameObject maOberta;
    public GameObject maTancada;

    [Header("Objectes propis d'aquest jugador")]
    public FollowPlayer myOpenCan;
    public SalsaStation mySalsaStation;

    private FollowPlayer currentCan = null;
    private SalsaStation currentStation = null;
    private Vector3 lastPosition;

    void Update()
    {
        if (currentCan == null)
        {
            TryGrabCan();
            return;
        }

        if (currentStation == null)
        {
            TryFindSalsaStation();
            return;
        }

        if (!currentStation.IsSalsaDone)
        {
            DetectCircularMotion();
        }
        else
        {
            ReleaseCan();
        }
    }

    void TryGrabCan()
    {
        if (myOpenCan == null) return;
        if (!myOpenCan.gameObject.activeInHierarchy) return;
        if (myOpenCan.IsFollowing) return;

        Vector2 playerXZ = new Vector2(transform.position.x, transform.position.z);
        Vector2 canXZ = new Vector2(myOpenCan.transform.position.x, myOpenCan.transform.position.z);

        if (Vector2.Distance(playerXZ, canXZ) <= grabRadius)
        {
            currentCan = myOpenCan;
            currentCan.StartFollowing(transform);

            if (maOberta != null) maOberta.SetActive(false);
            if (maTancada != null) maTancada.SetActive(true);
        }
    }

    void TryFindSalsaStation()
    {
        if (mySalsaStation == null) return;
        if (mySalsaStation.IsSalsaDone) return;

        Vector2 playerXZ = new Vector2(transform.position.x, transform.position.z);
        Vector2 stationXZ = new Vector2(mySalsaStation.transform.position.x, mySalsaStation.transform.position.z);

        if (Vector2.Distance(playerXZ, stationXZ) <= salsaRadius)
        {
            currentStation = mySalsaStation;
            lastPosition = transform.position;
        }
    }

    void DetectCircularMotion()
    {
        Vector3 stationPos = currentStation.transform.position;

        Vector2 prevDir = new Vector2(
            lastPosition.x - stationPos.x,
            lastPosition.z - stationPos.z
        ).normalized;

        Vector2 currDir = new Vector2(
            transform.position.x - stationPos.x,
            transform.position.z - stationPos.z
        ).normalized;

        float angle = Vector2.Angle(prevDir, currDir);

        if (angle > 0.5f)
            currentStation.AddRotation(angle);

        lastPosition = transform.position;
    }

    void ReleaseCan()
    {
        if (currentCan != null)
        {
            currentCan.StopFollowing();
            currentCan = null;
        }
        currentStation = null;

        if (maOberta != null) maOberta.SetActive(true);
        if (maTancada != null) maTancada.SetActive(false);
    }
}