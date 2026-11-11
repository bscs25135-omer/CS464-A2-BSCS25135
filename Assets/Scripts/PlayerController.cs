using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] float speed = 5f;

    [Header("Jump")]
    [SerializeField] float jumpHeight = 1.8f;
    [SerializeField] float fallMultiplier = 2.2f;
    [SerializeField] float coyoteTime = 0.1f;
    [SerializeField] float jumpBuffer = 0.12f;

    [Header("Ground check")]
    [SerializeField] Transform feet;
    [SerializeField] float checkRadius = 0.3f;
    [SerializeField] LayerMask groundMask;

    [Header("Landing")]
    [SerializeField] float minAirTime = 0.1f;

    public event Action<float> Landed;

    Rigidbody rb;
    Transform cam;
    Vector3 move;
    bool isGrounded, wasGrounded, jumpQueued;
    float lastGroundedTime = -1f, lastPressedTime = -1f, airStartTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        cam = Camera.main.transform;
        wasGrounded = true;
    }

    void Update()
    {
        // Ground check
        isGrounded = Physics.CheckSphere(feet.position, checkRadius, groundMask,
                                         QueryTriggerInteraction.Ignore);

        // Landing event (only after more than minAirTime in the air)
        if (!isGrounded && wasGrounded) airStartTime = Time.time;
        if (isGrounded && !wasGrounded)
        {
            float airTime = Time.time - airStartTime;
            if (airTime > minAirTime) Landed?.Invoke(airTime);
        }
        wasGrounded = isGrounded;

        // Camera-relative movement
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 forward = cam.forward; forward.y = 0f; forward.Normalize();
        Vector3 right = cam.right;     right.y = 0f;   right.Normalize();
        move = Vector3.ClampMagnitude(forward * v + right * h, 1f);

        // Jump windows
        if (Input.GetKeyDown(KeyCode.Space)) lastPressedTime = Time.time;
        if (isGrounded && !jumpQueued && rb.linearVelocity.y <= 0f)
            lastGroundedTime = Time.time;

        bool canJump  = Time.time - lastGroundedTime <= coyoteTime;
        bool wantJump = Time.time - lastPressedTime  <= jumpBuffer;
        if (canJump && wantJump && !jumpQueued)
        {
            jumpQueued = true;
            lastGroundedTime = -1f;
            lastPressedTime = -1f;
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector3(move.x * speed, rb.linearVelocity.y, move.z * speed);

        if (jumpQueued)
        {
            jumpQueued = false;
            float g = -Physics.gravity.y;
            float v0 = Mathf.Sqrt(2f * g * jumpHeight);

            // Zero vertical speed first so a jump taken while falling isn't short
            Vector3 vel = rb.linearVelocity; vel.y = 0f; rb.linearVelocity = vel;
            rb.AddForce(Vector3.up * rb.mass * v0, ForceMode.Impulse);
        }

        // Extra gravity only while falling
        if (rb.linearVelocity.y < 0f)
            rb.AddForce(Physics.gravity * (fallMultiplier - 1f), ForceMode.Acceleration);
    }

    void OnDrawGizmosSelected()
    {
        if (feet == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(feet.position, checkRadius);
    }
}