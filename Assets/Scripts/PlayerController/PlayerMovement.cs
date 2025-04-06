using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private float walkSpeed;
    [SerializeField]
    private float sprintSpeed, groundDrag;
    private float moveSpeed;

    [Header("Jumping")]
    [SerializeField]
    private float jumpForce;
    [SerializeField]
    private float jumpCooldown, airMultiplier;

    [Header("Crouching")]
    [SerializeField]
    private float crouchYScale;
    [SerializeField] 
    private float crouchSpeed;
    private float startYScale;

    [Header("Slope Handling")]
    [SerializeField]
    private float maxSlopeAngle;
    public RaycastHit slopeHit;
    private bool exitingSlope;

    [SerializeField]
    private float gravityMultiplier = 1;


    [SerializeField]
    private float playerHeight;
    [SerializeField]
    private LayerMask whatIsGround;
    private bool grounded;

    [SerializeField]
    private Transform orientation;

    private Rigidbody rb;
    private Vector3 moveDirection;

    private float horizontalInput;
    private float verticalInput;
    private bool readyToJump = true;

    private MovementState state;

    [SerializeField]
    private enum MovementState {
        walking,
        sprinting,
        crouching,
        air
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        Physics.gravity *= gravityMultiplier;

        startYScale = transform.localScale.y;
    }

    void FixedUpdate() {
        MovePlayer();
    }
    
    void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        Debug.DrawRay(transform.position, Vector3.down, Color.red, playerHeight * 0.5f + 0.2f);

        MyInput();
        SpeedControl();
        StateHandler();

        // handle drag
        if(OnSlope())
            rb.linearDamping = groundDrag * 2f;
        else if(grounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }

    private void MyInput() {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if(Input.GetKey(KeyCode.Space) && readyToJump && grounded) {
            readyToJump = false;

            Jump();

            Invoke("ResetJump", jumpCooldown);
        }

        // when to crouch
        if(Input.GetKeyDown(KeyCode.LeftControl)) {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        // stop crouching
        if(Input.GetKeyUp(KeyCode.LeftControl)) {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void StateHandler() {
        // Mode - crouching
        if(Input.GetKey(KeyCode.LeftControl)) {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }
        // Mode - Sprinting
        if(grounded && Input.GetKey(KeyCode.LeftShift)) {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }

        // Mode - walking
        else if(grounded) {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }

        // Mode - air
        else {
            state = MovementState.air;
        }
    }

    private void MovePlayer() {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // on slope
        if(OnSlope() && !exitingSlope) {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 10f, ForceMode.Force);

            if(rb.linearVelocity.y > 0) {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        if(grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else if(!grounded) {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        rb.useGravity = !OnSlope();
    }

    private void SpeedControl() {
        // limitng speed on slope
        if(OnSlope() && !exitingSlope) {
            if(rb.linearVelocity.magnitude > moveSpeed) {
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
            }
        }

        // limiting on ground or air
        else {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

            if(flatVel.magnitude > moveSpeed) {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
    }

    private void Jump() {
        exitingSlope = true;

        // reset y velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
    
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump() {
        readyToJump = true;
    
        exitingSlope = false;
    }

    private bool OnSlope() {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f)) {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection() {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
}
