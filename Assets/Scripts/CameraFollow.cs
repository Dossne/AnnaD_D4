using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float yOffset = 3f;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 nextPosition = transform.position;
        nextPosition.y = target.position.y + yOffset;
        transform.position = nextPosition;
    }

    public void Configure(Transform followTarget, float offset)
    {
        target = followTarget;
        yOffset = offset;
    }
}
