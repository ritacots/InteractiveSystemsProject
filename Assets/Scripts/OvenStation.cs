using UnityEngine;
using System.Collections;


public class OvenStation : MonoBehaviour
{

    [Header("Interaction")]
    [Tooltip("XZ radius within which the player can interact with the oven.")]
    public float interactRadius = 2.5f;


    [Header("Cooking")]
    public float cookTime = 10f;


    [Header("Output")]
    [Tooltip("Point where the cooked pizza will appear. If null, the oven position is used.")]
    public Transform outputPoint;

    [Tooltip("Raw pizza prefab (dough + sauce + cheese + 4 pepperonis). " +
             "Instantiated when the player places the pizza and visible during cooking.")]
    public GameObject rawPizzaInOvenPrefab;

    [Tooltip("Cooked pizza prefab. Instantiated when the 10 s cooking time ends " +
             "and is the object the player must collect during the 6 s countdown.")]
    public GameObject cookedPizzaPrefab;

    [Header("Pickup Countdown Sprites")]
    [Tooltip("SpriteRenderer where the pickup countdown sprites will be displayed.")]
    public SpriteRenderer countdownSpriteRenderer;

    [Tooltip("Countdown sprites. Index 0 = 6s remaining, index 1 = 5s … index 5 = 1s. " +
             "You need exactly 6 sprites (or as many as pickupTime whole seconds).")]
    public Sprite[] countdownSprites;

    public float pickupTime = 6f;


    [Header("Visual Indicators (optional)")]
    public GameObject cookingIndicator;
    public GameObject doneIndicator;


    [Header("Fire Mechanic")]
    [Tooltip("Particle effect (or GameObject) representing the oven flames. " +
             "Activated when the fire starts. Disable it by default in the Inspector.")]
    public GameObject fireEffect;

    [Tooltip("Burned pizza prefab that appears once the fire is extinguished. " +
             "The player must take it to the trash bin.")]
    public GameObject burnedPizzaPrefab;

    [Tooltip("XZ radius from which a player with a fire extinguisher can put out the fire.")]
    public float extinguishRadius = 2.5f;


    [Header("Audio")]
    [Tooltip("Looping fire sound. Starts when the oven catches fire and stops when extinguished.")]
    public AudioSource fireAudioSource;
    public AudioClip fireClip;

    [Tooltip("One-shot sound when the pizza is ready for pickup.")]
    public AudioSource doneAudioSource;
    public AudioClip doneClip;

    [Tooltip("Tick-tock sound played every second during the pickup countdown.")]
    public AudioSource tickTackAudioSource;
    public AudioClip tickTackClip;

    public bool IsOccupied => _state != OvenState.Idle;
    public bool IsCookDone => _state == OvenState.WaitingPickup;
    public bool IsCooking => _state == OvenState.Cooking;
    public bool IsOnFire => _state == OvenState.OnFire;

    public GameObject CookedPizzaInWorld => _cookedPizzaInWorld;
    public GameObject BurnedPizzaInWorld => _burnedPizzaInWorld;

    public System.Action OnPizzaPickedUp;
    public System.Action OnOvenFire;
    public System.Action OnFireExtinguished;

    enum OvenState { Idle, Cooking, WaitingPickup, OnFire }
    OvenState _state = OvenState.Idle;

    float _cookTimer = 0f;
    float _pickupTimer = 0f;
    GameObject _rawVisualInstance = null;
    GameObject _cookedPizzaInWorld = null;
    GameObject _burnedPizzaInWorld = null;
    Coroutine _countdownCoroutine = null;



    void Start()
    {
        SetIndicator(cookingIndicator, false);
        SetIndicator(doneIndicator, false);
        SetIndicator(fireEffect, false);
        if (countdownSpriteRenderer != null)
            countdownSpriteRenderer.enabled = false;
    }

    void Update()
    {
        switch (_state)
        {
            case OvenState.Cooking: UpdateCooking(); break;
            case OvenState.WaitingPickup: UpdateWaitingPickup(); break;
        }
    }



    public void ReceivePizza()
    {
        if (_state != OvenState.Idle) return;

        Vector3 pos = outputPoint != null ? outputPoint.position : transform.position;
        Quaternion rot = outputPoint != null ? outputPoint.rotation : Quaternion.identity;

        if (rawPizzaInOvenPrefab != null)
        {
            _rawVisualInstance = Instantiate(rawPizzaInOvenPrefab, pos, rot);
            _rawVisualInstance.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[OvenStation] rawPizzaInOvenPrefab not assigned!");
        }

        _cookTimer = 0f;
        _state = OvenState.Cooking;

        SetIndicator(cookingIndicator, true);
        SetIndicator(doneIndicator, false);

        Debug.Log("[OvenStation] Pizza received. Cooking for " + cookTime + " s.");
    }

    public bool TryPickupCooked()
    {
        if (_state != OvenState.WaitingPickup) return false;
        if (_cookedPizzaInWorld == null) return false;

        CollectCooked();
        return true;
    }

    public bool TryExtinguish()
    {
        if (_state != OvenState.OnFire) return false;

        ExtinguishFire();
        return true;
    }

    public GameObject TryPickupBurned()
    {
        if (_burnedPizzaInWorld == null) return null;

        GameObject burned = _burnedPizzaInWorld;
        _burnedPizzaInWorld = null;
        return burned;
    }

    public void ResetOven()
    {
        StopCountdown();

        if (_rawVisualInstance != null) { Destroy(_rawVisualInstance); _rawVisualInstance = null; }
        if (_cookedPizzaInWorld != null) { Destroy(_cookedPizzaInWorld); _cookedPizzaInWorld = null; }
        if (_burnedPizzaInWorld != null) { Destroy(_burnedPizzaInWorld); _burnedPizzaInWorld = null; }

        _cookTimer = 0f;
        _pickupTimer = 0f;
        _state = OvenState.Idle;

        SetIndicator(cookingIndicator, false);
        SetIndicator(doneIndicator, false);
        SetIndicator(fireEffect, false);
    }



