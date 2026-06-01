using UnityEngine;

public class ExtinguisherGrabber : MonoBehaviour
{
    [Header("Models of the hand")]
    public GameObject maOberta;
    public GameObject maTancada;

    [Header("Extintor")]
    public GameObject extinguisherObject;
    public float grabRadius = 1.5f;
    public Vector3 carryOffset = new Vector3(0.6f, 0.1f, 0.4f);

    [Tooltip("Local rotation of the extintor")]
    public Vector3 carryRotationEuler = new Vector3(0f, 0f, 0f);

    [Header("Put down fire")]
    [Tooltip("Seconds to put down the fire")]
    public float extinguishHoldTime = 3f;

    [Header("Oven")]
    public OvenStation ovenStation;
    public float useRadius = 2.5f;

    [Header("Foam (VFX)")]
    [Tooltip("ParticleSystem foam extintor. ")]
    public ParticleSystem foamEffect;

    [Tooltip("Foam activates when player enters the oven radius")]
    public bool foamOnlyWhileNearOven = true;

    [Header("Foam Sound")]
    public AudioSource foamAudioSource;
    public AudioClip foamClip;

    [Header("Burnt pizza")]
    public float burnedPickupRadius = 1.5f;
    public Vector3 burnedCarryOffset = new Vector3(0.5f, 0f, 0.5f);

    [Header("Trash")]
    public TrashBin myTrashBin;
    public float trashRadius = 2f;
    public float trashHoldTime = 0.5f;

    [Header("Pickup sound")]
    public AudioSource pickupBurnedAudioSource;
    public AudioClip pickupBurnedClip;


    enum State
    {
        Idle,
        HoldingExtinguisher,
        WaitingBurned,
        CarryingBurned
    }
    State _state = State.Idle;

    PlayerHandState _handState;
    GameObject _burnedPizza = null;
    float _trashTimer = 0f;
    float _extinguishTimer = 0f;
    bool _foamPlaying = false;

    Vector3 _extOriginalPosition;
    Quaternion _extOriginalRotation;
    Transform _extOriginalParent;



    void Awake()
    {
        _handState = GetComponent<PlayerHandState>()
                  ?? GetComponentInParent<PlayerHandState>();
    }

    void Start()
    {
        if (extinguisherObject != null)
        {
            _extOriginalPosition = extinguisherObject.transform.position;
            _extOriginalRotation = extinguisherObject.transform.rotation;
            _extOriginalParent = extinguisherObject.transform.parent;
        }

        StopFoam();
    }

    void Update()
    {
        switch (_state)
        {
            case State.Idle: UpdateIdle(); break;
            case State.HoldingExtinguisher: UpdateHoldingExtinguisher(); break;
            case State.WaitingBurned: UpdateWaitingBurned(); break;
            case State.CarryingBurned: UpdateCarryingBurned(); break;
        }
    }



    void UpdateIdle()
    {
        if (ovenStation == null || !ovenStation.IsOnFire) return;
        if (extinguisherObject == null) return;
        if (!extinguisherObject.activeInHierarchy) return;
        if (V2Dist(transform.position, extinguisherObject.transform.position) > grabRadius) return;
        if (_handState != null && !_handState.Occupy(this)) return;
        extinguisherObject.transform.SetParent(transform, worldPositionStays: false);
        extinguisherObject.transform.localPosition = carryOffset;
        extinguisherObject.transform.localRotation = Quaternion.Euler(carryRotationEuler);

        _extinguishTimer = 0f;
        _state = State.HoldingExtinguisher;

        if (maOberta != null) maOberta.SetActive(false);
        if (maTancada != null) maTancada.SetActive(true);

        Debug.Log("[ExtinguisherGrabber] Extintor grabbed!");
    }



