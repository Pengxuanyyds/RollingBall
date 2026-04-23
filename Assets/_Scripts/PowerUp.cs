using UnityEngine;

/// <summary>
/// Shield power-up. Blocks one hit.
/// Attach to Shield prefab. Visual model can be freely swapped.
/// </summary>
public class PowerUp : MonoBehaviour
{
    public static event System.Action OnShieldCollected;

    private bool hasCollected = false;

    [Header("Rotation Animation")]
    public float rotateSpeed = 60f;

    void Update()
    {
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasCollected) return;
        if (!other.CompareTag("Player") && other.name != "Player") return;

        hasCollected = true;
        OnShieldCollected?.Invoke();

        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
    }

    private void OnEnable()
    {
        hasCollected = false;
        GetComponent<MeshRenderer>().enabled = true;
        GetComponent<Collider>().enabled = true;
    }
}
