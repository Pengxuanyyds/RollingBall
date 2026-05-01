using System.Collections.Generic;
using UnityEngine;

public class SideScenery : MonoBehaviour
{
    [Header("Scan Visual")]
    public GameObject[] visualPrefabs;
    public string resourcesFolder = "Objects";
    public string[] resourceVisualNames = { "reading51", "Trophy" };
    public Vector3 maxVisualSize = new Vector3(4.2f, 6.2f, 4.2f);
    public float minLargestVisualSize = 2.6f;
    public bool randomizeYaw = true;

    private GameObject visualInstance;

    private void Awake()
    {
        EnsureVisualReady();
    }

    private void OnEnable()
    {
        EnsureVisualReady();
        ScanVisualUtility.SetVisible(transform, visualInstance, true);
        ScanVisualUtility.SetOnlyMainColliderEnabled(transform, null, false);
    }

    private void EnsureVisualReady()
    {
        RemoveDisallowedVisual();
        if (visualInstance != null) return;

        GameObject visualPrefab = PickVisualPrefab();
        visualInstance = ScanVisualUtility.InstantiateChild(visualPrefab, transform, "side scenery");
        if (visualInstance == null) return;

        if (randomizeYaw)
        {
            visualInstance.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        }

        ScanVisualUtility.SetRootRendererEnabled(transform, false);
        ScanVisualUtility.FitToBounds(transform, visualInstance, maxVisualSize, minLargestVisualSize);
        ScanVisualUtility.SetOnlyMainColliderEnabled(transform, null, false);
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
}