    void UpdateHoldingExtinguisher()
    {
        if (extinguisherObject != null)
            extinguisherObject.transform.localPosition = carryOffset;

        if (ovenStation == null || !ovenStation.IsOnFire)
        {
            StopFoam();
            ReturnExtinguisher();
            return;
        }

        bool nearOven = V2Dist(transform.position, ovenStation.transform.position) <= useRadius;

        if (nearOven)
        {
            
            PlayFoam();

            _extinguishTimer += Time.deltaTime;
            Debug.Log("[ExtinguisherGrabber] Putting down... " + _extinguishTimer.ToString("F1") + " / " + extinguishHoldTime);

            if (_extinguishTimer >= extinguishHoldTime)
                UseExtinguisher();
        }
        else
        {
            // Para l'escuma si s'allunya
            StopFoam();

            if (_extinguishTimer > 0f)
            {
                _extinguishTimer = 0f;
                Debug.Log("[ExtinguisherGrabber] You are far away from the fire, come closer");
            }
        }
    }

    void UseExtinguisher()
    {
        if (!ovenStation.TryExtinguish()) return;

        _extinguishTimer = 0f;
        StopFoam();

        ReturnExtinguisher();

        _state = State.WaitingBurned;

        Debug.Log("[ExtinguisherGrabber] Fire put down, pickup the burnt pizza");
    }



    void UpdateWaitingBurned()
    {
        if (ovenStation == null) return;
        if (ovenStation.BurnedPizzaInWorld == null) return;

        if (V2Dist(transform.position, ovenStation.BurnedPizzaInWorld.transform.position) > burnedPickupRadius) return;
        if (_handState != null && !_handState.Occupy(this)) return;

        _burnedPizza = ovenStation.TryPickupBurned();
        if (_burnedPizza == null) return;

        if (pickupBurnedAudioSource != null && pickupBurnedClip != null)
            pickupBurnedAudioSource.PlayOneShot(pickupBurnedClip);

        _trashTimer = 0f;
        _state = State.CarryingBurned;

        if (maOberta != null) maOberta.SetActive(false);
        if (maTancada != null) maTancada.SetActive(true);

        Debug.Log("[ExtinguisherGrabber] Burnt pizza grabbed, throw it away");
    }



    void UpdateCarryingBurned()
    {
        if (_burnedPizza != null)
            _burnedPizza.transform.position = transform.position + burnedCarryOffset;

        if (myTrashBin == null) return;

        if (V2Dist(transform.position, myTrashBin.transform.position) <= trashRadius)
        {
            myTrashBin.SetLidOpen(true);
            _trashTimer += Time.deltaTime;
            if (_trashTimer >= trashHoldTime) DisposeBurnedPizza();
        }
        else
        {
            myTrashBin.SetLidOpen(false);
            _trashTimer = 0f;
        }
    }

    void DisposeBurnedPizza()
    {
        if (myTrashBin != null) myTrashBin.SetLidOpen(false);
        _trashTimer = 0f;

        if (_burnedPizza != null) { Destroy(_burnedPizza); _burnedPizza = null; }

        if (_handState != null) _handState.Release(this);
        if (maOberta != null) maOberta.SetActive(true);
        if (maTancada != null) maTancada.SetActive(false);

        _state = State.Idle;

        Debug.Log("[ExtinguisherGrabber] Burnt pizza in the trash");
    }



    void PlayFoam()
    {
        if (_foamPlaying) return;
        _foamPlaying = true;

        if (foamEffect != null)
            foamEffect.Play();

        if (foamAudioSource != null)
        {
            if (foamClip != null) foamAudioSource.clip = foamClip;
            foamAudioSource.loop = true;
            if (!foamAudioSource.isPlaying) foamAudioSource.Play();
        }
    }

    void StopFoam()
    {
        if (!_foamPlaying) return;
        _foamPlaying = false;

        if (foamEffect != null)
            foamEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        if (foamAudioSource != null && foamAudioSource.isPlaying)
            foamAudioSource.Stop();
    }



    void ReturnExtinguisher()
    {
        if (extinguisherObject != null)
        {
            extinguisherObject.transform.SetParent(_extOriginalParent, worldPositionStays: false);
            extinguisherObject.transform.position = _extOriginalPosition;
            extinguisherObject.transform.rotation = _extOriginalRotation;
        }

        if (_handState != null) _handState.Release(this);
        if (maOberta != null) maOberta.SetActive(true);
        if (maTancada != null) maTancada.SetActive(false);

        _state = State.Idle;
    }



    float V2Dist(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x, dz = a.z - b.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }
}