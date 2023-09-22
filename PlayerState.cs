using UnityEngine;
using UnityEngine.InputSystem;

namespace PlatformingScripts
{
    public class PlayerState : MonoBehaviour
    {
        private Rigidbody2D rb;

        private GroundCheck groundCheck;
        private bool isGrounded = false;
        public bool IsGrounded => isGrounded;

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
            isGrounded = groundCheck.CheckGrounded();
            isMoving = (Mathf.Abs(movementInput.ReadValue<Vector2>().x) > 0.1) && (rb.velocity.x != 0);
            isRising = !isGrounded && rb.velocity.y > 0;
        }

    }
}