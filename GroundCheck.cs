using UnityEngine;

namespace PlatformingScripts
{
    public class GroundCheck : MonoBehaviour
    {
        private Collider2D playerCollider;
        [SerializeField]
        private float padding = 0.1f;
        [SerializeField]
        private LayerMask whatIsGround;

        private void Start()
        {
            playerCollider = GetComponent<Collider2D>();
        }

        public bool CheckGrounded()
        {
            return GetGroundCollider() != null;
        }

        public Collider2D GetGroundCollider()
        {
            RaycastHit2D raycastHit = Physics2D.BoxCast(playerCollider.bounds.center, playerCollider.bounds.size, 0f, Vector2.down, padding, whatIsGround);
            EventTrigger_OnPlayerGround groundTrigger = raycastHit.collider?.gameObject.GetComponent<EventTrigger_OnPlayerGround>();
            if (groundTrigger != null)
            {
                groundTrigger.Activate();
            }
            Color rayColor;
            rayColor = (raycastHit.collider != null) ? Color.green : Color.red;
            Debug.DrawRay(playerCollider.bounds.center - new Vector3(playerCollider.bounds.extents.x, playerCollider.bounds.extents.y + padding), Vector2.right * (playerCollider.bounds.extents.x), rayColor);
            return raycastHit.collider;
        }

    }
}