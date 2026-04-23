using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Barrier : MonoBehaviour
{
    public GameObject destroyEffectPrefab;

    [Header("Scan Visual")]
    public GameObject[] visualPrefabs;
    public string resourcesFolder = "Objects";
    public Vector3 maxVisualSize = new Vector3(1.3f, 2.2f, 1.6f);
    public float minLargestVisualSize = 0.9f;

    public delegate void BarrierHitAction();
    public static event BarrierHitAction OnAnyBarrierHit;

    private bool hasCollided = false;
    private BoxCollider hitCollider;
    private GameObject visualInstance;
    private Renderer[] renderers;
    private Collider[] colliders;
    private static GameObject[] cachedResourceVisuals;

    private void Awake()
    {
        transform.localScale = Vector3.one;
        hitCollider = GetComponent<BoxCollider>();
        hitCollider.isTrigger = true;
        EnsureVisualReady();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasCollided) return;
        if (!other.CompareTag("Player") && other.name != "Player") return;

        hasCollided = true;
        OnAnyBarrierHit?.Invoke();

        if (destroyEffectPrefab != null)
        {
            Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
        }

        SetRenderersEnabled(false);
        SetCollidersEnabled(false);
    }

    private void OnEnable()
    {
        hasCollided = false;
        EnsureVisualReady();
        SetRenderersEnabled(true);
        SetCollidersEnabled(true);
    }

    private void EnsureVisualReady()
    {
        if (hitCollider == null)
        {
            hitCollider = GetComponent<BoxCollider>();
            hitCollider.isTrigger = true;
        }

        if (visualInstance != null)
        {
            RefreshCachedComponents();
            return;
        }

        GameObject visualPrefab = PickVisualPrefab();
        if (visualPrefab != null)
        {
            MeshRenderer originalRenderer = GetComponent<MeshRenderer>();
            MeshFilter originalFilter = GetComponent<MeshFilter>();
            if (originalRenderer != null) originalRenderer.enabled = false;
            if (originalFilter != null) originalFilter.mesh = null;

            visualInstance = Instantiate(visualPrefab, transform);
            visualInstance.name = visualPrefab.name + "_Visual";
            visualInstance.transform.localPosition = Vector3.zero;
            visualInstance.transform.localRotation = Quaternion.identity;
            visualInstance.transform.localScale = Vector3.one;

            FitVisualToTrack();
        }
        else
        {
            FitExistingPrimitiveCollider();
        }

        RefreshCachedComponents();
    }

    private GameObject PickVisualPrefab()
    {
        if (visualPrefabs != null && visualPrefabs.Length > 0)
        {
            return visualPrefabs[Random.Range(0, visualPrefabs.Length)];
        }

        if (cachedResourceVisuals == null)
        {
            cachedResourceVisuals = Resources.LoadAll<GameObject>(resourcesFolder);
        }

        if (cachedResourceVisuals != null && cachedResourceVisuals.Length > 0)
        {
            return cachedResourceVisuals[Random.Range(0, cachedResourceVisuals.Length)];
        }

        return null;
    }

    private void FitVisualToTrack()
    {
        Bounds bounds = CalculateLocalRendererBounds();
        if (bounds.size == Vector3.zero) return;

        Vector3 size = bounds.size;
        float fitScale = Mathf.Min(
            maxVisualSize.x / Mathf.Max(size.x, 0.001f),
            maxVisualSize.y / Mathf.Max(size.y, 0.001f),
            maxVisualSize.z / Mathf.Max(size.z, 0.001f)
        );

        fitScale = Mathf.Min(fitScale, 1f);
        Vector3 fittedSize = size * fitScale;
        float largest = Mathf.Max(fittedSize.x, fittedSize.y, fittedSize.z);

        if (largest < minLargestVisualSize)
        {
            float readableScale = minLargestVisualSize / Mathf.Max(largest, 0.001f);
            fitScale *= readableScale;
        }

        visualInstance.transform.localScale *= fitScale;

        bounds = CalculateLocalRendererBounds();
        visualInstance.transform.localPosition += new Vector3(-bounds.center.x, -bounds.min.y, -bounds.center.z);

        bounds = CalculateLocalRendererBounds();
        hitCollider.center = bounds.center;
        hitCollider.size = new Vector3(
            Mathf.Max(bounds.size.x, 0.65f),
            Mathf.Max(bounds.size.y, 0.65f),
            Mathf.Max(bounds.size.z, 0.65f)
        );
    }

    private void FitExistingPrimitiveCollider()
    {
        hitCollider.center = new Vector3(0f, 0.5f, 0f);
        hitCollider.size = new Vector3(1.2f, 1f, 1.4f);
    }

    private Bounds CalculateLocalRendererBounds()
    {
        Renderer[] childRenderers = GetComponentsInChildren<Renderer>(true);
        bool hasBounds = false;
        Bounds localBounds = new Bounds(Vector3.zero, Vector3.zero);

        foreach (Renderer childRenderer in childRenderers)
        {
            if (childRenderer.transform == transform) continue;

            Bounds worldBounds = childRenderer.bounds;
            Vector3 min = worldBounds.min;
            Vector3 max = worldBounds.max;
            Vector3[] corners =
            {
                new Vector3(min.x, min.y, min.z),
                new Vector3(min.x, min.y, max.z),
                new Vector3(min.x, max.y, min.z),
                new Vector3(min.x, max.y, max.z),
                new Vector3(max.x, min.y, min.z),
                new Vector3(max.x, min.y, max.z),
                new Vector3(max.x, max.y, min.z),
                new Vector3(max.x, max.y, max.z)
            };

            foreach (Vector3 corner in corners)
            {
                Vector3 localCorner = transform.InverseTransformPoint(corner);
                if (!hasBounds)
                {
                    localBounds = new Bounds(localCorner, Vector3.zero);
                    hasBounds = true;
                }
                else
                {
                    localBounds.Encapsulate(localCorner);
                }
            }
        }

        return hasBounds ? localBounds : new Bounds(Vector3.zero, Vector3.zero);
    }

    private void RefreshCachedComponents()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        colliders = GetComponentsInChildren<Collider>(true);
    }

    private void SetRenderersEnabled(bool enabled)
    {
        if (renderers == null) RefreshCachedComponents();
        foreach (Renderer childRenderer in renderers)
        {
            if (visualInstance != null && childRenderer.transform == transform) continue;
            childRenderer.enabled = enabled;
        }
    }

    private void SetCollidersEnabled(bool enabled)
    {
        if (colliders == null) RefreshCachedComponents();
        foreach (Collider childCollider in colliders)
        {
            childCollider.enabled = childCollider == hitCollider ? enabled : false;
        }
    }
}
