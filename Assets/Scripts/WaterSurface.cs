using UnityEngine;

public class WaterSurface : MonoBehaviour
{
    [Header("Ripple Settings")]
    public float rippleSpeed = 2f;
    public float rippleScale = 5f;
    public float rippleStrength = 0.3f;

    [Header("Scroll Settings")]
    public float scrollSpeedX = 0.3f;
    public float scrollSpeedY = 0.15f;

    [Header("Color Settings")]
    public Color shallowColor = new Color(0.2f, 0.7f, 0.95f, 0.85f);
    public Color deepColor = new Color(0.05f, 0.3f, 0.7f, 0.95f);

    private Material waterMaterial;
    private Mesh mesh;
    private Vector3[] baseVertices;
    private Vector3[] animatedVertices;

    void Start()
    {
        // Get mesh and store original vertices
        MeshFilter mf = GetComponent<MeshFilter>();
        mesh = mf.mesh;
        baseVertices = mesh.vertices;
        animatedVertices = new Vector3[baseVertices.Length];

        // Set up material
        waterMaterial = GetComponent<Renderer>().material;
        waterMaterial.color = shallowColor;
    }

    void Update()
    {
        AnimateVertices();
        AnimateColor();
    }

    void AnimateVertices()
    {
        for (int i = 0; i < baseVertices.Length; i++)
        {
            Vector3 v = baseVertices[i];

            // Layer multiple sine waves for a realistic ripple
            float wave1 = Mathf.Sin(
                (v.x * rippleScale) + (Time.time * rippleSpeed)
            ) * rippleStrength;

            float wave2 = Mathf.Sin(
                (v.z * rippleScale * 0.8f) + (Time.time * rippleSpeed * 1.2f)
            ) * rippleStrength * 0.7f;

            float wave3 = Mathf.Sin(
                (v.x * rippleScale * 0.5f + v.z * rippleScale * 0.5f)
                + (Time.time * rippleSpeed * 0.8f)
            ) * rippleStrength * 0.5f;

            animatedVertices[i] = new Vector3(v.x, v.y + wave1 + wave2 + wave3, v.z);
        }

        mesh.vertices = animatedVertices;
        mesh.RecalculateNormals();
    }

    void AnimateColor()
    {
        // Scroll UV offset to simulate water moving
        float offsetX = Time.time * scrollSpeedX;
        float offsetY = Time.time * scrollSpeedY;
        waterMaterial.mainTextureOffset = new Vector2(offsetX, offsetY);

        // Pulse between shallow and deep color
        float pulse = (Mathf.Sin(Time.time * 1.5f) + 1f) / 2f;
        waterMaterial.color = Color.Lerp(shallowColor, deepColor, pulse);
    }

    void OnDestroy()
    {
        if (mesh != null && baseVertices != null)
            mesh.vertices = baseVertices;
    }
}