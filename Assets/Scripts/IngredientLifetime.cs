using UnityEngine;
using System.Collections;

public class IngredientLifetime : MonoBehaviour
{
    public float lifetime = 20f;

    [Tooltip("Duración del shrink de salida en segundos")]
    public float despawnAnimDuration = 0.3f;

    void Start()
    {
        StartCoroutine(LifetimeRoutine());
    }

    IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(lifetime - despawnAnimDuration);

        while (GetComponent<CheeseBeingHeld>() != null)
            yield return null;

        Vector3 originalScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < despawnAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / despawnAnimDuration);
            float scale = EaseInBack(1f - t);
            transform.localScale = originalScale * scale;
            yield return null;
        }

        Destroy(gameObject);
    }

    float EaseInBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return c3 * t * t * t - c1 * t * t;
    }
}