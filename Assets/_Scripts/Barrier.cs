using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Barrier : MonoBehaviour
{
    public GameObject destroyEffectPrefab;

    [Header("Scan Visual")]
    public GameObject[] visualPrefabs;
    public string resourcesFolder = "Objects";
    public string[] resourceVisualNames = { "bin", "Caution", "chair", "obstacle", "wash" };
    public Vector3 maxVisualSize = new Vector3(1.3f, 2.2f, 1.6f);
    public float minLargestVisualSize = 0.9f;

    public delegate void BarrierHitAction();
    public static event BarrierHitAction OnAnyBarrierHit;

    private bool hasCollided;
    private BoxCollider hitCollider;
    private GameObject visualInstance;

    private void Awake()
    {
        transform.localScale = Vector3.one;
        SetupCollider();
        EnsureVisualReady();
    }

    private void OnEnable()
    {
        hasCollided = false;
        EnsureVisualReady();
        ScanVisualUtility.SetVisible(transform, visualInstance, true);
        ScanVisualUtility.SetOnlyMainColliderEnabled(transform, hitCollider, true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasCollided || (!other.CompareTag("Player") && other.name != "Player")) return;

        hasCollided = true;
        OnAnyBarrierHit?.Invoke();

        if (destroyEffectPrefab != null)
        {
            Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
        }

        ScanVisualUtility.SetVisible(transform, visualInstance, false);
        ScanVisualUtility.SetOnlyMainColliderEnabled(transform, hitCollider, false);
    }

    private void EnsureVisualReady()
    {
        SetupCollider();
        RemoveDisallowedVisual();
        if (visualInstance != null) return;

        GameObject visualPrefab = PickVisualPrefab();
        visualInstance = ScanVisualUtility.InstantiateChild(visualPrefab, transform, "barrier");
        if (visualInstance == null)
        {
            FitFallbackCollider();
            return;
        }

        ScanVisualUtility.SetRootRendererEnabled(transform, false);
        FitVisualCollider();
    }

    private GameObject PickVisualPrefab()
    {
        GameObject resourceVisual = ScanVisualUtility.PickNamedResource(resourcesFolder, resourceVisualNames);
        if (resourceVisual != null) return resourceVisual;

        List<GameObject> validPrefabs = new List<GameObject>();
        if (visualPrefabs != null)
        {
            foreach (GameObject visualPrefab in visualPrefabs)
            {
                if (ScanVisualUtility.NameIsAllowed(visualPrefab, resourceVisualNames))
                {
                    validPrefabs.Add(visualPrefab);
                }
            }
        }

        return validPrefabs.Count == 0 ? null : validPrefabs[Random.Range(0, validPrefabs.Count)];
    }

    private void RemoveDisallowedVisual()
    {
        if (visualInstance == null) return;
        if (ScanVisualUtility.NameIsAllowed(visualInstance, resourceVisualNames)) return;

        Destroy(visualInstance);
        visualInstance = null;
    }

    private void SetupCollider()
    {
        if (hitCollider == null)
        {
            hitCollider = GetComponent<BoxCollider>();
        }

        hitCollider.isTrigger = true;
    }

    private void FitVisualCollider()
    {
        Bounds bounds = ScanVisualUtility.FitToBounds(transform, visualInstance, maxVisualSize, minLargestVisualSize);
        if (bounds.size == Vector3.zero)
        {
            FitFallbackCollider();
            return;
        }

        hitCollider.center = bounds.center;
        hitCollider.size = new Vector3(
            Mathf.Max(bounds.size.x, 0.65f),
            Mathf.Max(bounds.size.y, 0.65f),
            Mathf.Max(bounds.size.z, 0.65f)
        );
    }

    private void FitFallbackCollider()
    {
        hitCollider.center = new Vector3(0f, 0.5f, 0f);
        hitCollider.size = new Vector3(1.2f, 1f, 1.4f);
    }

}
