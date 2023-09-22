using Assets.Modules.GameJamHelpers.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlatformingScripts
{
    [RequireComponent(typeof(GroundCheck), typeof(PlayerState), typeof(PlayerInput))]
    public class PlatformingController : MonoBehaviour
    {
        private Rigidbody2D rb;
        private SpriteRenderer sr;
        private PlayerInput playerInput;
        private InputAction movementInput;
        private PlayerState player;

        [Header("Running")]
        [SerializeField]
        private float runSpeed = 2f;
        [SerializeField]
        [Tooltip("Should the character sprite flip when running in the opposite direction")]
        private bool flipSprite = true;
        [SerializeField]
        [Tooltip("If your animations are drawn facing left then set this to false")]
        private bool spritesFaceRight = true;

        [Header("Jumping")]
        [SerializeField]
        private float minJumpHeight = 5f;
        public void SetMinJumpHeight(float height) => minJumpHeight = height;
        [SerializeField]
        private float maxJumpHeight = 10f;
        private float maxJumpVel => PhysicsUtility.HeightToVelocity(maxJumpHeight, Physics2D.gravity.y, gravityScaleRising);
        public void SetMaxJumpHeight(float height) => maxJumpHeight = height;
        [SerializeField]
        [Tooltip("This is the vertical distance the player will travel after releasing the jump button.")]
        private float dampJumpHeight = 1f;
        private float dampJumpVel => PhysicsUtility.HeightToVelocity(dampJumpHeight, Physics2D.gravity.y, gravityScaleRising);
        public void SetDampJumpHeight(float height) => dampJumpHeight = height;

        private float dampLimitJumpVel => PhysicsUtility.HeightToVelocity(maxJumpHeight - minJumpHeight + dampJumpHeight, Physics2D.gravity.y, gravityScaleRising);

        [SerializeField]
        private float coyoteTime = 0.1f;
        private float coyoteTimer = 0f;
        [SerializeField]
        private int jumpsAllowed = 1;
        private int remainingJumps = 0;



        [Header("Gravity Tuning")]
        [SerializeField]
        private float gravityScaleRising = 2f;
        public void SetGravityScaleRising(float scale) => gravityScaleRising = scale;
        [SerializeField]
        private float gravityScaleFalling = 4f;
        public void SetGravityScaleFalling(float scale) => gravityScaleFalling = scale;

        public event EventHandler<EventArgs> PlayerJumped;

        [Header("Dash")]
        [SerializeField]
        private bool dashEnabled = true;
        [SerializeField]
        private float dashDistance = 5f;
        [SerializeField]
        private float dashTime = 0.5f;
        [SerializeField]
        private float dashStartLag = 0.05f;
        [SerializeField]
        private float dashEndLag = 0.05f;

        [SerializeField]
        private PrefabEmitter dashTrail;

        private Vector2 dashDirection;
        private float dashTimer = 0f;
        private float facingDirection = 1f;


        // Use this for initialization
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();
            player = GetComponent<PlayerState>();
            playerInput = GetComponent<PlayerInput>();
            movementInput = playerInput.actions.FindAction("Movement", true);
        }

        // Update is called once per frame
        void Update()
        {
            if (player.IsGrounded)
            {
                remainingJumps = jumpsAllowed;
                coyoteTimer = 0f;
            }
            else
            {
                if (remainingJumps == jumpsAllowed && coyoteTimer > coyoteTime)
                {
                    remainingJumps--;
                }
                coyoteTimer += Time.deltaTime;
            }

            if (player.isDashing)
            {
                dashTimer += Time.deltaTime;
                if (player.isPreDashing && dashTimer >= dashStartLag)
                {
                    dashDirection = GetDashDir();
                    player.isPreDashing = false;
                }
                if (dashTimer >= dashStartLag + dashTime + dashEndLag)
                {
                    if (dashTrail != null)
                    {
                        dashTrail.SetActive(false);
                    }
                    rb.velocity = Vector2.zero;
                    player.isDashing = false;
                    dashTimer = 0;
                }
            }

            rb.gravityScale = player.isDashing ? 0f : player.isRising ? gravityScaleRising : gravityScaleFalling;

            if (player.isDashing)
            {
                Dash();
            }
            else
            {
                Move();
            }
        }

        public void OnDash(InputAction.CallbackContext context)
        {
            if (context.performed) { OnDashPerformed(); }

        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed) { OnJumpPerformed(); }
            if (context.canceled) { OnJumpCancelled(); }

        }

        private void Dash()
        {
            if (player.isPreDashing)
            {
                rb.velocity = Vector2.zero;
                dashDirection = GetDashDir();
            }
            else if (dashTimer <= dashStartLag + dashTime)
            {
                if (dashTrail != null)
                {
                    dashTrail.SetActive(true);
                }
                rb.velocity = dashDirection.normalized * (dashDistance / dashTime);
            }
            else
            {
                if (dashTrail != null)
                {
                    dashTrail.SetActive(false);
                }
                rb.velocity = Vector2.zero;
            }
        }

        private void Move()
        {
            float directionX = Utility.NormaliseForJoystickThreshhold(movementInput.ReadValue<Vector2>().x);
            if (directionX != 0)
            {
                facingDirection = directionX;
                sr.flipX = (spritesFaceRight ? directionX < 0 : directionX > 0) && flipSprite;
            }
            rb.velocity = new Vector2(runSpeed * directionX, rb.velocity.y);
        }

        private void OnJumpPerformed()
        {
            if (
                player.canJump &&
                ((remainingJumps < jumpsAllowed && remainingJumps > 0) ||
                (remainingJumps == jumpsAllowed && remainingJumps > 0 && (player.IsGrounded || coyoteTimer < coyoteTime)))
            )
            {
                remainingJumps--;
                rb.gravityScale = gravityScaleRising;
                rb.velocity = new Vector2(rb.velocity.x, maxJumpVel);
                PlayerJumped?.Invoke(this, EventArgs.Empty);
            }

        }
        private void OnJumpCancelled()
        {
            if (rb.velocity.y > dampLimitJumpVel)
            {
                float currentHeight = PhysicsUtility.VelocityChangeToHeight(maxJumpVel, rb.velocity.y, Physics2D.gravity.y, gravityScaleRising);
                rb.velocity = new Vector2(rb.velocity.x, PhysicsUtility.HeightToVelocity(minJumpHeight - currentHeight, Physics2D.gravity.y, gravityScaleRising));
            }
            else if (rb.velocity.y > dampJumpVel)
            {
                rb.velocity = new Vector2(rb.velocity.x, dampJumpVel);
            }
        }
        private void OnDashPerformed()
        {
            if (dashEnabled && player.canDash)
            {
                player.isDashing = true;
                player.isPreDashing = true;
                dashDirection.y = 0;
                // PlayerDashed?.Invoke(this, EventArgs.Empty);
            }

        }

        private Vector2 GetDashDir()
        {
            float inputX = Utility.NormaliseForJoystickThreshhold(movementInput.ReadValue<Vector2>().x);
            float inputY = Utility.NormaliseForJoystickThreshhold(movementInput.ReadValue<Vector2>().y);
            if (inputX != 0)
            {
                facingDirection = inputX;
            }
            return inputX == 0 && inputY == 0 ? new Vector2(facingDirection, dashDirection.y) : new Vector2(inputX, inputY);
        }
    }
}
