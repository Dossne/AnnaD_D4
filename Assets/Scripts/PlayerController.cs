using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float upwardSpeed = 4f;
    [SerializeField] private float switchSpeed = 14f;
    [SerializeField] private float leftWallX = -2f;
    [SerializeField] private float rightWallX = 2f;
    [SerializeField] private float tiltAngle = 18f;
    [SerializeField] private float tiltSpeed = 10f;

    [Header("State")]
    [SerializeField] private bool startOnLeftWall = true;
    [SerializeField] private Transform visualRoot;

    private Rigidbody2D rb;
    private bool isAlive = true;
    private bool isOnLeftWall;
    private float currentTilt;

    public bool IsAlive => isAlive;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        isOnLeftWall = startOnLeftWall;

        if (rb == null)
        {
            Debug.LogError("PlayerController needs a Rigidbody2D.");
            enabled = false;
        }

        if (visualRoot == null)
        {
            visualRoot = transform;
        }
    }

    private void Start()
    {
        Vector2 startPosition = rb.position;
        startPosition.x = isOnLeftWall ? leftWallX : rightWallX;
        rb.position = startPosition;
    }

    private void Update()
    {
        if (isAlive && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            SwitchSide();
        }

        if (!isAlive && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.R)))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void FixedUpdate()
    {
        if (!isAlive)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        float targetX = isOnLeftWall ? leftWallX : rightWallX;
        float nextX = Mathf.MoveTowards(rb.position.x, targetX, switchSpeed * Time.fixedDeltaTime);
        Vector2 nextPosition = new Vector2(nextX, rb.position.y + upwardSpeed * Time.fixedDeltaTime);
        rb.MovePosition(nextPosition);

        float targetTilt = isOnLeftWall ? tiltAngle : -tiltAngle;
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, tiltSpeed * Time.fixedDeltaTime);
        visualRoot.rotation = Quaternion.Euler(0f, 0f, currentTilt);
    }

    private void SwitchSide()
    {
        isOnLeftWall = !isOnLeftWall;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAlive || other.GetComponent<ObstacleMarker>() == null)
        {
            return;
        }

        isAlive = false;
        ScoreManager.Instance?.GameOver();
    }

    public void Configure(float moveSpeed, float horizontalSwitchSpeed, float leftX, float rightX, bool startLeft, Transform visualTarget = null)
    {
        upwardSpeed = moveSpeed;
        switchSpeed = horizontalSwitchSpeed;
        leftWallX = leftX;
        rightWallX = rightX;
        startOnLeftWall = startLeft;
        isOnLeftWall = startOnLeftWall;
        visualRoot = visualTarget == null ? transform : visualTarget;
    }
}
