using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


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
    public float attackCooldown = 0.25f;
    public float attackDuration = 1f;
    public float whipLength = 2.0f;
    public float whipRadius = 0.35f;
    public int attackDamage = 1;
    public GameObject whipParticlePrefab;      
    public GameObject whipParticlePrefabUp;     
    public GameObject whipParticlePrefabDown;   
    [Range(0f, 1f)] public float attackMoveMultiplier = 0.35f; 
    private float attackCooldownTimer;
    public float attackInputThreshold = 0.5f;
    public bool allowDownAttackOnGround = false;
    public bool enablePogoBounce = true;
    public float pogoBounceForce = 12f;
    private Vector2 currentAttackDirection = Vector2.right;

    // Checks
    public Transform groundCheck;
    public Transform groundCheck2;
    public float groundCheckRadius = 0.25f;
    public LayerMask groundLayer;
    public Transform attackPoint;      
    public Transform attackPointUp;     
    public Transform attackPointDown;  
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

        if (attackCooldownTimer >0f)
            {
                attackCooldownTimer -= Time.deltaTime;
            }

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

        if (Input.GetButtonDown("Fire1") && attackCooldownTimer <= 0f && !isDashing)
        {
            currentAttackDirection = GetAttackDirection();
            StartCoroutine(WhipAttack());
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

    private Vector2 GetAttackDirection()
    {
        float vertical = Input.GetAxisRaw("Vertical");

        if (vertical > attackInputThreshold)
            return Vector2.up;

        if (vertical < -attackInputThreshold && (!isGrounded || allowDownAttackOnGround))
            return Vector2.down;

        return facingRight ? Vector2.right : Vector2.left;
    }

    private Vector2 GetAttackOrigin(Vector2 direction)
    {
        if (direction == Vector2.up && attackPointUp != null)
            return attackPointUp.position;

        if (direction == Vector2.down && attackPointDown != null)
            return attackPointDown.position;

        if (attackPoint != null)
            return attackPoint.position;

        return transform.position;
    }

    private GameObject GetAttackVfxPrefab(Vector2 direction)
    {
        if (direction == Vector2.up && whipParticlePrefabUp != null)
            return whipParticlePrefabUp;

        if (direction == Vector2.down && whipParticlePrefabDown != null)
            return whipParticlePrefabDown;

        return whipParticlePrefab;
    }

    IEnumerator WhipAttack()
    {
        isAttacking = true;
        attackCooldownTimer = attackCooldown;

        Vector2 direction = currentAttackDirection.normalized;
        Vector2 origin = GetAttackOrigin(direction);

        GameObject selectedVfx = GetAttackVfxPrefab(direction);
        if (selectedVfx != null)
        {
            Vector3 spawnPos = (Vector3)origin + (Vector3)(direction * 0.35f);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rot = Quaternion.Euler(0f, 0f, angle);
            GameObject vfx = Instantiate(selectedVfx, spawnPos, rot);
            Destroy(vfx, 0.05f);
        }

        RaycastHit2D[] hits = Physics2D.CircleCastAll(origin, whipRadius, direction, whipLength, attackLayer);
        HashSet<Collider2D> hitOnce = new HashSet<Collider2D>();
        bool hitSomething = false;

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null || hit.collider.attachedRigidbody == rb) continue;
            if (hitOnce.Contains(hit.collider)) continue;

            hitOnce.Add(hit.collider);
            hitSomething = true;
            hit.collider.gameObject.SendMessage("TakeDamage", attackDamage, SendMessageOptions.DontRequireReceiver);
        }

        if (enablePogoBounce && direction == Vector2.down && !isGrounded && hitSomething)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, pogoBounceForce);
        }

        yield return new WaitForSeconds(attackDuration);
        isAttacking = false;
    }

    private void FixedUpdate()
    {
        if (isDashing) return;

        float moveScale = isAttacking ? attackMoveMultiplier : 1f;
        float targetSpeed = moveInput * moveSpeed * moveScale;

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
        {
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (groundCheck2 != null)
        {
            Gizmos.DrawWireSphere(groundCheck2.position, groundCheckRadius);
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackPoint.position, whipRadius);
        }

        if (attackPointUp != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(attackPointUp.position, whipRadius);
        }

        if (attackPointDown != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPointDown.position, whipRadius);
        }

        Vector3 dir = Application.isPlaying
            ? (Vector3)(currentAttackDirection == Vector2.zero ? (facingRight ? Vector2.right : Vector2.left) : currentAttackDirection)
            : (facingRight ? Vector3.right : Vector3.left);

        Vector3 start = GetAttackOrigin(dir.normalized);
        Vector3 end = start + dir.normalized * whipLength;

        Gizmos.color = Color.white;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireSphere(end, whipRadius);
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }
}