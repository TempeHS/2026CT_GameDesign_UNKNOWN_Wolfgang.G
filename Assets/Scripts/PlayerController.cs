using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Movement
    public float moveSpeed = 5f;
    public float acceleration = 60f;
    public float deceleration = 80f;
    public float airAcceleration = 30f;
    public float airDeceleration = 40f;

    // Jump
    public float jumpForce = 16f;
    public float delayedJumpTime = 0.15f;
    public float jumpBufferTime = 0.15f;
    private float delayedJumpTimeCounter;
    private float jumpBufferCounter;
    private bool jumpHeld;

    // Checks
    public Transform groundCheck;
    public float groundCheckRadius = 0.25f;
    public LayerMask groundLayer;
    private bool isGrounded;

    // Input
    private float moveInput;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal"); 

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (isGrounded)
        {
            delayedJumpTimeCounter = delayedJumpTime;
        }
        else
        {
            delayedJumpTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0) 
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }

        jumpHeld = Input.GetButton("Jump");
    }

    private void FixedUpdate()
    {
        float targetSpeed = moveInput * moveSpeed;
        float accelRate = isGrounded ? (Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration)
                                     : (Mathf.Abs(targetSpeed) > 0.01f ? airAcceleration : airDeceleration);

        float speedDiff = targetSpeed - rb.linearVelocity.x; 
        float movement = speedDiff * accelRate * Time.fixedDeltaTime;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x + movement, rb.linearVelocity.y); 

        if (jumpBufferCounter > 0 && delayedJumpTimeCounter > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce); 
            jumpBufferCounter = 0;
            delayedJumpTimeCounter = 0;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}