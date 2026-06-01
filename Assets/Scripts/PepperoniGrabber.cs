using UnityEngine;
using TMPro;


public class PepperoniGrabber : MonoBehaviour
{
    [Header("Detection Radius")]
    public float grabRadius = 1.5f;
    public float placeRadius = 2.5f;

    [Header("Hand Models")]
    public GameObject maOberta;
    public GameObject maTancada;

    [Header("Pizza Dough")]
    public Transform pizzaDough;
    public GameObject pizzaDoughPrefab;
    public Transform doughSpawnPoint;
    public float pepperoniYOffset = 1.5f;
    public float pepperoniScale = 0.3f;
    public Vector2[] pepperoniSlots = new Vector2[]
    {
        new Vector2(-1.2f,  1.2f), new Vector2( 1.2f,  1.2f),
        new Vector2(-1.2f, -1.2f), new Vector2( 1.2f, -1.2f)
    };

    [Header("Carry Offsets")]
    public Vector3 pepperoniOffset = new Vector3(0.5f, 0.1f, 0.5f);
    public Vector3 pizzaCarryOffset = new Vector3(0.5f, 0f, 0.5f);

    [Header("Prerequisites")]
    public SalsaStation mySalsaStation;
    public CheeseSpreader myCheeseSpreader;

    [Header("Indicators")]
    public GameObject blockedIndicator;
    public TextMeshProUGUI counterText;
    public GameObject tickIndicator;
    [Tooltip("World-space offset relative to the dough where the counter and tick appear")]
    public Vector3 indicatorOffset = new Vector3(0f, 2.5f, 0f);
    [Tooltip("AudioSource for the tick sound when all pepperoni are placed")]
    public AudioSource tickAudioSource;
    public AudioClip tickSound;
    [Tooltip("AudioSource for the sound played each time a pepperoni is placed")]
    public AudioSource pepperoniPlaceAudioSource;
    public AudioClip pepperoniPlaceClip;

    [Header("Trash Bin")]
    public TrashBin myTrashBin;
    public float trashRadius = 2f;
    public float trashHoldTime = 0.5f;

    [Header("Oven")]
    [Tooltip("Reference to the OvenStation component in your scene.")]
    public OvenStation ovenStation;
    public float ovenRadius = 2.5f;

    [Header("Sabotage (enemy oven)")]
    [Tooltip("The opposing player's OvenStation. If assigned, this player can steal their cooked pizza.")]
    public OvenStation enemyOvenStation;
    public float stealRadius = 2.5f;

    [Header("Pickup Sounds")]
    [Tooltip("Sound when the player picks up a pepperoni.")]
    public AudioSource grabPepperoniAudioSource;
    public AudioClip grabPepperoniClip;
    [Tooltip("Sound when the player picks up the raw pizza to take it to the oven.")]
    public AudioSource pickupRawAudioSource;
    public AudioClip pickupRawClip;
    [Tooltip("Sound when the player picks up the cooked pizza from the oven.")]
    public AudioSource pickupCookedAudioSource;
    public AudioClip pickupCookedClip;

    [Header("Plate Delivery")]
    public Transform plateTransform;
    public float plateRadius = 2.5f;
    public GameObject pizzaOnPlatePrefab;
    public AudioSource plateDeliveryAudioSource;
    public AudioClip plateDeliveryClip;

    [Header("Pizza Counter")]
    public int playerNumber = 1;
    [Tooltip("World-space TextMeshPro placed above the plate. It will be shown/updated each time a pizza is delivered.")]
    public TextMeshPro pizzaCounterText3D;


    enum State
    {
        PlacingToppings,
        CarryingRawToOven,
        CarryingCookedToPlate,
        StealingEnemyPizza
    }
    State _state = State.PlacingToppings;

    int _pepperoniCount = 0;
    const int REQUIRED_PEPPERONI = 4;
    GameObject _heldPepperoni = null;
    float _trashTimer = 0f;
    GameObject _carriedCookedPizza = null;
    GameObject _carriedStolenPizza = null;
    int _pizzasCompleted = 0;
    PlayerHandState _handState;

    void Awake()
    {
        _handState = GetComponent<PlayerHandState>()
                  ?? GetComponentInParent<PlayerHandState>();
    }

    void Start()
    {
        SA(counterText, false);
        SA(blockedIndicator, false);
        SA(tickIndicator, false);
        UpdatePizzaCounterUI();
    }

