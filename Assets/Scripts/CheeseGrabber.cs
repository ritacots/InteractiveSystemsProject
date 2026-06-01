using UnityEngine;

public class CheeseGrabber : MonoBehaviour
{
    [Header("Detection radius")]
    public float grabRadius = 1.5f;
    public float spreadRadius = 3f;

    [Header("Models of the hand")]
    public GameObject maOberta;
    public GameObject maTancada;

    [Header("Reference to the CheeseSpreader (Dough)")]
    public CheeseSpreader myCheeseSpreader;

    [Header("Referencia to SalsaStation")]
    public SalsaStation mySalsaStation;

    [Header("Visual blocking indicator")]
    public GameObject blockedIndicator;

    [Header("Offset of the cheese")]
    public Vector3 cheeseOffset = new Vector3(0.5f, 0f, 0.5f);

    [Header("Sound grab")]
    public AudioSource grabAudioSource;
    public AudioClip grabClip;

    [Header("Trash Bin Disposal")]
    public TrashBin myTrashBin;
    public float trashRadius = 2f;
    public float trashHoldTime = 0.5f;

    private PlayerHandState _handState;
    private GameObject heldCheese = null;
    private bool isSpreading = false;
    private Vector3 lastPosition;
    private float trashTimer = 0f;

    void Awake()
    {
        _handState = GetComponent<PlayerHandState>() ?? GetComponentInParent<PlayerHandState>();
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameStarted) return;
        if (GameManager.Instance.IsGameOver) return;

        if (heldCheese != null)
            heldCheese.transform.position = transform.position + cheeseOffset;

        if (heldCheese != null && myTrashBin != null)
        {
            Vector2 cheeseXZ = new Vector2(heldCheese.transform.position.x, heldCheese.transform.position.z);
            Vector2 trashXZ = new Vector2(myTrashBin.transform.position.x, myTrashBin.transform.position.z);
            if (Vector2.Distance(cheeseXZ, trashXZ) <= trashRadius)
            {
                myTrashBin.SetLidOpen(true);
                trashTimer += Time.deltaTime;
                if (trashTimer >= trashHoldTime) DisposeIntoTrash();
                return;
            }
            myTrashBin.SetLidOpen(false);
            trashTimer = 0f;
        }

        if (heldCheese == null) { TryGrabCheese(); return; }
        if (myCheeseSpreader == null) return;
        if (myCheeseSpreader.IsSpreadDone) { ReleaseCheese(); return; }
        if (!isSpreading) { TryStartSpreading(); return; }
        DetectCircularMotion();
        if (myCheeseSpreader.IsSpreadDone) ReleaseCheese();
    }

    void TryGrabCheese()
    {
        if (myCheeseSpreader != null && myCheeseSpreader.IsSpreadDone) return;

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Topping"))
        {
            if (!obj.name.ToLower().Contains("cheese")) continue;
            if (!obj.activeInHierarchy) continue;
            if (obj.GetComponent<CheeseBeingHeld>() != null) continue;

            Vector2 playerXZ = new Vector2(transform.position.x, transform.position.z);
            Vector2 cheeseXZ = new Vector2(obj.transform.position.x, obj.transform.position.z);
            if (Vector2.Distance(playerXZ, cheeseXZ) > grabRadius) continue;
            if (_handState != null && !_handState.Occupy(this)) return;

            heldCheese = obj;
            heldCheese.SetActive(true);
            IngredientLifetime lt = heldCheese.GetComponent<IngredientLifetime>();
            if (lt != null) Destroy(lt);
            heldCheese.AddComponent<CheeseBeingHeld>();
            isSpreading = false;
            trashTimer = 0f;
            if (grabAudioSource != null && grabClip != null)
                grabAudioSource.PlayOneShot(grabClip);
            if (maOberta != null) maOberta.SetActive(false);
            if (maTancada != null) maTancada.SetActive(true);
            return;
        }
    }

    void DisposeIntoTrash()
    {
        if (myTrashBin != null) myTrashBin.SetLidOpen(false);
        trashTimer = 0f;
        if (heldCheese != null)
        {
            var m = heldCheese.GetComponent<CheeseBeingHeld>();
            if (m != null) Destroy(m);
            Destroy(heldCheese);
            heldCheese = null;
        }
        isSpreading = false;
        HideBlockedIndicator();
        if (_handState != null) _handState.Release(this);
        if (maOberta != null) maOberta.SetActive(true);
        if (maTancada != null) maTancada.SetActive(false);
    }

    void ReleaseCheese()
    {
        if (heldCheese != null)
        {
            var m = heldCheese.GetComponent<CheeseBeingHeld>();
            if (m != null) Destroy(m);
            heldCheese.SetActive(false);
            heldCheese = null;
        }
        isSpreading = false;
        HideBlockedIndicator();
        if (myTrashBin != null) myTrashBin.SetLidOpen(false);
        if (_handState != null) _handState.Release(this);
        if (maOberta != null) maOberta.SetActive(true);
        if (maTancada != null) maTancada.SetActive(false);
    }

    void TryStartSpreading()
    {
        Vector2 playerXZ = new Vector2(transform.position.x, transform.position.z);
        Vector2 spreaderXZ = new Vector2(myCheeseSpreader.transform.position.x, myCheeseSpreader.transform.position.z);
        if (Vector2.Distance(playerXZ, spreaderXZ) <= spreadRadius)
        {
            if (mySalsaStation != null && !mySalsaStation.IsSalsaDone) { ShowBlockedIndicator(); return; }
            HideBlockedIndicator();
            isSpreading = true;
            lastPosition = transform.position;
        }
        else HideBlockedIndicator();
    }

    void DetectCircularMotion()
    {
        Vector3 c = myCheeseSpreader.transform.position;
        Vector2 prevDir = new Vector2(lastPosition.x - c.x, lastPosition.z - c.z);
        Vector2 currDir = new Vector2(transform.position.x - c.x, transform.position.z - c.z);
        float deg = Mathf.Abs(Vector2.SignedAngle(prevDir, currDir));
        if (deg > 0.1f) myCheeseSpreader.AddRotation(deg);
        lastPosition = transform.position;
    }

    void ShowBlockedIndicator() { if (blockedIndicator != null) blockedIndicator.SetActive(true); }
    void HideBlockedIndicator() { if (blockedIndicator != null) blockedIndicator.SetActive(false); }
}

public class CheeseBeingHeld : MonoBehaviour { }