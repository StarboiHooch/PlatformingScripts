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
        public DefaultPlatformerInputActions playerInputActions;
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
        private bool isPreppingDash = false;
        private Vector2 dashDirection;
        private float dashTimer = 0f;
        private float facingDirection = 1f;


        // Use this for initialization
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();
            player = GetComponent<PlayerState>();
        }

        private void OnEnable()
        {
            playerInputActions = new DefaultPlatformerInputActions();
            playerInputActions.Player.Enable();
            playerInputActions.Player.Jump.performed += Jump;
            playerInputActions.Player.Jump.canceled += JumpDamp;
            playerInputActions.Player.Dash.performed += DashPerformed;
        }

        private void OnDisable()
        {
            playerInputActions.Player.Jump.performed += Jump;
            playerInputActions.Player.Jump.canceled += JumpDamp;
            playerInputActions.Player.Disable();
        }

        // Update is called once per frame
        void Update()
        {
            if (player.isDashing)
            {
                dashTimer += Time.deltaTime;
                if (isPreppingDash && dashTimer >= dashStartLag)
                {
                    dashDirection = GetDashDir();
                    isPreppingDash = false;
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

        private void Dash()
        {
            if (isPreppingDash)
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

        private void FixedUpdate()
        {

        }
        private void Move()
        {
            float movementInput = playerInputActions.Player.Move.ReadValue<float>();
            if (movementInput != 0)
            {
                facingDirection = movementInput;
                sr.flipX = (spritesFaceRight ? movementInput < 0 : movementInput > 0) && flipSprite;
            }
            rb.velocity = new Vector2(runSpeed * movementInput, rb.velocity.y);
        }

        private void Jump(InputAction.CallbackContext context)
        {
            if (player.canJump)
            {
                rb.gravityScale = gravityScaleRising;
                rb.velocity = new Vector2(rb.velocity.x, maxJumpVel);
                PlayerJumped?.Invoke(this, EventArgs.Empty);
            }

        }
        private void JumpDamp(InputAction.CallbackContext context)
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
        private void DashPerformed(InputAction.CallbackContext context)
        {
            if (dashEnabled && player.canDash)
            {
                player.isDashing = true;
                isPreppingDash = true;
                dashDirection.y = 0;
                // PlayerDashed?.Invoke(this, EventArgs.Empty);
            }

        }

        private Vector2 GetDashDir()
        {
            float inputX = playerInputActions.Player.Move.ReadValue<float>();
            float inputY = playerInputActions.Player.YDirection.ReadValue<float>();
            if (inputX != 0)
            {
                facingDirection = inputX;
            }
            return inputX == 0 && inputY == 0 ? new Vector2(facingDirection, dashDirection.y) : new Vector2(inputX, inputY);
        }
    }
}
