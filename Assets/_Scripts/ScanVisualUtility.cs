using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public static class ScanVisualUtility
{
    public static GameObject PickNamedResource(string folder, string[] names)
    {
        List<GameObject> loaded = LoadNamedResources(folder, names);
        if (loaded.Count == 0) return null;
        return loaded[UnityEngine.Random.Range(0, loaded.Count)];
    }

    public static GameObject InstantiateChild(GameObject prefab, Transform parent, string context)
    {
        if (prefab == null) return null;

        try
        {
            Object created = Object.Instantiate((Object)prefab);
            GameObject instance = created as GameObject;
            if (instance == null)
            {
                Object.Destroy(created);
                return null;
            }

            instance.name = prefab.name + "_Visual";
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            return instance;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Failed to instantiate " + context + " visual '" + prefab.name + "': " + exception.Message);
            return null;
        }
    }

    public static Bounds FitToBounds(Transform root, GameObject visual, Vector3 maxSize, float minLargestSize)
    {
        Bounds bounds = CalculateChildRendererBounds(root);
        if (visual == null || bounds.size == Vector3.zero) return bounds;

        Vector3 size = bounds.size;
        float fitScale = Mathf.Min(
            maxSize.x / Mathf.Max(size.x, 0.001f),
            maxSize.y / Mathf.Max(size.y, 0.001f),
            maxSize.z / Mathf.Max(size.z, 0.001f)
        );

        fitScale = Mathf.Min(fitScale, 1f);
        float largestAfterFit = Mathf.Max(size.x * fitScale, size.y * fitScale, size.z * fitScale);
        if (largestAfterFit < minLargestSize)
        {
            fitScale *= minLargestSize / Mathf.Max(largestAfterFit, 0.001f);
        }

        visual.transform.localScale *= fitScale;

        bounds = CalculateChildRendererBounds(root);
        visual.transform.localPosition += new Vector3(-bounds.center.x, -bounds.min.y, -bounds.center.z);
        return CalculateChildRendererBounds(root);
    }

    public static Bounds CalculateChildRendererBounds(Transform root)
    {
        Renderer[] childRenderers = root.GetComponentsInChildren<Renderer>(true);
        bool hasBounds = false;
        Bounds localBounds = new Bounds(Vector3.zero, Vector3.zero);

        foreach (Renderer childRenderer in childRenderers)
        {
            if (childRenderer.transform == root) continue;

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
                Vector3 localCorner = root.InverseTransformPoint(corner);
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

    public static void SetChildRenderersEnabled(Transform root, bool enabled)
    {
        foreach (Renderer childRenderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (childRenderer.transform != root)
            {
                childRenderer.enabled = enabled;
            }
        }
    }

    public static void SetRootRendererEnabled(Transform root, bool enabled)
    {
        Renderer rootRenderer = root.GetComponent<Renderer>();
        if (rootRenderer != null)
        {
            rootRenderer.enabled = enabled;
        }
    }

    public static void SetVisible(Transform root, GameObject visual, bool enabled)
    {
        if (visual != null)
        {
            SetChildRenderersEnabled(root, enabled);
            return;
        }

        SetRootRendererEnabled(root, enabled);
    }

    public static void SetOnlyMainColliderEnabled(Transform root, Collider mainCollider, bool enabled)
    {
        foreach (Collider childCollider in root.GetComponentsInChildren<Collider>(true))
        {
            childCollider.enabled = childCollider == mainCollider ? enabled : false;
        }
    }

    public static bool NameIsAllowed(GameObject instance, string[] allowedNames)
    {
        if (instance == null) return false;
        if (allowedNames == null || allowedNames.Length == 0) return true;

        string visualName = instance.name.Replace("_Visual", string.Empty).Trim();
        foreach (string allowedName in allowedNames)
        {
            if (string.Equals(visualName, allowedName?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static List<GameObject> LoadNamedResources(string folder, string[] names)
    {
        List<GameObject> loaded = new List<GameObject>();
        if (names == null || names.Length == 0)
        {
            loaded.AddRange(Resources.LoadAll<GameObject>(folder));
            return loaded;
        }

        foreach (string visualName in names)
        {
            if (string.IsNullOrWhiteSpace(visualName)) continue;

            GameObject visual = Resources.Load<GameObject>(folder + "/" + visualName.Trim());
            if (visual != null)
            {
                loaded.Add(visual);
            }
        }

        return loaded;
    }
}
