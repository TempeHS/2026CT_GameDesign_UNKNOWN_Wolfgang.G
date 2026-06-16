using UnityEngine;
using System.Collections;
using System;


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
    private bool jumpConsumed;  

    // Dash
    public float dashSpeed = 10;
    public float dashCooldown = 1f;
    public float dashDuration = 0.5f;
    private float dashDirection;

    // Attack
    [Range(0f, 360f)] public float attackArcAngle = 180f;
    

    // Checks
    public Transform groundCheck;
    public Transform groundCheck2;
    public float groundCheckRadius = 0.25f;
    public LayerMask groundLayer;
    public Transform attackPoint;
    public LayerMask attackLayer;
    public float attackRange = 1.2f;
    private bool isGrounded;
    private bool facingRight = true;
    private bool isAttacking;
    private bool isDashing;
    public bool canDash;

    


    // Input
    private float moveInput;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        canDash = true;

        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        bool groundedA = groundCheck != null &&
                     Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        bool groundedB = groundCheck2 != null &&
                     Physics2D.OverlapCircle(groundCheck2.position, groundCheckRadius, groundLayer);

        isGrounded = groundedA || groundedB;

        bool attackingCheck = attackPoint != null &&
                     Physics2D.OverlapCircle(attackPoint.position, attackRange, attackLayer);

        isAttacking = attackingCheck;


        if (isGrounded && rb.linearVelocity.y <= 0.01f)
        {
            delayedJumpTimeCounter = delayedJumpTime;
            jumpConsumed = false;
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

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash());
        }

        if (moveInput > 0 && !facingRight)
        {
            Flip();
        }   
        else if (moveInput < 0 && facingRight)
        {
            Flip();
        }

        if (facingRight == true)
        {
            dashDirection = 1;
        } else if (facingRight == false)
        {
            dashDirection = -1;
        }
    }
    
    IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;
        // When damage is set up, make player invincible
        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, rb.linearVelocity.y);
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
        // When damage is set up, remove invincibility
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            return;
        }

        float targetSpeed = moveInput * moveSpeed;
        float accelRate = isGrounded 
                ? (Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration)
                : (Mathf.Abs(targetSpeed) > 0.01f ? airAcceleration : airDeceleration);

        float newX = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, accelRate * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);

        if (jumpBufferCounter > 0 && delayedJumpTimeCounter > 0 && !jumpConsumed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0;
            delayedJumpTimeCounter = 0;
            jumpConsumed = true;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        if (groundCheck2 != null)
            Gizmos.DrawWireSphere(groundCheck2.position, groundCheckRadius);
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }
}