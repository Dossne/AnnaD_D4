using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ParallaxMaterialScroller : MonoBehaviour
{
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private float yScrollFactor = 0.02f;
    [SerializeField] private float xScrollFactor = 0f;
    [SerializeField] private float autoScrollY = 0.01f;
    [SerializeField] private float autoScrollX = 0f;

    private Material runtimeMaterial;
    private Vector2 baseOffset;

    private void Awake()
    {
        Renderer rendererComponent = GetComponent<Renderer>();
        runtimeMaterial = rendererComponent.material;
        baseOffset = runtimeMaterial.mainTextureOffset;
    }

    private void LateUpdate()
    {
        if (runtimeMaterial == null)
        {
            return;
        }

        Vector3 cameraPosition = cameraTarget == null ? Vector3.zero : cameraTarget.position;
        float time = Time.time;

        runtimeMaterial.mainTextureOffset = new Vector2(
            baseOffset.x + cameraPosition.x * xScrollFactor + time * autoScrollX,
            baseOffset.y + cameraPosition.y * yScrollFactor + time * autoScrollY);
    }

    public void Configure(Transform target, float yFactor, float xFactor = 0f, float driftY = 0.01f, float driftX = 0f)
    {
        cameraTarget = target;
        yScrollFactor = yFactor;
        xScrollFactor = xFactor;
        autoScrollY = driftY;
        autoScrollX = driftX;
    }
}
