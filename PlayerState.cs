using UnityEngine;

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
        private DefaultPlatformerInputActions InputActions;


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
            InputActions = GetComponent<PlatformingController>().playerInputActions;
        }

        // Update is called once per frame
        void Update()
        {
            isGrounded = groundCheck.CheckGrounded();
            isMoving = (InputActions.Player.Move.ReadValue<float>() != 0) && (rb.velocity.x != 0);
            isRising = !isGrounded && rb.velocity.y > 0;
        }

    }
}