    void Update()
    {
        if (!GameManager.Instance.IsGameStarted) return;
        if (GameManager.Instance.IsGameOver) return;

        switch (_state)
        {
            case State.PlacingToppings:
                UpdatePlacingToppings();
                break;
            case State.CarryingRawToOven:
                UpdateCarryingRawToOven();
                break;
            case State.CarryingCookedToPlate:
                UpdateCarryingCookedToPlate();
                break;
            case State.StealingEnemyPizza:
                UpdateStealingEnemyPizza();
                break;
        }

        if (_state != State.CarryingCookedToPlate && _state != State.StealingEnemyPizza)
        {
            CheckOvenPickup();
            CheckEnemyOvenPickup();
        }

        if (pizzaDough != null)
        {
            Vector3 targetPos = pizzaDough.position + indicatorOffset;
            if (tickIndicator != null)
                tickIndicator.transform.position = targetPos;
            if (counterText != null)
            {
                Canvas c = counterText.GetComponentInParent<Canvas>();
                if (c != null) c.transform.position = targetPos;
            }
        }
    }


    void UpdatePlacingToppings()
    {
        if (_heldPepperoni != null)
        {
            _heldPepperoni.transform.position = transform.position + pepperoniOffset;

            if (myTrashBin != null)
            {
                if (XZ(_heldPepperoni.transform.position, myTrashBin.transform.position) <= trashRadius)
                {
                    myTrashBin.SetLidOpen(true);
                    _trashTimer += Time.deltaTime;
                    if (_trashTimer >= trashHoldTime) { DisposeIntoTrash(); return; }
                    return;
                }
                myTrashBin.SetLidOpen(false);
                _trashTimer = 0f;
            }

            if (_pepperoniCount >= REQUIRED_PEPPERONI)
                TryPickUpCompletedPizza();
            else
                TryPlacePepperoni();
        }
        else
        {
            if (_pepperoniCount >= REQUIRED_PEPPERONI)
                TryPickUpCompletedPizza();
            else
                TryGrabPepperoni();
        }
    }

    void TryPickUpCompletedPizza()
    {
        if (pizzaDough == null) return;
        if (XZ(transform.position, pizzaDough.position) > grabRadius) return;
        if (ovenStation != null && ovenStation.IsOccupied) return;
        if (_handState != null && !_handState.Occupy(this)) return;

        if (pickupRawAudioSource != null && pickupRawClip != null)
            pickupRawAudioSource.PlayOneShot(pickupRawClip);

        SetState(State.CarryingRawToOven);
        SA(tickIndicator, false);
        SetHand(true);
    }


    void UpdateCarryingRawToOven()
    {
        if (pizzaDough != null)
            pizzaDough.position = transform.position + pizzaCarryOffset;

        if (ovenStation == null) return;

        if (!ovenStation.IsOccupied &&
            XZ(transform.position, ovenStation.transform.position) <= ovenRadius)
        {
            InsertPizzaIntoOven();
        }
    }

    void InsertPizzaIntoOven()
    {
        if (ovenStation == null) return;

        RecycleAndRespawnDough();

        SA(blockedIndicator, false);
        SA(tickIndicator, false);
        SA(counterText, false);

        ovenStation.ReceivePizza();

        SetHand(false);
        if (_handState != null) _handState.Release(this);
        SetState(State.PlacingToppings);

        Debug.Log("[PepperoniGrabber] Pizza sent to the oven. Player is free to make a new one.");
    }


    void CheckOvenPickup()
    {
        if (ovenStation == null) return;
        if (!ovenStation.IsCookDone) return;
        if (ovenStation.CookedPizzaInWorld == null) return;
        if (XZ(transform.position, ovenStation.CookedPizzaInWorld.transform.position) > grabRadius) return;
        if (_handState != null && !_handState.Occupy(this)) return;

        GameObject cookedObj = ovenStation.CookedPizzaInWorld;
        bool picked = ovenStation.TryPickupCooked();
        if (!picked) return;

        _carriedCookedPizza = cookedObj;
        if (_carriedCookedPizza != null)
            _carriedCookedPizza.SetActive(true);

        if (pickupCookedAudioSource != null && pickupCookedClip != null)
            pickupCookedAudioSource.PlayOneShot(pickupCookedClip);

        SetHand(true);
        SetState(State.CarryingCookedToPlate);

        Debug.Log("[PepperoniGrabber] Cooked pizza picked up. Carrying to the plate");
    }


