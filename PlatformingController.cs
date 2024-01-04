using GameJamHelpers.Generic;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace PlatformingScripts
{
    public class PlatformingController : MonoBehaviour
    {
        [Header("General")]
        [SerializeField]
        private bool movementEnabled = true;
        [SerializeField]
        private float gravityScaleRising = 4f;
        [SerializeField]
        private float gravityScaleFalling = 2f;
        [SerializeField]
        private float maxFallSpeed = 5f;
        [SerializeField]
        private bool spritesFaceRight = true;
        [SerializeField]
        private bool flipSprites = true;

        [Header("Run")]
        private bool runEnabled = true;
        [SerializeField]
        private float runSpeed = 8f;
        [SerializeField]
        private LayerMask whatIsGround;
        [SerializeField]
        private float groundCheckPadding = 0.1f;
        [SerializeField]
        private LayerMask whatIsWall;
        [SerializeField]
        private float wallCheckPadding = 0.1f;

        [Header("Jump")]
        [SerializeField]
        private bool jumpEnabled = true;
        [SerializeField]
        private float jumpMinHeight = 1.5f;
        [SerializeField]
        private float jumpMaxHeight = 3.6f;
        [SerializeField]
        private float jumpDampHeight = 0.2f;
        [SerializeField]
        private float coyoteTime = 0.1f;
        private float coyoteTimer;
        [SerializeField]
        private int jumpsAllowed = 1;
        private int jumpsRemaining;

        [Header("Wall Slide")]
        [SerializeField]
        private bool wallSlideEnabled = true;
        [SerializeField]
        private float wallSlideSpeed = 1f;
        [SerializeField]
        private float wallSlideDeceleration = 0.1f;

        [Header("Wall Jump")]
        [SerializeField]
        private bool wallJumpEnabled = true;
        [SerializeField]
        private float wallJumpMinHeight = 1f;
        [SerializeField]
        private float wallJumpMaxHeight = 2f;
        [SerializeField]
        private float wallJumpDampHeight = 0.2f;
        [SerializeField]
        private float wallJumpDistance = 2f;
        [SerializeField]
        private float wallJumpInputBlendingOffset = 0.05f;
        [SerializeField]
        private float wallJumpInputBlendingTime = 0.1f;
        [SerializeField]
        private AnimationCurve wallJumpInputBlendingCurve;
        [SerializeField]
        private float wallCoyoteTime = 0.1f;
        private float wallCoyoteTimer;


        [Header("Dash")]
        [SerializeField]
        private bool dashEnabled = true;
        [SerializeField]
        private float dashDistance = 3.6f;
        [SerializeField]
        private float dashTime = 0.3f;
        private float dashTimer;
        [SerializeField]
        private float dashStartLag = 0.2f;
        [SerializeField]
        private float dashEndLag = 0f;
        [SerializeField]
        private int dashesAllowed = 1;
        private int dashesRemaining;
        [SerializeField]
        private PrefabEmitter dashTrail = null;

        [Header("Events")]
        [SerializeField]
        private UnityEvent onWallSlideLeftStarted;
        [SerializeField]
        private UnityEvent onWallSlideRightStarted;
        [SerializeField]
        private UnityEvent onWallSlideLeftEnded;
        [SerializeField]
        private UnityEvent onWallSlideRightEnded;

        private Rigidbody2D rb;
        private Collider2D playerCollider;
        private PlayerInput playerInput;
        private InputAction movementInputAction;
        private SpriteRenderer sr;
        private Vector2 movementInput;
        private float facingDirection;
        public bool IsMoving => Mathf.Abs(rb.velocity.x) > 0.1f;

        #region Disabling Movement
        [SerializeField]
        private int movementInterrupters = 0;
        private bool canMove => movementEnabled && movementInterrupters == 0;

        public void AddMovementInterrupter() { movementInterrupters++; }
        public void RemoveMovementInterrupter() { movementInterrupters--; }

        #endregion


        #region Movement Stuff

        private void ProcessMovement()
        {
            if (runEnabled)
            {
                SetFacingDirection();
                float xSpeed = runSpeed * movementInput.x;
                float ySpeed = rb.velocity.y;
                if (isWallSliding && ySpeed <= -wallSlideSpeed)
                {
                    rb.gravityScale = 0f;
                    ySpeed += wallSlideDeceleration;
                    if (ySpeed > -wallSlideSpeed)
                    {
                        ySpeed = -wallSlideSpeed;
                    }
                }
                rb.velocity = new Vector2(xSpeed, ySpeed);
            }
        }

        private void SetFacingDirection()
        {
            if (movementInput.x != 0)
            {
                facingDirection = movementInput.x;
                sr.flipX = (spritesFaceRight ? movementInput.x < 0 : movementInput.x > 0) && flipSprites;
            }
        }

        private bool moveToCoroutineIsActive = false;
        public event EventHandler<EventArgs> MoveToLocationReached;

        public void MoveTo(float xCoord)
        {
            if (!moveToCoroutineIsActive)
            {
                StartCoroutine(MoveToCoroutine(xCoord));
            }
        }

        private IEnumerator MoveToCoroutine(float xCoord)
        {
            moveToCoroutineIsActive = true;
            AddMovementInterrupter();
            isWallJumping = false;
            isDashing = false;
            float initialDirectionToTarget = this.gameObject.transform.position.x > xCoord ? -1 : 1;
            float currentDirectionToTarget = initialDirectionToTarget;
            facingDirection = initialDirectionToTarget;
            sr.flipX = (spritesFaceRight ? initialDirectionToTarget < 0 : initialDirectionToTarget > 0) && flipSprites;
            while (currentDirectionToTarget * -1 != initialDirectionToTarget)
            {
                rb.velocity = new Vector2(runSpeed * initialDirectionToTarget, rb.velocity.y);
                currentDirectionToTarget = this.gameObject.transform.position.x > xCoord ? -1 : 1;
                yield return new WaitForFixedUpdate();
            }
            rb.velocity = new Vector2(0f, rb.velocity.y);
            MoveToLocationReached?.Invoke(this, EventArgs.Empty);
            RemoveMovementInterrupter();
            moveToCoroutineIsActive = false;
        }

        public void ResetMovement()
        {
            isWallJumping = false;
            isDashing = false;
            rb.velocity = Vector2.zero;
        }
        #endregion


        #region Ground Check stuff
        private bool isGrounded;
        public bool IsGrounded => isGrounded;
        public bool CheckGrounded()
        {
            return GetGroundCollider() != null;
        }

        public Collider2D GetGroundCollider()
        {
            RaycastHit2D raycastHit = Physics2D.BoxCast(playerCollider.bounds.center, playerCollider.bounds.size, 0f, Vector2.down, groundCheckPadding, whatIsGround);
            EventTrigger_OnPlayerGround groundTrigger = raycastHit.collider?.gameObject.GetComponent<EventTrigger_OnPlayerGround>();
            if (groundTrigger != null)
            {
                groundTrigger.Activate();
            }
            Color rayColor;
            rayColor = (raycastHit.collider != null) ? Color.green : Color.red;
            Debug.DrawRay(playerCollider.bounds.center - new Vector3(playerCollider.bounds.extents.x, playerCollider.bounds.extents.y + groundCheckPadding), Vector2.right * (playerCollider.bounds.extents.x), rayColor);
            return raycastHit.collider;
        }
        #endregion


        #region Wall Jump stuff
        private bool isWallJumping;
        public bool IsWallJumping => isWallJumping;
        private void SetIsWallJumping(bool jumping)
        {
            isWallJumping = jumping;
        }
        private float wallJumpDirection;
        private float wallJumpInputBlendingTimer;
        //velocity change over the duration of the offset
        //also the velocity at which blending should start
        private float wallJumpInputBlendOffsetVelocity => wallJumpInputBlendingOffset * -Physics2D.gravity.y * gravityScaleRising;


        private float wallJumpXvelocity => wallJumpDistance / (PhysicsUtility.HeightToTime(wallJumpMaxHeight, Physics2D.gravity.y, gravityScaleRising));
        //private float wallJumpMinXvelocity => wallJumpMinDistance / (PhysicsUtility.HeightToTime(wallJumpMinHeight, Physics2D.gravity.ySpeed, gravityScaleRising));
        //private float wallJumpMidXvelocity => (wallJumpMaxXvelocity + wallJumpMinXvelocity) / 2;
        private float wallJumpMaxYvelocity => PhysicsUtility.HeightToVelocity(wallJumpMaxHeight, Physics2D.gravity.y, gravityScaleRising);
        private float wallJumpDampLimitVelocity => PhysicsUtility.HeightToVelocity(wallJumpMaxHeight - wallJumpMinHeight + wallJumpDampHeight, Physics2D.gravity.y, gravityScaleRising);
        private float wallJumpDampYvelocity => PhysicsUtility.HeightToVelocity(wallJumpDampHeight, Physics2D.gravity.y, gravityScaleRising);

        private void ProcessWallJump()
        {
            float xVel;
            float yVel = rb.velocity.y;
            // Set initial ySpeed velocity
            if (jumpPerformed)
            {
                jumpPerformed = false;
                if (isHoldingTowardsWall || wallCoyoteTimer < wallCoyoteTime)
                {
                    if (isDashing) { CancelDash(); }
                    isWallJumping = true;
                    wallJumpDirection = wasTouchingWhichWall == WallCheckResult.Left ? 1 : -1;
                    rb.gravityScale = gravityScaleRising;
                    yVel = wallJumpMaxYvelocity;
                    wallJumpInputBlendingTimer = 0f;
                }
            }
            if (isTouchingWall &&
                (wallJumpDirection == 1 && isTouchingWhichWall == WallCheckResult.Right) ||
                (wallJumpDirection == -1 && isTouchingWhichWall == WallCheckResult.Left))
            {
                isWallJumping = false;
            }
            // Set x velocity
            if (isWallJumping)
            {
                if (yVel == wallJumpMaxYvelocity || rb.velocity.y > wallJumpInputBlendOffsetVelocity || wallJumpInputBlendingTimer <= wallJumpInputBlendingTime)
                {
                    if (rb.velocity.y > wallJumpInputBlendOffsetVelocity || yVel == wallJumpMaxYvelocity)
                    {
                        xVel = wallJumpXvelocity * wallJumpDirection;
                        sr.flipX = (spritesFaceRight ? wallJumpDirection < 0 : wallJumpDirection > 0) && flipSprites;
                    }
                    else
                    {
                        float timeRatio = wallJumpInputBlendingTimer / wallJumpInputBlendingTime;
                        xVel = Mathf.Lerp(wallJumpXvelocity * wallJumpDirection, runSpeed * movementInput.x, wallJumpInputBlendingCurve.Evaluate(timeRatio));
                        SetFacingDirection();
                        wallJumpInputBlendingTimer += Time.deltaTime;
                    }
                    rb.velocity = new Vector2(xVel, yVel);
                }
                else
                {
                    isWallJumping = false;
                }
            }
        }

        private void OnWallJumpCancelled()
        {
            if (rb.velocity.y > wallJumpDampLimitVelocity)
            {
                float currentHeight = PhysicsUtility.VelocityChangeToHeight(wallJumpMaxYvelocity, rb.velocity.y, Physics2D.gravity.y, gravityScaleRising);
                rb.velocity = new Vector2(rb.velocity.x, PhysicsUtility.HeightToVelocity(wallJumpMinHeight - currentHeight, Physics2D.gravity.y, gravityScaleRising));
            }
            else if (rb.velocity.y > wallJumpDampYvelocity)
            {
                rb.velocity = new Vector2(rb.velocity.x, wallJumpDampYvelocity);
            }
        }

        #endregion


        #region Wall Slide stuff
        private bool isWallSliding;
        private bool wasWallSliding;
        public bool IsWallSliding => isWallSliding;

        private WallCheckResult isTouchingWhichWall;
        private WallCheckResult wasTouchingWhichWall;
        private bool isTouchingWall;
        private bool isHoldingTowardsWall;

        private void SetIsWallSliding(bool sliding)
        {
            isWallSliding = sliding;
        }

        public WallCheckResult TouchingWhichWall()
        {
            RaycastHit2D raycastHitLeft = Physics2D.BoxCast(playerCollider.bounds.center, playerCollider.bounds.size, 0f, Vector2.left, wallCheckPadding, whatIsWall);
            if (raycastHitLeft.collider != null)
            {
                return WallCheckResult.Left;
            }
            RaycastHit2D raycastHitRight = Physics2D.BoxCast(playerCollider.bounds.center, playerCollider.bounds.size, 0f, Vector2.right, wallCheckPadding, whatIsWall);
            if (raycastHitRight.collider != null)
            {
                return WallCheckResult.Right;
            }
            return WallCheckResult.None;
        }
        private void CheckIfWallSliding()
        {
            isTouchingWhichWall = TouchingWhichWall();
            wasTouchingWhichWall = isTouchingWhichWall == WallCheckResult.None ? wasTouchingWhichWall : isTouchingWhichWall;

            isTouchingWall = isTouchingWhichWall != WallCheckResult.None;


            wasWallSliding = isWallSliding;


            isHoldingTowardsWall = !isGrounded && ((isTouchingWhichWall == WallCheckResult.Left && (movementInput.x < 0))
                || isTouchingWhichWall == WallCheckResult.Right && (movementInput.x > 0));

            isWallSliding = wallSlideEnabled ? isHoldingTowardsWall && !isRising : false;
        }

        private void ProcessWallSlideEvents()
        {
            if (!wasWallSliding && isWallSliding)
            {
                if (isTouchingWhichWall == WallCheckResult.Left)
                {
                    onWallSlideLeftStarted.Invoke();
                }
                else
                {
                    onWallSlideRightStarted.Invoke();
                }
            }
            else if (wasWallSliding && !isWallSliding)
            {
                if (wasTouchingWhichWall == WallCheckResult.Left)
                {
                    onWallSlideLeftEnded.Invoke();
                }
                else
                {
                    onWallSlideRightEnded.Invoke();
                }
            }
        }
        #endregion


        #region Jump stuff
        private bool isRising;
        public bool IsRising => isRising;
        private bool wasRising;
        public bool WasRising => wasRising;

        private bool jumpPerformed;
        private bool jumpCancelled;
        private float jumpMaxVel => PhysicsUtility.HeightToVelocity(jumpMaxHeight, Physics2D.gravity.y, gravityScaleRising);
        private float jumpDampLimitVel => PhysicsUtility.HeightToVelocity(jumpMaxHeight - jumpMinHeight + jumpDampHeight, Physics2D.gravity.y, gravityScaleRising);
        private float jumpDampVel => PhysicsUtility.HeightToVelocity(jumpDampHeight, Physics2D.gravity.y, gravityScaleRising);


        public void OnJump(InputAction.CallbackContext context)
        {
            if (canMove)
            {
                if (context.performed) { jumpPerformed = true; }
                if (context.canceled) { jumpCancelled = true; }
            }
        }
        private void OnJumpPerformed()
        {
            jumpPerformed = false;
            if (
                jumpEnabled &&
                ((jumpsRemaining < jumpsAllowed && jumpsRemaining > 0) ||
                (jumpsRemaining == jumpsAllowed && jumpsRemaining > 0 && (isGrounded || coyoteTimer < coyoteTime)))
            )
            {
                jumpsRemaining--;
                rb.gravityScale = gravityScaleRising;
                rb.velocity = new Vector2(rb.velocity.x, jumpMaxVel);
            }
        }
        private void OnJumpCancelled()
        {
            if (rb.velocity.y > jumpDampLimitVel)
            {
                float currentHeight = PhysicsUtility.VelocityChangeToHeight(jumpMaxVel, rb.velocity.y, Physics2D.gravity.y, gravityScaleRising);
                rb.velocity = new Vector2(rb.velocity.x, PhysicsUtility.HeightToVelocity(jumpMinHeight - currentHeight, Physics2D.gravity.y, gravityScaleRising));
            }
            else if (rb.velocity.y > jumpDampVel)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpDampVel);
            }
        }

        #endregion


        #region Dashing stuff

        public bool IsPreDashing => dashTimer <= dashStartLag && isDashing;
        private bool isDashing;
        public bool IsDashing => isDashing;

        private bool dashPerformed;
        private Vector2 dashDirection;

        public void OnDash(InputAction.CallbackContext context)
        {
            if (canMove)
            {
                if (context.performed) { dashPerformed = true; }
            }

        }
        private void OnDashPerformed()
        {
            dashPerformed = false;
            if (dashEnabled && dashesRemaining > 0)
            {
                dashesRemaining--;
                dashTimer = 0;
                isDashing = true;
                isWallJumping = false;
            }

        }

        private void ProcessDash()
        {
            dashTimer += Time.deltaTime;
            //Start Lag
            if (dashTimer <= dashStartLag)
            {
                rb.velocity = Vector2.zero;
                dashDirection = GetDashDirection();
            }
            //Dash
            else if (dashTimer <= dashStartLag + dashTime)
            {
                if (dashTrail != null)
                {
                    dashTrail.SetActive(true);
                }
                rb.velocity = dashDirection.normalized * (dashDistance / dashTime);
            }
            //End Lag
            else if (dashTimer <= dashStartLag + dashTime + dashEndLag)
            {
                rb.velocity = Vector2.zero;
            }
            //End Dash
            else
            {
                if (dashTrail != null)
                {
                    dashTrail.SetActive(false);
                }
                rb.velocity = new Vector2(0f, rb.velocity.y * 0.5f);
                isDashing = false;
                dashTimer = 0;
            }
        }

        private void CancelDash()
        {
            if (dashTrail != null)
            {
                dashTrail.SetActive(false);
            }
            isDashing = false;
            dashTimer = 0;
        }


        private Vector2 GetDashDirection()
        {
            if (movementInput.x != 0)
            {
                facingDirection = movementInput.x;
            }
            return movementInput.x == 0 && movementInput.y == 0 ? new Vector2(facingDirection, movementInput.y) : movementInput;
        }

        #endregion

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            playerCollider = GetComponent<Collider2D>();
            sr = GetComponent<SpriteRenderer>();
            playerInput = GetComponent<PlayerInput>();
            movementInputAction = playerInput.actions.FindAction("Movement", true);

            wallCoyoteTime = wallCoyoteTimer;

            movementInterrupters = 0;
            facingDirection = 1;
            jumpPerformed = false;
            jumpCancelled = false;
            dashPerformed = false;
            isWallJumping = false;
        }
        private void Update()
        {
            movementInput = new Vector2(Utility.NormaliseForJoystickThreshhold(movementInputAction.ReadValue<Vector2>().x),
                                        Utility.NormaliseForJoystickThreshhold(movementInputAction.ReadValue<Vector2>().y));
            isGrounded = CheckGrounded();
            wasRising = isRising;
            isRising = !isGrounded && rb.velocity.y > 0f;
            CheckIfWallSliding();
            ProcessWallSlideEvents();
        }


        private void FixedUpdate()
        {
            // State Calculations

            rb.gravityScale = isDashing ? 0f : isRising ? gravityScaleRising : gravityScaleFalling;

            if (isGrounded)
            {
                SetIsWallJumping(false);
                SetIsWallSliding(false);

                coyoteTimer = 0f;

                jumpsRemaining = jumpsAllowed;
                dashesRemaining = dashesAllowed;
            }
            else
            {
                //Take one jump away from the player if they leave the ground without jumping
                if (jumpsRemaining == jumpsAllowed && coyoteTimer > coyoteTime)
                {
                    jumpsRemaining--;
                }
                coyoteTimer += Time.deltaTime;
            }


            if (isHoldingTowardsWall)
            {
                wallCoyoteTimer = 0f;
            }
            else
            {
                wallCoyoteTimer += Time.deltaTime;
            }

            // Movement
            if (canMove)
            {
                if (dashPerformed)
                {
                    OnDashPerformed();
                }
                else if (jumpPerformed)
                {
                    if (isHoldingTowardsWall || (wallCoyoteTimer < wallCoyoteTime && !isGrounded && !isRising))
                    {
                        isWallJumping = true;
                    }
                    else
                    {
                        OnJumpPerformed();
                    }
                }
                else if (jumpCancelled)
                {
                    jumpCancelled = false;
                    if (isWallJumping)
                    {
                        OnWallJumpCancelled();
                    }
                    else
                    {
                        OnJumpCancelled();
                    }
                }


                if (isWallJumping)
                {
                    ProcessWallJump();
                }

                if (isDashing)
                {
                    ProcessDash();
                }

                // The players x speed should be set unless the player is in the middle of a wall jump or dash
                // isWallJumping and isDashing need to be rechecked as they may be true at the start of the frame but not after processing.
                if (!isWallJumping && !isDashing)
                {
                    wallCoyoteTimer += Time.deltaTime;
                    ProcessMovement();
                }

                if (rb.velocity.y < -maxFallSpeed)
                {
                    rb.velocity = new Vector2(rb.velocity.x, -maxFallSpeed);
                }
            }
        }

        // -------------------------------------------
        // ---------------- OLD CODE -----------------
        // -------------------------------------------


    }
}
