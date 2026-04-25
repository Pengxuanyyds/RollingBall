using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class Bonus : MonoBehaviour
{
    [Header("Scan Visual")]
    public GameObject visualPrefab;
    public string visualResourcePath = "BonusObjects/999";
    public Vector3 maxVisualSize = new Vector3(0.9f, 1.4f, 0.9f);
    public float minLargestVisualSize = 0.6f;

    public delegate void BonusCollectedAction();
    public static event BonusCollectedAction OnBonusCollected;

    private bool hasCollected;
    private CapsuleCollider hitCollider;
    private GameObject visualInstance;

    private void Awake()
    {
        SetupCollider();
        EnsureVisualReady();
    }

    private void OnEnable()
    {
        hasCollected = false;
        EnsureVisualReady();
        ScanVisualUtility.SetVisible(transform, visualInstance, true);
        ScanVisualUtility.SetOnlyMainColliderEnabled(transform, hitCollider, true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasCollected || (!other.CompareTag("Player") && other.name != "Player")) return;

        hasCollected = true;
        OnBonusCollected?.Invoke();
        ScanVisualUtility.SetVisible(transform, visualInstance, false);
        ScanVisualUtility.SetOnlyMainColliderEnabled(transform, hitCollider, false);
    }

    private void EnsureVisualReady()
    {
        SetupCollider();
        if (visualInstance != null) return;

        GameObject visual = Resources.Load<GameObject>(visualResourcePath);
        if (visual == null) visual = visualPrefab;

        visualInstance = ScanVisualUtility.InstantiateChild(visual, transform, "bonus");
        if (visualInstance == null) return;

        ScanVisualUtility.SetRootRendererEnabled(transform, false);
        FitVisualCollider();
    }

    private void SetupCollider()
    {
        if (hitCollider == null)
        {
            hitCollider = GetComponent<CapsuleCollider>();
        }

        hitCollider.isTrigger = true;
    }

    private void FitVisualCollider()
    {
        Bounds bounds = ScanVisualUtility.FitToBounds(transform, visualInstance, maxVisualSize, minLargestVisualSize);
        if (bounds.size == Vector3.zero) return;

        hitCollider.center = bounds.center;
        hitCollider.radius = Mathf.Clamp(Mathf.Max(bounds.extents.x, bounds.extents.z) * 0.45f, 0.18f, 0.45f);
        hitCollider.height = Mathf.Max(bounds.size.y, hitCollider.radius * 2f);
    }

}
