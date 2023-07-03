using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlatformingScripts
{
    public class PlayerState : MonoBehaviour
    {
        private Rigidbody2D rb;
        private GroundCheck groundCheck;
        public bool isGrounded = false;
        public bool isMoving = false;
        public bool isRising = false;
        public bool isDashing = false;
        private DefaultPlatformerInputActions InputActions;

        private List<JumpCondition> jumpConditions = new List<JumpCondition>();
        public void AddJumpCondition(JumpCondition condition) => jumpConditions.Add(condition);
        public bool canJump => jumpConditions.Any(condition => condition.CanJump());
        public bool canDash => true;
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