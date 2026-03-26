using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float yOffset = 3f;
    [SerializeField] private float hitShakeDuration = 0.14f;
    [SerializeField] private Vector3 hitOffset = new Vector3(0.12f, -0.2f, 0f);

    private float hitShakeTimer;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 nextPosition = transform.position;
        nextPosition.y = target.position.y + yOffset;

        if (hitShakeTimer > 0f)
        {
            float t = hitShakeTimer / hitShakeDuration;
            nextPosition += hitOffset * t;
            hitShakeTimer -= Time.unscaledDeltaTime;
        }

        transform.position = nextPosition;
    }

    public void Configure(Transform followTarget, float offset)
    {
        target = followTarget;
        yOffset = offset;
    }

    public void PlayHitEffect()
    {
        hitShakeTimer = hitShakeDuration;
    }
}
