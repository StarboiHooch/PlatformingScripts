using UnityEngine;

namespace PlatformingScripts
{
    public abstract class JumpCondition : MonoBehaviour
    {
        protected PlayerState player;

        // Use this for initialization
        public void Start()
        {
            player = GetComponent<PlayerState>();
            player.AddJumpCondition(this);
        }

        public abstract bool CanJump();
    }
}