    void UpdateCooking()
    {
        _cookTimer += Time.deltaTime;
        if (_cookTimer >= cookTime)
            FinishCooking();
    }

    void FinishCooking()
    {
        if (_rawVisualInstance != null)
        {
            Destroy(_rawVisualInstance);
            _rawVisualInstance = null;
        }

        Vector3 pos = outputPoint != null ? outputPoint.position : transform.position;
        Quaternion rot = outputPoint != null ? outputPoint.rotation : Quaternion.identity;

        if (cookedPizzaPrefab != null)
        {
            _cookedPizzaInWorld = Instantiate(cookedPizzaPrefab, pos, rot);
            _cookedPizzaInWorld.SetActive(true);
        }
        else
        {
            _cookedPizzaInWorld = new GameObject("CookedPizzaMarker");
            _cookedPizzaInWorld.transform.position = pos;
            Debug.LogWarning("[OvenStation] cookedPizzaPrefab not assigned!");
        }

        _pickupTimer = 0f;
        _state = OvenState.WaitingPickup;

        SetIndicator(cookingIndicator, false);
        SetIndicator(doneIndicator, true);

        if (doneAudioSource != null && doneClip != null)
            doneAudioSource.PlayOneShot(doneClip);

        _countdownCoroutine = StartCoroutine(PickupCountdownCoroutine());

        Debug.Log("[OvenStation] Cooking finished! The player has " + pickupTime + " s to pick it up.");
    }

    void UpdateWaitingPickup()
    {
        _pickupTimer += Time.deltaTime;
        if (_pickupTimer >= pickupTime)
            StartFire();
    }

    void CollectCooked()
    {
        StopCountdown();

        _cookedPizzaInWorld = null;

        SetIndicator(doneIndicator, false);
        _state = OvenState.Idle;

        OnPizzaPickedUp?.Invoke();
        Debug.Log("[OvenStation] Pizza picked up by the player.");
    }

    void StartFire()
    {
        StopCountdown();

        if (_cookedPizzaInWorld != null) { Destroy(_cookedPizzaInWorld); _cookedPizzaInWorld = null; }

        SetIndicator(doneIndicator, false);
        SetIndicator(cookingIndicator, false);
        SetIndicator(fireEffect, true);

        if (fireAudioSource != null && fireClip != null)
        {
            fireAudioSource.clip = fireClip;
            fireAudioSource.loop = true;
            fireAudioSource.Stop();
            fireAudioSource.Play();
        }

        _state = OvenState.OnFire;

        OnOvenFire?.Invoke();
        Debug.Log("[OvenStation] OVEN ON FIRE! A fire extinguisher is required.");
    }

    void ExtinguishFire()
    {
        SetIndicator(fireEffect, false);

        if (fireAudioSource != null && fireAudioSource.isPlaying)
            fireAudioSource.Stop();

        Vector3 pos = outputPoint != null ? outputPoint.position : transform.position;
        Quaternion rot = outputPoint != null ? outputPoint.rotation : Quaternion.identity;

        if (burnedPizzaPrefab != null)
        {
            _burnedPizzaInWorld = Instantiate(burnedPizzaPrefab, pos, rot);
            _burnedPizzaInWorld.SetActive(true);
        }
        else
        {
            _burnedPizzaInWorld = new GameObject("BurnedPizzaMarker");
            _burnedPizzaInWorld.transform.position = pos;
            Debug.LogWarning("[OvenStation] burnedPizzaPrefab not assigned!");
        }

        _state = OvenState.Idle;

        OnFireExtinguished?.Invoke();
        Debug.Log("[OvenStation] Fire extinguished. Burned pizza ready to be thrown away.");
    }

    //countdown

    IEnumerator PickupCountdownCoroutine()
    {
        if (countdownSpriteRenderer != null)
            countdownSpriteRenderer.enabled = true;

        // Start the looping clip at the beginning of the countdown
        if (tickTackAudioSource != null && tickTackClip != null)
        {
            tickTackAudioSource.clip = tickTackClip;
            tickTackAudioSource.loop = true;
            tickTackAudioSource.Play();
        }

        int totalSeconds = Mathf.CeilToInt(pickupTime); // 6

        for (int secondsLeft = totalSeconds; secondsLeft >= 1; secondsLeft--)
        {

            if (countdownSpriteRenderer != null && countdownSprites != null)
            {
                int idx = totalSeconds - secondsLeft;
                if (idx < countdownSprites.Length && countdownSprites[idx] != null)
                    countdownSpriteRenderer.sprite = countdownSprites[idx];
            }

            yield return new WaitForSeconds(1f);

            if (_state != OvenState.WaitingPickup)
            {
                StopTickTack();
                HideCountdownSprite();
                yield break;
            }
        }

        StopTickTack();
        HideCountdownSprite();
        _countdownCoroutine = null;
    }

    void StopCountdown()
    {
        if (_countdownCoroutine != null)
        {
            StopCoroutine(_countdownCoroutine);
            _countdownCoroutine = null;
        }
        StopTickTack();
        HideCountdownSprite();
    }

    void StopTickTack()
    {
        if (tickTackAudioSource != null && tickTackAudioSource.isPlaying)
            tickTackAudioSource.Stop();
    }

    void HideCountdownSprite()
    {
        if (countdownSpriteRenderer != null)
            countdownSpriteRenderer.enabled = false;
    }



    static void SetIndicator(GameObject go, bool active)
    {
        if (go != null) go.SetActive(active);
    }
}