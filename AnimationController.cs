using PlatformingScripts;
using UnityEngine;

namespace Assets.Scripts.PlatformingScripts
{
    public class AnimationController : MonoBehaviour
    {
        private PlatformingController player;
        private Animator anim;
        // Use this for initialization
        void Start()
        {
            player = GetComponent<PlatformingController>();
            anim = GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void FixedUpdate()
        {
            if (player.IsPreDashing)
            {
                anim.Play("PreDash");
            }
            else if (player.IsGrounded && player.IsMoving)
            {
                anim.Play("Run");
            }
            else if (!player.IsGrounded && player.IsRising)
            {
                anim.Play("Rise");
            }
            else if (!player.IsGrounded && !player.IsRising)
            {
                anim.Play("Fall");
            }
            else
            {
                anim.Play("Idle");
            }
        }
    }
}