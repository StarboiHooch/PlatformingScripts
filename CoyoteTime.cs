using PlatformingScripts;
using UnityEngine;

namespace Assets.Modules.PlatformingScripts
{
    public class CoyoteTime : JumpCondition
    {
        [SerializeField]
        private float coyoteTime = 0.1f;

        private float timer = 0f;

        public override bool CanJump()
        {
            return timer <= coyoteTime;
        }


        // Update is called once per frame
        void Update()
        {
            if (!player.isGrounded)
            {
                timer += Time.deltaTime;
            }
            else
            {
                timer = 0f;
            }
        }
    }
}