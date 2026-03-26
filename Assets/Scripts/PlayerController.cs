using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float upwardSpeed = 4f;
    [SerializeField] private float leftWallX = -2f;
    [SerializeField] private float rightWallX = 2f;

    [Header("State")]
    [SerializeField] private bool startOnLeftWall = true;

    private Rigidbody2D rb;
    private bool isAlive = true;
    private bool isOnLeftWall;

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
        Vector2 nextPosition = new Vector2(targetX, rb.position.y + upwardSpeed * Time.fixedDeltaTime);
        rb.MovePosition(nextPosition);
    }

    private void SwitchSide()
    {
        isOnLeftWall = !isOnLeftWall;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAlive || !other.CompareTag("Obstacle"))
        {
            return;
        }

        isAlive = false;
        ScoreManager.Instance?.GameOver();
    }
}