    void CheckEnemyOvenPickup()
    {
        if (enemyOvenStation == null) return;
        if (!enemyOvenStation.IsCookDone) return;
        if (enemyOvenStation.CookedPizzaInWorld == null) return;
        if (XZ(transform.position, enemyOvenStation.CookedPizzaInWorld.transform.position) > stealRadius) return;
        if (_handState != null && !_handState.Occupy(this)) return;

        GameObject stolenPizza = enemyOvenStation.CookedPizzaInWorld;
        bool picked = enemyOvenStation.TryPickupCooked();
        if (!picked) return;

        _carriedStolenPizza = stolenPizza;
        if (_carriedStolenPizza != null)
            _carriedStolenPizza.SetActive(true);

        _trashTimer = 0f;
        SetHand(true);
        SetState(State.StealingEnemyPizza);

        Debug.Log("[PepperoniGrabber] Enemy pizza stolen. Carrying it to the trash bin.");
    }


    void UpdateStealingEnemyPizza()
    {
        if (_carriedStolenPizza != null)
            _carriedStolenPizza.transform.position = transform.position + pizzaCarryOffset;

        if (myTrashBin == null) return;

        if (XZ(transform.position, myTrashBin.transform.position) <= trashRadius)
        {
            myTrashBin.SetLidOpen(true);
            _trashTimer += Time.deltaTime;
            if (_trashTimer >= trashHoldTime)
                DisposeEnemyPizzaIntoTrash();
        }
        else
        {
            myTrashBin.SetLidOpen(false);
            _trashTimer = 0f;
        }
    }

    void DisposeEnemyPizzaIntoTrash()
    {
        if (myTrashBin != null) myTrashBin.SetLidOpen(false);
        _trashTimer = 0f;

        if (_carriedStolenPizza != null) { Destroy(_carriedStolenPizza); _carriedStolenPizza = null; }

        SetHand(false);
        if (_handState != null) _handState.Release(this);
        SetState(State.PlacingToppings);

        Debug.Log("[PepperoniGrabber] Enemy pizza thrown into the trash.");
    }


    void UpdateCarryingCookedToPlate()
    {
        if (_carriedCookedPizza != null)
            _carriedCookedPizza.transform.position = transform.position + pizzaCarryOffset;

        if (plateTransform != null &&
            XZ(transform.position, plateTransform.position) <= plateRadius)
        {
            DeliverPizzaToPlate();
        }
    }

    void DeliverPizzaToPlate()
    {
        if (_carriedCookedPizza != null)
        {
            if (plateTransform != null)
            {
                _carriedCookedPizza.transform.position = plateTransform.position + new Vector3(0, 1.0f, 0);
                _carriedCookedPizza.transform.rotation = plateTransform.rotation;
                _carriedCookedPizza.transform.SetParent(plateTransform);
            }
            else
            {
                Destroy(_carriedCookedPizza);
            }
            _carriedCookedPizza = null;
        }

        SetHand(false);
        if (_handState != null) _handState.Release(this);

        _pizzasCompleted++;
        UpdatePizzaCounterUI();

        if (GameManager.Instance != null) GameManager.Instance.AddScore(playerNumber, 1);

        if (plateDeliveryAudioSource != null && plateDeliveryClip != null)
            plateDeliveryAudioSource.PlayOneShot(plateDeliveryClip);

        SetState(State.PlacingToppings);

        Debug.Log("[PepperoniGrabber] Pizza delivered to the plate! Total: " + _pizzasCompleted);
    }


