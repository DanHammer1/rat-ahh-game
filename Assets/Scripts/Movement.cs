using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System.Net.NetworkInformation;
using Unity.Netcode;
using TMPro;
using NUnit.Framework.Constraints;
public class Movement : NetworkBehaviour
{
    Animator animator;
    Transform headBone;

    // camera rotation
    public float mouseSensitivity;
    private float verticalRotation;
    private Transform cameraTransform;
    public GameObject lookTarget;


    // ground movement
    private Rigidbody rb;
    public float moveSpeed = 2f;
    public float speed;
    public float moveHorizontal;
    public float moveForward;
    public bool isPerformingAbility = false;
    public readonly float forceMultiplier = 10;
    public readonly float jumpForceMultiplier = 0.2f;


    // jumping
    public float jumpforce = 5f;
    public float fallMultiplier = 2.5f; //reset in Start cause changing values here doesn't do anything for some reason
    public float ascendMultiplier = 2f; //reset in Start cause changing values here doesn't do anything for some reason
    public bool isGrounded = true;
    public bool toggleGravity = true;
    public bool pressedSpace = false;
    LayerMask GROUNDLAYER;
    public float timeAirborne = 0f;
    public float groundCheckDelay = 0.3f;
    public float playerHeight;
    public float raycastDistance;

    public Vector3 movement;
    public float yaw;


    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        cameraTransform = FindFirstObjectByType<Camera>().transform;

        // Set the raycast to be slightly beneath the player's feet
        playerHeight = GetComponent<Collider>().bounds.size.y; //  / 2 * transform.localScale.y removed
        raycastDistance = (playerHeight / 2) + 0.2f;

        GROUNDLAYER = LayerMask.GetMask("groundLayer");

        if (transform.tag == "PlayerMouse")
        {
            moveSpeed = Constants.ratMoveSpeed;
            jumpforce = Constants.ratJumpForce;
            fallMultiplier = Constants.ratFallMultiplier;
            ascendMultiplier = Constants.ratAscendMultiplier;
        }
        if (transform.tag == "PlayerHuman")
        {
            moveSpeed = Constants.humanMoveSpeed;
            jumpforce = Constants.humanJumpForce;
            fallMultiplier = Constants.humanFallMultiplier;
            ascendMultiplier = Constants.humanAscendMultiplier;
            headBone = animator.GetBoneTransform(HumanBodyBones.Head);
        }
        Debug.Log("Avatar: " + animator.avatar);
        mouseSensitivity = Constants.mouseSensitivity;
        animator = GetComponent<Animator>();
    }


    void Update()
    {
        // if (!IsOwner) return;
        // moveHorizontal = Input.GetAxisRaw("Horizontal");
        // moveForward = Input.GetAxisRaw("Vertical");
    }

    void FixedUpdate()
    {
        if (!isPerformingAbility)
        {
            isGrounded = CheckGrounded();
        }

        Debug.DrawRay(
            transform.position + Vector3.up * 0.05f,
            Vector3.down * 0.075f,
            Color.red
        );
        RaycastHit hit;
        bool grounded = Physics.Raycast(
            transform.position + Vector3.up * 0.05f,
            Vector3.down,
            out hit,
            0.075f,
            GROUNDLAYER
        );

        if (!IsOwner) return;
        moveHorizontal = Input.GetAxisRaw("Horizontal");
        moveForward = Input.GetAxisRaw("Vertical");
        if (!isPerformingAbility)
        {
            MovePlayer(moveSpeed);
        }

        // ApplyJumpPhysics(fallMultiplier, ascendMultiplier);
        if (Input.GetButton("Jump") && isGrounded)
        {
            Jump(jumpforce, ascendMultiplier, fallMultiplier);
        }
        // Checking when we're on the ground and keeping track of our ground check delay
        if (!isGrounded && toggleGravity)
        {
            rb.useGravity = true;
            // timeAirborne += Time.deltaTime;
            // Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        }
        else if (toggleGravity)
        {
            rb.useGravity = false;
        }

        speed = new Vector2(moveForward, moveHorizontal).magnitude; // replaced below line with this cause moving camera was triggering walk animation
        // speed = rb.linearVelocity.magnitude; // Do not delete PlayerAnimator script uses this.

        lookTarget.transform.position = cameraTransform.position + cameraTransform.forward * 1f;

        verticalRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        // verticalRotation = Mathf.Clamp(verticalRotation, -60f, 60f);
    }

    void LateUpdate()
    {
        if (!IsOwner) return;

        ApplyHeadTilt();
    }

    void ApplyHeadTilt()
    {
        if (headBone == null) return;

        Vector3 currentEuler = headBone.localEulerAngles;
        currentEuler.x = verticalRotation;
        headBone.localEulerAngles = currentEuler;

        // Quaternion animRotation = headBone.localRotation;
        // Quaternion pitchRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        // headBone.localRotation = animRotation * pitchRotation;
    }


    bool CheckGrounded()
    {
        RaycastHit hit;
        bool grounded = Physics.Raycast(transform.position + Vector3.up * 0.05f, Vector3.down, out hit, 0.075f, GROUNDLAYER);
        if (grounded)
        {
            pressedSpace = false;
        }
        return grounded;
    }

    public void MovePlayer(float moveSpeed)
    {
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();
        movement = (camForward * moveForward + camRight * moveHorizontal).normalized;

        if (movement != Vector3.zero)
        {
            yaw = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg;
        }

        // Apply rotation
        Quaternion targetRotation = Quaternion.Euler(0, yaw, 0);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 8f * Time.fixedDeltaTime));

        // Movement
        Vector3 targetVelocity = movement * moveSpeed;
        Vector3 velocity = rb.linearVelocity;
        velocity.x = targetVelocity.x;
        velocity.y = 0;
        velocity.z = targetVelocity.z;
        rb.AddForce(velocity * forceMultiplier);

        LimitSpeed(moveSpeed);

        if (isGrounded && moveHorizontal == 0 && moveForward == 0)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    void LimitSpeed(float maxSpeed)
    {
        if (rb == null) return;

        Vector3 surfaceVelocity = rb.linearVelocity;
        surfaceVelocity.y = 0;

        if (surfaceVelocity.magnitude <= maxSpeed) return;

        surfaceVelocity /= surfaceVelocity.magnitude;
        surfaceVelocity *= moveSpeed;

        surfaceVelocity.y = rb.linearVelocity.y;

        rb.linearVelocity = surfaceVelocity;
    }

    public void Jump(float jumpHeight, float ascendMultiplier, float fallMultiplier)
    {
        pressedSpace = true;
        isGrounded = false;
        timeAirborne = groundCheckDelay;

        float gravity = Mathf.Abs(Physics.gravity.y) * ascendMultiplier;
        float jumpVelocity = Mathf.Sqrt(2f * gravity * jumpHeight);

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(new Vector3(rb.linearVelocity.x, jumpVelocity, rb.linearVelocity.z) *
            forceMultiplier * jumpForceMultiplier, ForceMode.Impulse); // initial burst for the jump
    }

    void ApplyJumpPhysics(float fallMultiplier, float ascendMultiplier)
    {
        if (rb.linearVelocity.y < 0)
        {
            // Falling: Apply fall multiplier to make descent faster
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0)
        {
            // Asending: Apply ascend multiplier to make ascent faster
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (ascendMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }
}