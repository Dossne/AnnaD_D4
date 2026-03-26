using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ParallaxMaterialScroller : MonoBehaviour
{
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private float yScrollFactor = 0.02f;
    [SerializeField] private float xScrollFactor = 0f;

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
        if (runtimeMaterial == null || cameraTarget == null)
        {
            return;
        }

        Vector3 cameraPosition = cameraTarget.position;
        runtimeMaterial.mainTextureOffset = new Vector2(
            baseOffset.x + cameraPosition.x * xScrollFactor,
            baseOffset.y + cameraPosition.y * yScrollFactor);
    }

    public void Configure(Transform target, float yFactor, float xFactor = 0f)
    {
        cameraTarget = target;
        yScrollFactor = yFactor;
        xScrollFactor = xFactor;
    }
}
