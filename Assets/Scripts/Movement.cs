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
    public Transform headBone;

    // camera rotation
    private Transform cameraTransform;
    public GameObject lookTarget;


    // ground movement
    private Rigidbody rb;
    public float moveSpeed;
    public float speed;
    public float moveHorizontal;
    public float moveForward;
    public bool isPerformingAbility = false;
    public bool isMovementLocked = false;
    public bool isRotationLocked = false;
    public readonly float forceMultiplier = 10;
    public readonly float jumpForceMultiplier = 0.2f;
    public float movementRecoveryMultiplier;


    // jumping
    public float jumpforce = 5f;
    public float fallMultiplier = 2.5f; //reset in Start cause changing values here doesn't do anything for some reason
    public float ascendMultiplier = 2f; //reset in Start cause changing values here doesn't do anything for some reason
    public bool isGrounded = true;
    public bool toggleGravity = true;
    public bool pressedSpace = false;
    public Transform eyePosition;
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
        cameraTransform = FindFirstObjectByType<Camera>().transform;

        // Set the raycast to be slightly beneath the player's feet
        playerHeight = GetComponent<Collider>().bounds.size.y; //  / 2 * transform.localScale.y removed
        raycastDistance = (playerHeight / 2) + 0.2f;

        GROUNDLAYER = LayerMask.GetMask("groundLayer");
        animator = GetComponent<Animator>();

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
        movementRecoveryMultiplier = 1;
    }

    bool CheckPlayerGrounded()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        float xScale = boxCollider.size.x * gameObject.transform.lossyScale.x * 1.01f;
        float zScale = boxCollider.size.z * gameObject.transform.lossyScale.z * 1.01f;

        Vector3[] checkPositions = new Vector3[1];

        /*checkPositions[0] = (transform.position + Quaternion.Euler(0, transform.eulerAngles.y, 0) * new Vector3(-xScale / 2, 0, zScale / 2));
        checkPositions[1] = (transform.position + Quaternion.Euler(0, transform.eulerAngles.y, 0) * new Vector3(xScale / 2, 0, zScale / 2));
        checkPositions[2] = (transform.position + Quaternion.Euler(0, transform.eulerAngles.y, 0) * new Vector3(-xScale / 2, 0, -zScale / 2));
        checkPositions[3] = (transform.position + Quaternion.Euler(0, transform.eulerAngles.y, 0) * new Vector3(xScale / 2, 0, -zScale / 2));
        checkPositions[4] = transform.position;*/

        checkPositions[0] = transform.position;

        int hits = 0;

        foreach (Vector3 corner in checkPositions)
        {
            Debug.DrawRay(
                corner + Vector3.up * 0.05f,
                Vector3.down * 0.075f,
                Color.red
            );

            if (Physics.BoxCast(
                corner + Vector3.up * 0.05f,
                new Vector3(xScale / 2, 0, zScale / 2),
                Vector3.down,
                out RaycastHit hit,
                Quaternion.Euler(0, transform.eulerAngles.y, 0),
                0.075f, GROUNDLAYER))
                hits++;
        }
        return (hits >= 1);
    }

    void FixedUpdate()
    {
        if (Player.localPlayer != null)
        {
            eyePosition = Player.localPlayer.gameObject.transform.Find("EyePosition");
        }
        if (!isPerformingAbility)
        {
            isGrounded = CheckPlayerGrounded();
            if (isGrounded)
            {
                pressedSpace = false;
            }
        }

        if (!IsOwner) return;

        if (!isMovementLocked)
        {
            moveHorizontal = Input.GetAxisRaw("Horizontal");
            moveForward = Input.GetAxisRaw("Vertical");
        }
        else
        {
            moveHorizontal = 0f;
            moveForward = 0f;
        }

        if (isMovementLocked)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
        if (!isPerformingAbility)
        {
            MovePlayer(moveSpeed);
        }

        if (Input.GetButton("Jump") && isGrounded && !isMovementLocked)
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

        speed = new Vector2(moveForward, moveHorizontal).magnitude;
        //Debug.Log("speed: " + speed + ", moveForwards: " + moveForward + ", moveHorizontal: " + moveHorizontal);


        if (transform.tag == "PlayerHuman")
        {
            lookTarget.transform.position = cameraTransform.position + cameraTransform.forward * 1f;
        }
        Debug.Log(moveSpeed);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void MultiplyMoveSpeedRpc(float multiplier)
    {
        moveSpeed *= multiplier;
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

        switch (PlayerCamera.instance.cameraState)
        {
            case PlayerCamera.CameraState.FirstPerson:
                yaw = Mathf.Atan2(camForward.x, camForward.z) * Mathf.Rad2Deg;
                break;

            case PlayerCamera.CameraState.ThirdPerson:
                if (movement != Vector3.zero)
                    yaw = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg;
                break;
        }
        ;

        // Apply rotation
        if (!isRotationLocked)
        {
            Quaternion targetRotation = Quaternion.Euler(0, yaw, 0);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 8f * Time.fixedDeltaTime));
        }

        if (!isMovementLocked)
        {
            // Movement
            Vector3 targetVelocity = movement * moveSpeed * movementRecoveryMultiplier;
            Vector3 velocity = rb.linearVelocity;
            velocity.y = 0;

            Vector3 velocityChange = targetVelocity - velocity;
            rb.AddForce(velocityChange * forceMultiplier, ForceMode.Acceleration);

            LimitSpeed(moveSpeed);

            /*if (isGrounded && moveHorizontal == 0 && moveForward == 0)
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }*/
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

        rb.AddForce(new Vector3(0, jumpVelocity, 0) *
            forceMultiplier * jumpForceMultiplier, ForceMode.Impulse);
    }
}