using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System.Net.NetworkInformation;
using Unity.Netcode;
using TMPro;
public class Movement : NetworkBehaviour
{
    // camera rotation
    public float mouseSensitivity;
    private float verticalRotation;
    private Transform cameraTransform;


    // ground movement
    private Rigidbody rb;
    public float moveSpeed = 2f;
    public float speed;
    public float moveHorizontal;
    public float moveForward;
    public bool isPerformingAbility = false;


    // jumping
    public float jumpforce = 5f;
    public float fallMultiplier = 2.5f; //reset in Start cause changing values here doesn't do anything for some reason
    public float ascendMultiplier = 2f; //reset in Start cause changing values here doesn't do anything for some reason
    public bool isGrounded = true;
    public bool pressedSpace = false;
    public LayerMask groundLayer;
    public float groundCheckTimer = 0f;
    public float groundCheckDelay = 0.3f;
    public float playerHeight;
    public float raycastDistance;


    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        // cameraTransform = Camera.main.transform;

        // Set the raycast to be slightly beneath the player's feet
        playerHeight = GetComponent<Collider>().bounds.size.y / 2 * transform.localScale.y;
        raycastDistance = (playerHeight / 2) + 0.2f;

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
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        if (!isPerformingAbility)
        {
            MovePlayer(moveSpeed);
        }
        ApplyJumpPhysics(fallMultiplier, ascendMultiplier);

        if (!IsOwner) return;

        moveHorizontal = Input.GetAxisRaw("Horizontal");
        moveForward = Input.GetAxisRaw("Vertical");

        if (Input.GetButton("Jump") && isGrounded)
        {
            Jump(jumpforce, ascendMultiplier);
        }

        // Checking when we're on the ground and keeping track of our ground check delay
        if (!isGrounded && groundCheckTimer <= 0f)
        {
            Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
            isGrounded = Physics.Raycast(rayOrigin, Vector3.down, raycastDistance, groundLayer);
            if (isGrounded) pressedSpace = false;
        }
        else groundCheckTimer -= Time.deltaTime;


        speed = new Vector2(moveForward, moveHorizontal).magnitude; // replaced below line with this cause moving camera was triggering walk animation
        // speed = rb.linearVelocity.magnitude; // Do not delete PlayerAnimator script uses this.
    }


    public void MovePlayer(float moveSpeed)
    {
        Vector3 movement = (transform.right * moveHorizontal + transform.forward * moveForward).normalized;
        Vector3 targetVelocity = movement * moveSpeed;

        // Apply movement to Rigidbody
        Vector3 velocity = rb.linearVelocity;
        velocity.x = targetVelocity.x;
        velocity.z = targetVelocity.z;
        rb.linearVelocity = velocity;

        // If we aren't moving and are on the ground, stop velocity so we don't slide
        if (isGrounded && moveHorizontal == 0 && moveForward == 0)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    public void MovePlayer(Vector3 direction, float moveSpeed) // Used for rat ability in Player script
    {
        Vector3 movement = direction.normalized;
        Vector3 targetVelocity = movement * moveSpeed;

        Vector3 velocity = rb.linearVelocity;
        velocity.x = targetVelocity.x;
        velocity.z = targetVelocity.z;
        rb.linearVelocity = velocity;
    }

    public void Jump(float jumpHeight, float ascendMultiplier)
    {
        pressedSpace = true;
        isGrounded = false;
        groundCheckTimer = groundCheckDelay;

        float gravity = Mathf.Abs(Physics.gravity.y) * ascendMultiplier;
        float jumpVelocity = Mathf.Sqrt(2f * gravity * jumpHeight);

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpVelocity, rb.linearVelocity.z); // initial burst for the jump
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







