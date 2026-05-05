using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class WeightedIngredient
{
    public GameObject prefab;
    [Tooltip("Probabilitat relativa. Pepperoni = 3, Cheese = 2")]
    public int weight = 1;
}

public class GlobalIngredientSpawner : MonoBehaviour
{
    [Header("Ingredients (amb pesos)")]
    public WeightedIngredient[] ingredients;

    [Header("Spawn Zones")]
    public BoxCollider[] spawnZones;

    [Header("Settings")]
    public int targetCount = 8;
    public float ingredientLifetime = 10f;

    [Header("Overlap Prevention")]
    public float minSpawnRadius = 1.2f;
    public int maxPlacementAttempts = 50;

    [Header("Spawn Animation")]
    public float spawnAnimDuration = 0.35f;
    public float spawnDelay = 0.3f;

    private List<GameObject> activeIngredients = new List<GameObject>();

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            // Netegem ingredients destruïts
            activeIngredients.RemoveAll(obj => obj == null);

            int needed = targetCount - activeIngredients.Count;
            for (int i = 0; i < needed; i++)
            {
                SpawnIngredient();
                yield return new WaitForSeconds(spawnDelay);
            }

            yield return new WaitForSeconds(2f);
        }
    }

    void SpawnIngredient()
    {
        if (spawnZones == null || spawnZones.Length == 0) return;
        if (ingredients == null || ingredients.Length == 0) return;

        BoxCollider zone = spawnZones[Random.Range(0, spawnZones.Length)];
        Vector3 spawnPos = Vector3.zero;
        bool positionFound = false;

        for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
        {
            Vector3 candidate = new Vector3(
                Random.Range(zone.bounds.min.x, zone.bounds.max.x),
                zone.transform.position.y,
                Random.Range(zone.bounds.min.z, zone.bounds.max.z)
            );

            if (!IsOccupied(candidate))
            {
                spawnPos = candidate;
                positionFound = true;
                break;
            }
        }

        if (!positionFound) return;

        GameObject prefab = PickWeightedRandom();
        Quaternion randomRot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        GameObject obj = Instantiate(prefab, spawnPos, randomRot);

        IngredientLifetime lt = obj.AddComponent<IngredientLifetime>();
        lt.lifetime = ingredientLifetime;

        activeIngredients.Add(obj);

        StartCoroutine(SpawnAnimation(obj, spawnAnimDuration));
    }

    bool IsOccupied(Vector3 candidate)
    {
        foreach (GameObject ing in activeIngredients)
        {
            if (ing == null) continue;
            if (Vector3.Distance(candidate, ing.transform.position) < minSpawnRadius)
                return true;
        }
        return false;
    }

    GameObject PickWeightedRandom()
    {
        int totalWeight = 0;
        foreach (var wi in ingredients)
            totalWeight += Mathf.Max(1, wi.weight);

        int roll = Random.Range(0, totalWeight);
        int cumulative = 0;
        foreach (var wi in ingredients)
        {
            cumulative += Mathf.Max(1, wi.weight);
            if (roll < cumulative)
                return wi.prefab;
        }
        return ingredients[0].prefab;
    }

    IEnumerator SpawnAnimation(GameObject obj, float duration)
    {
        if (obj == null) yield break;

        Vector3 originalScale = obj.transform.localScale;
        obj.transform.localScale = Vector3.zero;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (obj == null) yield break;
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            obj.transform.localScale = originalScale * EaseOutBack(t);
            yield return null;
        }

        if (obj != null)
            obj.transform.localScale = originalScale;
    }

    float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}