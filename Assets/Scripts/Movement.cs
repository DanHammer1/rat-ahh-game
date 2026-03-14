using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System.Net.NetworkInformation;

public class Movement : MonoBehaviour
{
    // camera rotation
    public float mouseSensitivity;
    private float verticalRotation;
    private Transform cameraTransform;


    // ground movement
    private Rigidbody rb;
    public float moveSpeed = 5f;
    private float moveHorizontal;
    private float moveForward;


    // jumping
    public float jumpforce = 10f;
    public float fallMultiplier = 2.5f;
    public float ascendMultiplier = 2f;
    public bool isGrounded = true;
    public LayerMask groundLayer;
    public float groundCheckTimer = 0f;
    public float groundCheckDelay = 0.3f;
    public float playerHeight;
    public float raycastDistance;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        cameraTransform = Camera.main.transform;

        // Set the raycast to be slightly beneath the player's feet
        playerHeight = GetComponent<CapsuleCollider>().height * transform.localScale.y;
        raycastDistance = (playerHeight / 2) + 0.2f;
    }

    void FixedUpdate()
    {
        MovePlayer();
        ApplyJumpPhysics();
    }

    void Update()
    {
        moveHorizontal = Input.GetAxisRaw("Horizontal");
        moveForward = Input.GetAxisRaw("Vertical");

        // RotateCamera();

        if (Input.GetButton("Jump") && isGrounded)
        {
            Jump();
        }

        // Checking when we're on the ground and keeping track of our ground check delay
        if (!isGrounded && groundCheckTimer <= 0f)
        {
            Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
            isGrounded = Physics.Raycast(rayOrigin, Vector3.down, raycastDistance, groundLayer);
        }
        else groundCheckTimer -= Time.deltaTime;
    }
    void MovePlayer()
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

    // void RotateCamera()
    // {
    //     float horizontalRotation = Input.GetAxis("Mouse X") * mouseSensitivity;
    //     transform.Rotate(0, horizontalRotation, 0);

    //     verticalRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
    //     verticalRotation = Mathf.Clamp(verticalRotation, -90, 90);

    //     cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    // }

    void Jump()
    {
        isGrounded = false;
        groundCheckTimer = groundCheckDelay;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpforce, rb.linearVelocity.z); // initial burst for the jump
    }

    void ApplyJumpPhysics()
    {
        if (rb.linearVelocity.y < 0)
        {
            // Falling: Apply fall multiplier to make descent faster
            rb.linearVelocity += Vector3.up * Physics.gravity.y * fallMultiplier * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0)
        {
            // Asending: Apply ascend multiplier to make ascent faster
            rb.linearVelocity += Vector3.up * Physics.gravity.y * ascendMultiplier * Time.fixedDeltaTime;
        }
    }
}







