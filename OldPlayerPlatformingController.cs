//using UnityEngine;

//public class OldPlayerPlatformingController : MonoBehaviour
//{
//    private Rigidbody2D rb;
//    private SpriteRenderer sr;
//    private PlayerInputActions playerInputActions;

//    [Header("Jump Tuning")]
//    [SerializeField]
//    [Tooltip("This is the vertical distance the player will travel after releasing the jump button.")]
//    private float dampJumpHeight = 1f;
//    public void SetDampJumpHeight(float height) => dampJumpHeight = height;
//    [SerializeField]
//    private float minJumpHeight = 5f;
//    public void SetMinJumpHeight(float height) => minJumpHeight = height;
//    [SerializeField]
//    private float maxJumpHeight = 10f;
//    public void SetMaxJumpHeight(float height) => maxJumpHeight = height;
//    [SerializeField]
//    private int extraJumps = 0;
//    private int jumpsAvailable = 0;
//    private float lastJumpTimer;
//    private float dampJumpVel, maxJumpVel, dampLimitJumpVel;
//    [SerializeField]
//    private float coyoteTime = 0.2f;
//    private float coyoteTimer = 0f;


//    [Header("Gravity Tuning")]
//    [SerializeField]
//    private float gravityScaleRising = 2f;
//    public void SetGravityScaleRising(float scale) => gravityScaleRising = scale;
//    [SerializeField]
//    private float gravityScaleFalling = 4f;
//    public void SetGravityScaleFalling(float scale) => gravityScaleFalling = scale;

//    [Header("Running")]
//    [SerializeField]
//    private float runSpeed = 2f;
//    public void SetRunSpeed(float speed) => runSpeed = speed;


//    private PlayerState player;
//    private GameData gameData;
//    private bool hasParent;
//    private Transform originalParent;
//    private Rigidbody2D parentRigidBody;

//    [SerializeField]
//    private AudioSource jumpSfx;

//    void Start()
//    {
//        originalParent = this.transform.parent;
//        player = GetComponent<PlayerState>();
//        rb = GetComponent<Rigidbody2D>();
//        rb.gravityScale = gravityScaleRising;

//        gameData = GameObject.FindGameObjectWithTag("GameData").GetComponent<GameDataRef>().Data;
//        sr = GetComponent<SpriteRenderer>();
//    }

//    private void OnEnable()
//    {
//        // Uses the input system and C# events to trigger actions.
//        // C# events must be subscribed to and then unsubscribed from on disable.
//        playerInputActions = new PlayerInputActions();
//        playerInputActions.Player.Enable();
//        playerInputActions.Player.Jump.performed += Jump;
//        playerInputActions.Player.Jump.canceled += JumpDamp;
//        playerInputActions.Player.Movement.performed += StartRun;
//        playerInputActions.Player.Movement.canceled += StopRun;
//    }

//    private void OnDisable()
//    {
//        playerInputActions.Player.Jump.performed -= Jump;
//        playerInputActions.Player.Jump.canceled -= JumpDamp;
//        playerInputActions.Player.Movement.performed -= StartRun;
//        playerInputActions.Player.Movement.canceled -= StopRun;

//    }
//    // Update is called once per frame
//    void Update()
//    {
//        if (hasParent && (player.CurrentGround == null || !player.CurrentGround.CompareTag("MovingGround") || this.gameObject.transform.parent != player.CurrentGround.transform))
//        {
//            parentRigidBody = null;
//            this.gameObject.transform.SetParent(originalParent);
//            hasParent = false;
//        }
//        if (player.IsGrounded)
//        {
//            jumpsAvailable = extraJumps;
//            coyoteTimer = 0f;
//            if (player.CurrentGround != null && player.CurrentGround.CompareTag("MovingGround") && this.gameObject.transform.parent != player.CurrentGround.transform)
//            {
//                parentRigidBody = player.CurrentGround.GetComponent<Rigidbody2D>();
//                this.gameObject.transform.SetParent(player.CurrentGround.transform, true);
//                hasParent = true;
//            }
//        }
//        else
//        {
//            coyoteTimer += Time.deltaTime;
//        }
//        lastJumpTimer += Time.deltaTime;
//    }
//    private void FixedUpdate()
//    {
//        Move();
//        rb.gravityScale = (player.IsFalling) ? gravityScaleFalling : gravityScaleRising;
//    }

//    private void Jump(InputAction.CallbackContext context)
//    {
//        if (!player.IsBreathing && (player.IsGrounded || jumpsAvailable > 0 || coyoteTimer < coyoteTime) && player.MovementEnabled && gameData.PlayerCanMove && lastJumpTimer > 0.1f)
//        {
//            jumpSfx.pitch = Mathf.Lerp(1.5f, 0.8f, player.Weight);
//            jumpSfx.Play();
//            jumpsAvailable--;
//            lastJumpTimer = 0f;
//            dampJumpVel = PhysicsUtility.HeightToVelocity(dampJumpHeight, Physics2D.gravity.y, gravityScaleRising);
//            maxJumpVel = PhysicsUtility.HeightToVelocity(maxJumpHeight, Physics2D.gravity.y, gravityScaleRising);
//            dampLimitJumpVel = PhysicsUtility.HeightToVelocity(maxJumpHeight - minJumpHeight + dampJumpHeight, Physics2D.gravity.y, gravityScaleRising);
//            rb.gravityScale = gravityScaleRising;
//            rb.velocity = new Vector2(rb.velocity.x, maxJumpVel);
//        }

//    }
//    private void JumpDamp(InputAction.CallbackContext context)
//    {
//        if (rb.velocity.y > dampLimitJumpVel)
//        {
//            float currentHeight = PhysicsUtility.VelocityChangeToHeight(maxJumpVel, rb.velocity.y, Physics2D.gravity.y, gravityScaleRising);
//            rb.velocity = new Vector2(rb.velocity.x, PhysicsUtility.HeightToVelocity(minJumpHeight - currentHeight, Physics2D.gravity.y, gravityScaleRising));
//        }
//        else if (rb.velocity.y > dampJumpVel)
//        {
//            rb.velocity = new Vector2(rb.velocity.x, dampJumpVel);
//        }
//    }

//    private void Move()
//    {
//        if (!player.IsBreathing && player.MovementEnabled && gameData.PlayerCanMove)
//        {
//            float movementInput = playerInputActions.Player.Movement.ReadValue<float>();
//            if (movementInput != 0)
//            {
//                sr.flipX = movementInput < 0;
//            }
//            rb.velocity = new Vector2(runSpeed * movementInput + (parentRigidBody != null ? parentRigidBody.velocity.x : 0f), rb.velocity.y);
//        }
//        else
//        {
//            rb.velocity = new Vector2(0f, rb.velocity.y);
//        }
//    }

//    private void StartRun(InputAction.CallbackContext context)
//    {

//    }
//    private void StopRun(InputAction.CallbackContext context)
//    {
//    }
//}

