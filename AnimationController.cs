﻿using PlatformingScripts;
using UnityEngine;

namespace Assets.Scripts.PlatformingScripts
{
    public class AnimationController : MonoBehaviour
    {
        private PlayerState player;
        private Animator anim;
        // Use this for initialization
        void Start()
        {
            player = GetComponent<PlayerState>();
            anim = GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void FixedUpdate()
        {
            if (player.isPreDashing)
            {
                anim.Play("PreDash");
            }
            else if (player.IsGrounded && player.isMoving)
            {
                anim.Play("Run");
            }
            else if (!player.IsGrounded && player.isRising)
            {
                anim.Play("Rise");
            }
            else if (!player.IsGrounded && !player.isRising)
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