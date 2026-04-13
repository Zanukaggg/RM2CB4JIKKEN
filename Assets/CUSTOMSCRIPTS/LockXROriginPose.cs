using UnityEngine;

[DisallowMultipleComponent]
public class LockXROriginPose : MonoBehaviour
{
    [Tooltip("Apply only once at Start")]
    public bool applyOnStart = true;

    private Vector3 worldPosition;
    private Quaternion worldRotation;

    void Awake()
    {
        worldPosition = transform.position;
        worldRotation = transform.rotation;
    }

    void Start()
    {
        if (!applyOnStart) return;

        transform.SetPositionAndRotation(worldPosition, worldRotation);
    }
}
