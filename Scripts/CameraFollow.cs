using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow Settings")]
    [SerializeField] private float smoothTime = 0.15f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    private Vector3 velocity = Vector3.zero;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 targetPosition = target.position + offset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}