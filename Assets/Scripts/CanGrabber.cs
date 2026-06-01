using UnityEngine;

public class CanGrabber : MonoBehaviour
{
    [Header("Radius can")]
    public float grabRadius = 1.5f;

    [Header("Radius for laps")]
    public float salsaRadius = 3f;

    [Header("Models of the hand")]
    public GameObject maOberta;
    public GameObject maTancada;

    [Header("Objects of the player")]
    public FollowPlayer myOpenCan;
    public SalsaStation mySalsaStation;

    [Header("Trash Bin Disposal")]
    public TrashBin myTrashBin;
    public float trashRadius = 2f;
    public float trashHoldTime = 0.5f;
    public CanOpener closedCan;

    private PlayerHandState _handState;
    private FollowPlayer currentCan = null;
    private SalsaStation currentStation = null;
    private Vector3 lastPosition;
    private float trashTimer = 0f;

    void Awake()
    {
        _handState = GetComponent<PlayerHandState>() ?? GetComponentInParent<PlayerHandState>();
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameStarted) return;

        if (currentCan == null)
        {
            TryGrabCan();
            return;
        }
        
        if (myTrashBin != null)
        {
            Vector2 canXZ   = new Vector2(currentCan.transform.position.x, currentCan.transform.position.z);
            Vector2 trashXZ = new Vector2(myTrashBin.transform.position.x,  myTrashBin.transform.position.z);
            if (Vector2.Distance(canXZ, trashXZ) <= trashRadius)
            {
                myTrashBin.SetLidOpen(true);
                trashTimer += Time.deltaTime;
                if (trashTimer >= trashHoldTime) DisposeCanIntoTrash();
                return;
            }
            myTrashBin.SetLidOpen(false);
            trashTimer = 0f;
        }

        if (currentStation == null) { TryFindSalsaStation(); return; }
        if (!currentStation.IsSalsaDone) DetectCircularMotion();
        else ReleaseCan();
    }

    void TryGrabCan()
    {
        if (myOpenCan == null || !myOpenCan.gameObject.activeInHierarchy || myOpenCan.IsFollowing) return;

        Vector2 playerXZ = new Vector2(transform.position.x, transform.position.z);
        Vector2 canXZ    = new Vector2(myOpenCan.transform.position.x, myOpenCan.transform.position.z);
        if (Vector2.Distance(playerXZ, canXZ) > grabRadius) return;

        if (_handState != null && !_handState.Occupy(this)) return;

        currentCan = myOpenCan;
        currentCan.StartFollowing(transform);
        trashTimer = 0f;
        if (maOberta  != null) maOberta.SetActive(false);
        if (maTancada != null) maTancada.SetActive(true);
    }

    void DisposeCanIntoTrash()
    {
        if (myTrashBin != null) myTrashBin.SetLidOpen(false);
        trashTimer = 0f;
        if (currentCan != null) { currentCan.StopFollowing(); currentCan.gameObject.SetActive(false); currentCan = null; }
        if (closedCan  != null) { closedCan.gameObject.SetActive(true); closedCan.ResetCan(); }
        currentStation = null;
        if (_handState != null) _handState.Release(this);
        if (maOberta  != null) maOberta.SetActive(true);
        if (maTancada != null) maTancada.SetActive(false);
    }

    void ReleaseCan()
    {
        if (currentCan != null) { currentCan.StopFollowing(); currentCan = null; }
        currentStation = null;
        if (myTrashBin != null) myTrashBin.SetLidOpen(false);
        if (_handState != null) _handState.Release(this);
        if (maOberta  != null) maOberta.SetActive(true);
        if (maTancada != null) maTancada.SetActive(false);
    }

    void TryFindSalsaStation()
    {
        if (mySalsaStation == null || mySalsaStation.IsSalsaDone) return;
        Vector2 playerXZ  = new Vector2(transform.position.x, transform.position.z);
        Vector2 stationXZ = new Vector2(mySalsaStation.transform.position.x, mySalsaStation.transform.position.z);
        if (Vector2.Distance(playerXZ, stationXZ) <= salsaRadius)
        {
            currentStation = mySalsaStation;
            lastPosition = transform.position;
        }
    }

    void DetectCircularMotion()
    {
        Vector3 sp      = currentStation.transform.position;
        Vector2 prevDir = new Vector2(lastPosition.x - sp.x, lastPosition.z - sp.z).normalized;
        Vector2 currDir = new Vector2(transform.position.x - sp.x, transform.position.z - sp.z).normalized;
        float angle = Vector2.Angle(prevDir, currDir);
        if (angle > 0.5f) currentStation.AddRotation(angle);
        lastPosition = transform.position;
    }
}