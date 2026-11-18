using UnityEngine;


[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : SneedSpace {

    //:::::::://
    // FIELDS //
    //:::::::://

    #region fields

    [Space(20)]
    [Header("Ground / Air Speeds")]
    public float maxVelGround = 6.0f;
    public float maxVelAir = 0.6f;
    public float maxAccel = 60.0f; // 10*maxVelocityGround
    public float stopSpeed = 1.5f;
    public float friction = 4.0f; // the 4 multiplier in drop 
    public float gravity = (3f) * 15.34f; // GRAVITY, to be subtracted from

    [Space]
    [Header("Jump / Sprint / Crouch / Collisions")]
    public float jumpHeight = 0.85f;
    public float sprintMult = 12f;
    public float crouchHeightMult = 0.5f;
    public float crouchSpeedMult = 0.5f;
    public float PUSHFORCE = 1.0f;

    [Space]
    [Header("Ground Check")]
    [SerializeField] private CharacterController charController;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundCheckStartOffset = 0.1f;
    [SerializeField] private float groundCheckRadiusMultiplier = 0.9f;
    [SerializeField] private float groundCheckExtraDistance = 0.1f;

    [Space]
    [Header("Footsteps")]
    [SerializeField] private PlayerAudioManager playerAudio;
    [SerializeField] private float stepDistance=1.8f;


    /// <summary>
    /// The current world-direction being moved towards by the player. <br />
    /// The value is the normalized vector <tt><b>(x, y, z)</b></tt> where <br />
    /// <tt><b>x = -1</b></tt> is full leftward movement and <tt><b>x = 1</b></tt> is full rightward movement <br />
    /// <tt><b>z = -1</b></tt> is full backward movement and <tt><b>z = 1</b></tt> is full forward movement
    /// </summary>
    private Vector3 wishDir;
    private Vector3 velocity;
    private Vector3 moveDirWorld;
    private Vector3 inputDir;
    private Vector3  lastPosition;
    private Vector3 standCenter;
    private Vector3 crouchCenter;

    private float distanceSinceLastStep;
    private float standHeight;
    private float crouchHeight;

    private bool wasMoving;
    private bool wishJump, wishSprint;
    private bool wasGrounded;
    private bool crouchToggle;
    private bool justLanded;

    private PlayerControls config;
    private PlayerConfig controls;


    public Vector3 Velocity => velocity;
    public Vector3 myPosition => transform.position;
    public Vector2 MoveInput { get; private set; } // world space velocity
    public bool IsMoving => MoveInput.sqrMagnitude > 0.01f;
    public bool JustLanded => justLanded;
    public bool isGrounded { get; private set; }
    public bool isSprinting { get; private set; }
    public bool isCrouching { get; private set; }

    #endregion


    //:::::::::::::::::::://
    // main movement loop //
    //:::::::::::::::::::://

    #region update/start/awake

    private void Awake() {
        charController = GetComponent<CharacterController>();
        config = GetComponentInParent<PlayerControls>();
        controls = config.Controls;
    }


    void Start() {
        // charcontroller
        standHeight = charController.height;
        crouchHeight = standHeight * crouchHeightMult;
        crouchCenter = standCenter - Vector3.up*((standHeight-crouchHeight)*0.5f); // player feet closer to the same place when crouching
        crouchToggle = false;

        // movement
        maxAccel = 10f * maxVelGround;
        moveDirWorld = Vector3.zero; // reset world move direction
        inputDir = Vector2.zero;     // reset input direction

        // initialize tracking with current position
        lastPosition = transform.position;
        distanceSinceLastStep = 0f;
    }


    void Update() {
        PollInput();
        ApplyMovement();
    }

    #endregion


    //=======//
    // Input //
    //=======//

    #region input

    /// <summary>
    /// 
    /// </summary>
    private void PollInput() {
        if (charController.isGrounded && velocity.y < 0) 
            velocity.y = -2f; // reset gravity when grounded

        wasGrounded = charController.isGrounded || GroundCheck();
        justLanded = false;

        // Get WASD 
        moveDirWorld = Vector3.zero;
        inputDir = Vector2.zero;
        GetInput();

        // Crouch
        //::@TODO:: fix crouch behaviour to include camera etc, and uncrouch smooth movement
        if(Input.GetKeyDown(controls.crouchKey)) {
            crouchToggle = !crouchToggle;
            PrintMessage($"{crouchToggle}");
        }

        isCrouching = crouchToggle;
        charController.height = isCrouching ? crouchHeight : standHeight;
        //charController.center = isCrouching ? crouchCenter : standCenter;

        wishSprint =
            Input.GetKey(KeyCode.LeftShift) &&
            MoveInput.y > 0.1f &&
            !isCrouching;

        isSprinting =
            wishSprint &&
            wasGrounded;

        // Jump
        //::@TODO:: listen for SPACEBAR HOLD MOUNT EDGE
        if (wasGrounded && Input.GetButtonDown("Jump")) {
            wishJump = true;
        } else {
            if (!wasGrounded) 
                wishJump = false; // omit jump input while in air
        }

        // Sprint
        if (isSprinting && !wasGrounded) 
            wishSprint = false;
    }


    /// <summary>
    /// Applies input actions used for determining current world-direction movement
    /// </summary>
    private void GetInput() {
        // FORWARD (+y)
        if (Input.GetKey(KeyCode.W)) {
            moveDirWorld += transform.forward;
            inputDir.y += 1f;                       
        }
        // BACKWARD (-y)
        if (Input.GetKey(KeyCode.S)) {
            moveDirWorld -= transform.forward;
            inputDir.y -= 1f;                       
        }
        // LEFT (-x)
        if (Input.GetKey(KeyCode.A)) {
            moveDirWorld -= transform.right;
            inputDir.x -= 1f;                       
        }
        // RIGHT (+x)
        if (Input.GetKey(KeyCode.D)) {
            moveDirWorld += transform.right;
            inputDir.x += 1f;                       
        }

        MoveInput = inputDir.sqrMagnitude > 1f ? inputDir.normalized : inputDir; // for other scripts
        wishDir = moveDirWorld.normalized;
    }


    #endregion


    //==========//
    // Movement //
    //==========//

    #region movement

    /// <summary>
    /// Main movement method, polls movement states <tt><b>(wasGrounded, wishJump, justLanded)</b></tt>
    /// then updates Air and Ground Velocity, moves the Character Controller, plays footstep sounds
    /// </summary>
    private void ApplyMovement() {
        float dt = Time.deltaTime;

        if (wasGrounded) {
            if (wishJump) {
                velocity.y = Mathf.Sqrt(2f * gravity * jumpHeight); // change y velocity
                velocity = UpdateVelocityAir(wishDir, dt); // horizontal update uses AIR behaviour
                wishJump = false;
            } else {
                velocity = UpdateVelocityGround(wishDir, dt);
            }
        } else {
            velocity.y -= gravity * dt; // apply gravity (downwards)
            velocity = UpdateVelocityAir(wishDir, dt); // air horizontal acceleration
        }

        charController.Move(velocity * dt);

        // Landed
        //::@TODO:: more post-jump behaviour
        if (charController.isGrounded && !wasGrounded) {
            PrintMessage("JUST LANDED");
            justLanded = true;
            //OnLanding();
        }

        HandleFootsteps();
    }


    /// <summary>
    /// Updates air movement w/ same accelerate, but using air max velocity.
    /// </summary>
    /// <returns>Updated air velocity vector.</returns>
    private Vector3 UpdateVelocityAir(Vector3 wishDir, float delta) {
        float maxSpeed = RaiseSpeed(maxVelAir);
        return Accelerate(wishDir, maxSpeed, delta);
    }


    /// <summary>
    /// Updates ground movement w/ friction + accelerate toward wishDir at ground max velocity
    /// </summary>
    /// <returns>Gound velocity vector.</returns>
    private Vector3 UpdateVelocityGround(Vector3 wishDir, float delta) {
        float speed = velocity.magnitude; // friction
         
        // drop = control * 4 * delta  (the "4" is friction strength)
        if (speed > 0f) {
            float control = Mathf.Max(stopSpeed, speed);
            float drop = control * friction * delta;
            float newSpeed = Mathf.Max(speed - drop, 0f);

            if(newSpeed != speed) {
                velocity *= newSpeed / speed;
            }
        }

        // raises max allowed speed
        float maxSpeed = RaiseSpeed(maxVelGround);

        // then accelerate along wish direction up to ground max
        return Accelerate(wishDir, maxSpeed, delta);
    }


    /// <summary>
    /// Raises maximum allowed speed by a modifier, accounting for crouch/sprint.
    /// </summary>
    /// <returns>New maximum allowed speed.</returns>
    private float RaiseSpeed(float initialMaxSpeed) {
        float maxSpeed = initialMaxSpeed;

        if (isCrouching) {
            maxSpeed *= crouchSpeedMult;
        } else if (isSprinting) {
            maxSpeed *= sprintMult;
        }

        return maxSpeed;
    }


    /// <summary>
    /// Accelerates velocity in accordance to wishDir.
    /// </summary>
    /// <returns>The vector sum of Velocity and speed along the wishDir.</returns>
    private Vector3 Accelerate(Vector3 wishDir, float maxVelocity, float delta) {
        if (wishDir.sqrMagnitude < 1e-6f) return velocity;    // no input
        float currentSpeed = Vector3.Dot(velocity, wishDir);  // speed along wishdir

        // how much added speed along wish direction
        float addSpeed = 
            Mathf.Clamp(maxVelocity-currentSpeed, 0f, maxAccel * delta);

        // new velocity is old + extra along wishDir
        return velocity + addSpeed * wishDir;

    }


    /// <summary>
    /// Tracks distance traveled to trigger footstep audio.
    /// </summary>
    private void HandleFootsteps() {
        // reset movement tracking
        if (!charController.isGrounded) {
            wasMoving = false;
            lastPosition = transform.position;
            return;
        }

        bool movementKeyPressed = 
            Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.D);

        // determine if player is currently giving movement input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        bool isMoving = 
            Mathf.Abs(x) > 0.02f || 
            Mathf.Abs(z) > 0.02f;

        Vector3 stepVector = velocity;
        stepVector.y = 0f;
        float stepDist = stepVector.magnitude/2f;


        // play initial footstep immediately
        if (isMoving && !wasMoving) {
            playerAudio?.StartPlayback();
            distanceSinceLastStep = 0f;
        }

        // accumulate distance and play footsteps at intervals while moving
        if (isMoving) {
            float delta = Vector3.Distance(transform.position, lastPosition);
            distanceSinceLastStep += delta;

            // trigger next footstep if enough distance is covered
            if (distanceSinceLastStep >= stepDist && movementKeyPressed) {
                distanceSinceLastStep -= stepDist;
                playerAudio?.StartPlayback();
            }
        }

        // update state for the next frame
        wasMoving = isMoving;
        lastPosition = transform.position;
    }


    /// <summary>
    /// Peforms secondary ground-check, provides a slightly more customizable ground-check
    /// </summary>
    /// <returns>if the spherecast is overlapping with groundMask</returns>
    private bool GroundCheck() {
        float sphereRadius = charController.radius * groundCheckRadiusMultiplier;
        float castDistance = charController.skinWidth + groundCheckExtraDistance;
        Vector3 origin = transform.position + Vector3.up * groundCheckStartOffset;

        return Physics.SphereCast(
            origin,
            sphereRadius,
            Vector3.down,
            out RaycastHit hit,
            castDistance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
    }


    #endregion



    void OnControllerColliderHit(ControllerColliderHit hit) {
        var bodyHit = hit.rigidbody;
        if (bodyHit == null || bodyHit.isKinematic) return;
        if (hit.normal.y > 0.5f) return;

        Vector3 horizontalDir = 
            new Vector3( 
                hit.moveDirection.x, 0f, hit.moveDirection.z 
            );

        // force too small
        if(horizontalDir.sqrMagnitude < 1e-6f) 
            return; 

        horizontalDir.Normalize();

        Vector3 push = 
            horizontalDir * 
            PUSHFORCE * 
            Mathf.Max(0.2f, charController.velocity.magnitude);

        bodyHit.linearVelocity = 
            new Vector3(
                push.x, bodyHit.linearVelocity.y, push.z
            );

    }


    private void OnDrawGizmos() {
        float sphereRadius = charController.radius * groundCheckRadiusMultiplier;
        float castDistance = charController.skinWidth + groundCheckExtraDistance;
        Vector3 origin = transform.position + Vector3.up * groundCheckStartOffset;
        Vector3 end = origin + Vector3.down * castDistance;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, sphereRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(end, sphereRadius);
        Gizmos.color = Color.white;
        Gizmos.DrawLine(origin, end);
    }


}
