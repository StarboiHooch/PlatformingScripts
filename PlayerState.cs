﻿using UnityEngine;
using UnityEngine.InputSystem;

namespace PlatformingScripts
{
    public class PlayerState : MonoBehaviour
    {
        private Rigidbody2D rb;

        private GroundCheck groundCheck;
        private bool isGrounded = false;
        public bool IsGrounded => isGrounded;
        private Collider2D currentGround;
        public Collider2D CurrentGround => currentGround;

        public bool isMoving = false;
        public bool isRising = false;
        public bool isPreDashing = false;
        public bool isDashing = false;
        private PlayerInput playerInput;
        private InputAction movementInput;

        [SerializeField]
        private bool jumpEnabled = true;
        public bool canJump => jumpEnabled;
        [SerializeField]
        private bool dashEnabled = true;
        public void SetDashEnabled(bool enabled)
        {
            dashEnabled = enabled;
        }
        public bool canDash => dashEnabled;
        // Use this for initialization  
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            groundCheck = GetComponent<GroundCheck>();
            playerInput = GetComponent<PlayerInput>();
            movementInput = playerInput.actions.FindAction("Movement", true);
        }

        // Update is called once per frame
        void Update()
        {
            currentGround = groundCheck.GetGroundCollider();
            isGrounded = currentGround != null;
            //isMoving = (Mathf.Abs(movementInput.ReadValue<Vector2>().x) > 0.1) && (rb.velocity.x != 0);
            isMoving = (Mathf.Abs(rb.velocity.x) > 0.1);
            isRising = !isGrounded && rb.velocity.y > 0;
        }
    }
}