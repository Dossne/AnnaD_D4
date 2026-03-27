using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float earlyUpwardSpeed = 4.5f;
    [SerializeField] private float midUpwardSpeed = 6f;
    [SerializeField] private float lateUpwardSpeed = 7.8f;
    [SerializeField] private float speedIncreasePerSecond = 0.08f;
    [SerializeField] private float switchSpeed = 24f;
    [SerializeField] private float wallSnapDistance = 0.12f;
    [SerializeField] private float leftWallX = -2f;
    [SerializeField] private float rightWallX = 2f;
    [SerializeField] private float inputBlockAfterSpawn = 0.15f;

    [Header("State")]
    [SerializeField] private bool startOnLeftWall = true;
    [SerializeField] private Transform visualRoot;

    private Rigidbody2D rb;
    private bool isAlive = true;
    private bool isOnLeftWall;
    private float elapsedRunTime;
    private float inputBlockTimer;

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
        isOnLeftWall = startOnLeftWall;

        Vector2 startPosition = rb.position;
        startPosition.x = isOnLeftWall ? leftWallX : rightWallX;
        rb.position = startPosition;
        visualRoot.rotation = Quaternion.identity;
        inputBlockTimer = inputBlockAfterSpawn;
    }

    private void Update()
    {
        if (inputBlockTimer > 0f)
        {
            inputBlockTimer -= Time.deltaTime;
            return;
        }

        if (isAlive && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            SwitchSide();
        }
    }

    private void FixedUpdate()
    {
        if (!isAlive)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        elapsedRunTime += Time.fixedDeltaTime;
        float currentUpwardSpeed = GetCurrentUpwardSpeed();

        float targetX = isOnLeftWall ? leftWallX : rightWallX;
        float nextX = Mathf.MoveTowards(rb.position.x, targetX, switchSpeed * Time.fixedDeltaTime);
        if (Mathf.Abs(targetX - nextX) <= wallSnapDistance)
        {
            nextX = targetX;
        }

        Vector2 nextPosition = new Vector2(nextX, rb.position.y + currentUpwardSpeed * Time.fixedDeltaTime);
        rb.MovePosition(nextPosition);
        visualRoot.rotation = Quaternion.identity;
    }

    private float GetCurrentUpwardSpeed()
    {
        float baseTopSpeed = Mathf.Max(midUpwardSpeed, lateUpwardSpeed);
        float acceleratedSpeed = earlyUpwardSpeed + elapsedRunTime * speedIncreasePerSecond;
        return Mathf.Min(acceleratedSpeed, baseTopSpeed);
    }

    private void SwitchSide()
    {
        isOnLeftWall = !isOnLeftWall;
        GameAudio.PlaySwitch();
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

    public void Configure(
        float moveSpeed,
        float horizontalSwitchSpeed,
        float leftX,
        float rightX,
        bool startLeft,
        Transform visualTarget = null,
        float midSpeed = 6f,
        float lateSpeed = 7.8f,
        float snapDistance = 0.12f,
        float accelerationPerSecond = 0.08f)
    {
        earlyUpwardSpeed = moveSpeed;
        switchSpeed = horizontalSwitchSpeed;
        leftWallX = leftX;
        rightWallX = rightX;
        startOnLeftWall = startLeft;
        isOnLeftWall = startOnLeftWall;
        visualRoot = visualTarget == null ? transform : visualTarget;
        midUpwardSpeed = midSpeed;
        lateUpwardSpeed = lateSpeed;
        wallSnapDistance = snapDistance;
        speedIncreasePerSecond = accelerationPerSecond;
    }
}
