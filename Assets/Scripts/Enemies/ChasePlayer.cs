using UnityEngine;

public class ChasePlayer : MonoBehaviour
{
    [SerializeField]
    private Transform player;
    [SerializeField]
    private float fieldOfView = 20f;
    [SerializeField]
    private float moveSpeed;
    [SerializeField]
    private float groundDrag;
    [SerializeField]
    private LayerMask whatToSee;

    private Rigidbody rb;
    public RaycastHit sight;
    private bool seeingPlayer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearDamping = groundDrag;

        // Physics.gravity *= 2;
    }

    void FixedUpdate() {
        Move();
    }

    void Update()
    {
        SeeForPlayer();

        SpeedControl();
    }

    void SeeForPlayer() {
        if(Physics.Raycast(transform.position, player.position - transform.position, out sight, fieldOfView, whatToSee)) {
            if(sight.transform == player) {
                seeingPlayer = true;
            } else {
                seeingPlayer = false;
            }
        } else {
            seeingPlayer = false;
        }

        if(seeingPlayer) {
            Debug.DrawRay(transform.position, player.position - transform.position, Color.green, 1f);
        } else {
            Debug.DrawRay(transform.position, player.position - transform.position, Color.red, 1f);
        }
    }

    void Move() {
        rb.AddForce((player.position - transform.position).normalized * moveSpeed * 10f, ForceMode.Force);
    }

    void SpeedControl() {
        if(rb.linearVelocity.magnitude > moveSpeed) {
            rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }
    }
}
