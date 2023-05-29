using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlatformingScripts
{
    public class PlayerState : MonoBehaviour
    {
        private GroundCheck groundCheck;
        public bool isGrounded = false;

        private List<JumpCondition> jumpConditions = new List<JumpCondition>();
        public void AddJumpCondition(JumpCondition condition) => jumpConditions.Add(condition);
        public bool canJump => jumpConditions.Any(condition => condition.CanJump());
        // Use this for initialization  
        void Start()
        {
            groundCheck = GetComponent<GroundCheck>();
        }

        // Update is called once per frame
        void Update()
        {
            isGrounded = groundCheck.CheckGrounded();
        }

    }
}