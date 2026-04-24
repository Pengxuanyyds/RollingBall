using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class BallVisualRoll : MonoBehaviour
{
    public float visualRadius = 0.5f;
    [Range(0.1f, 1f)] public float visualRollSpeed = 0.7f;

    private Vector3 lastPosition;
    private Transform visualTarget;
    private MeshRenderer rootRenderer;

    private void Awake()
    {
        rootRenderer = GetComponent<MeshRenderer>();
        EnsureVisualTarget();
    }

    private void Start()
    {
        lastPosition = transform.position;
    }

    private void LateUpdate()
    {
        if (visualTarget == null) return;

        Vector3 displacement = transform.position - lastPosition;
        Vector3 planarDisplacement = Vector3.ProjectOnPlane(displacement, Vector3.up);

        if (planarDisplacement.sqrMagnitude > 0.000001f)
        {
            float scaledRadius = Mathf.Max(visualRadius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z), 0.001f);
            Vector3 eulerStep = new Vector3(
                planarDisplacement.z / scaledRadius * Mathf.Rad2Deg,
                0f,
                -planarDisplacement.x / scaledRadius * Mathf.Rad2Deg
            ) * visualRollSpeed;
            visualTarget.Rotate(eulerStep, Space.Self);
        }

        lastPosition = transform.position;
    }

    private void EnsureVisualTarget()
    {
        Transform existing = transform.Find("BallVisual");
        if (existing != null)
        {
            visualTarget = existing;
            if (rootRenderer != null) rootRenderer.enabled = false;
            return;
        }

        MeshFilter sourceFilter = GetComponent<MeshFilter>();
        MeshRenderer sourceRenderer = rootRenderer;
        if (sourceFilter == null || sourceRenderer == null) return;

        GameObject visualObject = new GameObject("BallVisual");
        visualObject.transform.SetParent(transform, false);
        visualObject.transform.localPosition = Vector3.zero;
        visualObject.transform.localRotation = Quaternion.identity;
        visualObject.transform.localScale = Vector3.one;

        MeshFilter visualFilter = visualObject.AddComponent<MeshFilter>();
        visualFilter.sharedMesh = sourceFilter.sharedMesh;

        MeshRenderer visualRenderer = visualObject.AddComponent<MeshRenderer>();
        visualRenderer.sharedMaterials = sourceRenderer.sharedMaterials;
        visualRenderer.shadowCastingMode = sourceRenderer.shadowCastingMode;
        visualRenderer.receiveShadows = sourceRenderer.receiveShadows;
        visualRenderer.lightProbeUsage = sourceRenderer.lightProbeUsage;
        visualRenderer.reflectionProbeUsage = sourceRenderer.reflectionProbeUsage;

        sourceRenderer.enabled = false;
        visualTarget = visualObject.transform;
    }
}
