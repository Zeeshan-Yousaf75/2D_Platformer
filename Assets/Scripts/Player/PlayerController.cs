using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerController : MonoBehaviour
{
    
    private float movementInputDirection;
    private float dashTimeLeft;
    private float lastImageXpos;
    private float lastDash = -100f;
    private float knockbackStartTime;
    [SerializeField]
    private float knockbackDuration;

    private int amountOfJumpsLeft;
    private int facingDirection = 1;
    

    private bool isFacingRight = true;
    private bool isWalking;
    private bool isGrounded;
    private bool isTouchingWall;
    //private
    public bool isWallSliding;
    private bool canJump;
    private bool isDashing;
    private bool knockback;

    [SerializeField]
    private Vector2 knockbackSpeed;

    private Rigidbody2D rb;
    private Animator anim;


    public int amountOfJumps = 1;

    public float movementSpeed = 10.0f;
    public float jumpForce = 16.0f;
    public float groundCheckRadius;
    public float wallCheckDistance;
    public float wallSlideSpeed;
    public float movementForceInAir;
    public float airDragMultiplier = 0.95f;
    public float variableJumpHeightMultiplier = 0.5f;
    public float wallHopForce;
    public float wallJumpForce;
    public float dashTime;
    public float dashSpeed;
    public float distanceBetweenImages;
    public float dashCoolDown;

    public Vector2 wallHopDirection;
    public Vector2 wallJumpDirection;

    public Transform groundCheck;
    public Transform wallCheck;

    public LayerMask whatIsGround;

   
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        amountOfJumpsLeft = amountOfJumps;
        wallHopDirection.Normalize();
        wallJumpDirection.Normalize();
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
        CheckMovementDirection();
        UpdateAnimations();
        CheckIfCanJump();
        CheckIfWallSliding();
        CheckDash();
        CheckKnockback();
        
    }

    private void FixedUpdate()
    {
        
        ApplyMovement();
        CheckSurroundings();
        
    }

    private void CheckIfWallSliding()
    {
        if (isTouchingWall && !isGrounded && rb.velocity.y <0)
        {
            isWallSliding = true;
            
        }
        else
        {
            isWallSliding = false;
        }
    }

    public bool GetDashStatus()
    {
        return isDashing;
    }
    public void Knockback(int diraction)
    {
        knockback = true;
        knockbackStartTime = Time.time;
        rb.velocity = new Vector2(knockbackSpeed.x * diraction, knockbackSpeed.y);
    }

    private void CheckKnockback()
    {
        if (Time.time >= knockbackStartTime + knockbackDuration && knockback)
        {
            knockback = false;
            rb.velocity = new Vector2(0.0f, rb.velocity.y);
        }
    }
    private void CheckSurroundings()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position , groundCheckRadius, whatIsGround);
        

        isTouchingWall = Physics2D.Raycast(wallCheck.position,transform.right,wallCheckDistance,whatIsGround);
    }

    private void CheckIfCanJump()
    {
        if ((isGrounded && rb.velocity.y <= 0) || isWallSliding)
        {
           // canJump = true;
            amountOfJumpsLeft = amountOfJumps;
        }
      
        if (amountOfJumpsLeft <=0)
        {
            canJump = false;
        }
        else
        {
            canJump = true;
        }
    }
    private void CheckMovementDirection()
    {
        if (isFacingRight && movementInputDirection <0)
        {
            Flip();

        }
        else if (!isFacingRight && movementInputDirection >0)
        {
            
            Flip();
        }
      
        if (Mathf.Abs(rb.velocity.x) >= 0.01f) //rb.velocity.x != 0
        {
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }
    }

    private void UpdateAnimations()
    {
        anim.SetBool("isWalking", isWalking);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isWallSliding", isWallSliding);
    }
    private void CheckInput()
    {
        movementInputDirection =  Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown ("Jump"))
        {
            Jump();
        }

        if (Input.GetButtonUp("Jump"))
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * variableJumpHeightMultiplier);
        }

        if (Input.GetButtonDown("Dash"))
        {
            //canMove = false;
           // Debug.Log("dash");
            if (Time.time >=(lastDash + dashCoolDown))
            {
                AttemptToDash();
                
            }
        }
       
        
    }


    private void AttemptToDash()
    {
        
        isDashing = true;

        dashTimeLeft = dashTime;
        lastDash = Time.time;
        Debug.Log("dash");
        PlayerAfterImagePool.Instance.GetFromPool();
        lastImageXpos = transform.position.x;
    }

    public int GetFacingDirection()
    {
        return facingDirection;
    }
    private void CheckDash()
    {
        if (isDashing)
        {
            if (dashTimeLeft > 0)
            {
 
                rb.velocity = new Vector2(dashSpeed * facingDirection, rb.velocity.y);
                 dashTimeLeft -= Time.deltaTime;

                 if (Mathf.Abs(transform.position.x - lastImageXpos)> distanceBetweenImages)
                 {
                    PlayerAfterImagePool.Instance.GetFromPool();
                    lastImageXpos = transform.position.x;
                 }

            }

            if (dashTimeLeft <= 0 || isTouchingWall)
            {
                isDashing = false;
            }
        }
    }
    private void Jump()
    {
        if (canJump && !isWallSliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            amountOfJumpsLeft --;
        }
        else if (isWallSliding && movementInputDirection == 0 && canJump) //wall hop
        {
            isWallSliding = false;
            amountOfJumpsLeft--;

            Vector2 forceToAdd = new Vector2(wallHopForce * wallHopDirection.x * -facingDirection , wallHopForce * wallHopDirection.y );
            rb.AddForce(forceToAdd,ForceMode2D.Impulse);
        }
        else if ((isWallSliding || isTouchingWall) && movementInputDirection !=0 &&  canJump)
        {
            isWallSliding = false;
            amountOfJumpsLeft--;

            Vector2 forceToAdd = new Vector2(wallJumpForce * wallJumpDirection.x * movementInputDirection, wallJumpForce * wallJumpDirection.y);
            rb.AddForce(forceToAdd, ForceMode2D.Impulse);
        }
       
    }

    private void ApplyMovement()
    {
        
        if (!isGrounded && !isWallSliding && movementInputDirection ==0 && !knockback)
        {
            rb.velocity = new Vector2(rb.velocity.x * airDragMultiplier, rb.velocity.y);
        }

       if(isGrounded && !isDashing) // when player is on ground
       {
          rb.velocity = new Vector2(movementSpeed * movementInputDirection , rb.velocity.y);

        }

        if (!isGrounded && !isWallSliding && movementInputDirection != 0) // when player is on ground
        {
            rb.velocity = new Vector2(movementSpeed * movementInputDirection, rb.velocity.y);

        }

        if (isWallSliding)
        {
            if (rb.velocity.y < -wallSlideSpeed)
            {
                rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
            }
        }
    }
    public void DisableFlip()
    {
       
           // canFlip = false;
    }
    public void EnableFlip()
    {

        // canFlip = true;
    }

    
    private void Flip()
    {
        if (!isWallSliding && !knockback)
        {
        facingDirection *= -1;
        isFacingRight = !isFacingRight;
        transform.Rotate(0.0f,180.0f,0.0f);

        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));
    }

}
