using UnityEngine;

namespace PlatformingScripts
{
    public abstract class JumpCondition : MonoBehaviour
    {
        private PlayerState player;

        // Use this for initialization
        public void Start()
        {
            player = GetComponent<PlayerState>();
            player.AddJumpCondition(this);
        }

        public abstract bool CanJump();
    }
}