    void TryGrabPepperoni()
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Topping"))
        {
            if (!obj.name.ToLower().Contains("pepperoni")) continue;
            if (!obj.activeInHierarchy) continue;
            if (obj.GetComponent<PepperoniBeingHeld>() != null) continue;
            if (XZ(transform.position, obj.transform.position) > grabRadius) continue;
            if (_handState != null && !_handState.Occupy(this)) return;

            _heldPepperoni = obj;
            obj.AddComponent<PepperoniBeingHeld>();
            _trashTimer = 0f;

            var lt = obj.GetComponent<IngredientLifetime>();
            if (lt != null) Destroy(lt);

            if (grabPepperoniAudioSource != null && grabPepperoniClip != null)
                grabPepperoniAudioSource.PlayOneShot(grabPepperoniClip);

            SetHand(true);
            return;
        }
    }

    void TryPlacePepperoni()
    {
        if (_heldPepperoni == null) { SA(blockedIndicator, false); return; }
        if (pizzaDough == null || XZ(transform.position, pizzaDough.position) > placeRadius)
        { SA(blockedIndicator, false); return; }

        bool salsaDone = mySalsaStation == null || mySalsaStation.IsSalsaDone;
        bool cheeseDone = myCheeseSpreader == null || myCheeseSpreader.IsSpreadDone;

        SA(blockedIndicator, !salsaDone || !cheeseDone);

        if (salsaDone && cheeseDone) PlacePepperoni();
    }

    void PlacePepperoni()
    {
        if (_heldPepperoni == null) return;

        Vector2 slot = pepperoniSlots[_pepperoniCount];
        Vector3 doughScale = pizzaDough.lossyScale;

        _heldPepperoni.transform.SetParent(pizzaDough, false);
        _heldPepperoni.transform.localPosition = new Vector3(
            slot.x / doughScale.x,
            pepperoniYOffset / doughScale.y,
            slot.y / doughScale.z);
        _heldPepperoni.transform.localScale = Vector3.one * pepperoniScale;

        var m = _heldPepperoni.GetComponent<PepperoniBeingHeld>(); if (m != null) Destroy(m);
        var lt = _heldPepperoni.GetComponent<IngredientLifetime>(); if (lt != null) Destroy(lt);

        _heldPepperoni.tag = "PlacedTopping";
        _heldPepperoni = null;
        _trashTimer = 0f;
        _pepperoniCount++;

        if (pepperoniPlaceAudioSource != null)
        {
            if (pepperoniPlaceClip != null) pepperoniPlaceAudioSource.clip = pepperoniPlaceClip;
            pepperoniPlaceAudioSource.Play();
        }

        if (myTrashBin != null) myTrashBin.SetLidOpen(false);

        SetHand(false);
        if (_handState != null) _handState.Release(this);

        UpdateCounter();
    }

    void DisposeIntoTrash()
    {
        if (myTrashBin != null) myTrashBin.SetLidOpen(false);
        _trashTimer = 0f;

        if (_heldPepperoni != null)
        {
            var m = _heldPepperoni.GetComponent<PepperoniBeingHeld>(); if (m != null) Destroy(m);
            Destroy(_heldPepperoni);
            _heldPepperoni = null;
        }

        SA(blockedIndicator, false);
        SetHand(false);
        if (_handState != null) _handState.Release(this);
    }

    void RecycleAndRespawnDough()
    {
        if (pizzaDough == null) return;

        for (int i = pizzaDough.childCount - 1; i >= 0; i--)
        {
            Transform c = pizzaDough.GetChild(i);
            if (c.CompareTag("PlacedTopping") || c.name.ToLower().Contains("pepperoni"))
                Destroy(c.gameObject);
        }

        Vector3 pos = doughSpawnPoint ? doughSpawnPoint.position : transform.position + Vector3.right * 2f;
        Quaternion rot = doughSpawnPoint ? doughSpawnPoint.rotation : Quaternion.identity;

        pizzaDough.position = pos;
        pizzaDough.rotation = rot;

        _pepperoniCount = 0;
        SA(counterText, false);
        SA(tickIndicator, false);
        SA(blockedIndicator, false);

        if (mySalsaStation != null) mySalsaStation.ResetStation();
        if (myCheeseSpreader != null) myCheeseSpreader.ResetSpreader();
    }

    void UpdateCounter()
    {
        if (_pepperoniCount >= REQUIRED_PEPPERONI)
        {
            SA(counterText, false);
            SA(tickIndicator, true);

            if (tickAudioSource != null)
            {
                if (tickSound != null) tickAudioSource.clip = tickSound;
                tickAudioSource.Play();
            }
        }
        else
        {
            if (counterText != null)
            {
                counterText.gameObject.SetActive(_pepperoniCount > 0);
                counterText.text = _pepperoniCount + "/" + REQUIRED_PEPPERONI;
            }
        }
    }


    void SetState(State s) => _state = s;

    float XZ(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x, dz = a.z - b.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    void SetHand(bool closed)
    {
        if (maOberta != null) maOberta.SetActive(!closed);
        if (maTancada != null) maTancada.SetActive(closed);
    }

    void SA(GameObject go, bool v) { if (go != null) go.SetActive(v); }
    void SA(TextMeshProUGUI t, bool v) { if (t != null) t.gameObject.SetActive(v); }

    void UpdatePizzaCounterUI()
    {
        if (pizzaCounterText3D != null)
        {
            pizzaCounterText3D.gameObject.SetActive(_pizzasCompleted > 0);
            if (_pizzasCompleted > 0)
                pizzaCounterText3D.text = _pizzasCompleted.ToString();
        }
    }
}

public class PepperoniBeingHeld : MonoBehaviour { }