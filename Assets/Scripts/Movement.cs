using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System.Net.NetworkInformation;
using System;
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
    public NetworkVariable<float> moveHorizontal = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> moveForward = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<Vector3> camPos = new NetworkVariable<Vector3>(writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<Quaternion> camRotation = new NetworkVariable<Quaternion>(writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> isJumping = new NetworkVariable<bool>(writePerm: NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> pressedSpace = new NetworkVariable<bool>(false);
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
    public Transform eyePosition;
    LayerMask GROUNDLAYER;
    public float timeAirborne = 0f;
    public float groundCheckDelay = 0.3f;
    public float playerHeight;
    public float raycastDistance;

    public Vector3 movement;
    public float yaw;
    Crawl crawl;


    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        cameraTransform = FindFirstObjectByType<Camera>().transform;

        // Set the raycast to be slightly beneath the player's feet
        playerHeight = GetComponent<Collider>().bounds.size.y; //  / 2 * transform.localScale.y removed
        raycastDistance = (playerHeight / 2) + 0.2f;

        GROUNDLAYER = LayerMask.GetMask("groundLayer", "InteractableObject");
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
            crawl = GetComponent<Crawl>();
            crawl.onCrawlStart += () => moveSpeed *= Constants.crawlSpeedMultiplier;
            crawl.onCrawlEnd += () => moveSpeed *= 1 / Constants.crawlSpeedMultiplier;
        }
        movementRecoveryMultiplier = 1;

        GetComponent<Player>().onDeath += () => isMovementLocked = true;
        GetComponent<Player>().onRevive += () => isMovementLocked = false;

    }

    public bool CheckPlayerGrounded()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        float xScale = boxCollider.size.x * gameObject.transform.lossyScale.x * 1.01f;
        float zScale = boxCollider.size.z * gameObject.transform.lossyScale.z * 1.01f;

        if (Physics.BoxCast(
            transform.position + Vector3.up * 0.03f,
            new Vector3(xScale / 2, 0, zScale / 2),
            Vector3.down,
            out RaycastHit hit,
            Quaternion.Euler(0, transform.eulerAngles.y, 0),
            0.035f, GROUNDLAYER))
            return true;

        return false;
    }

    [ServerRpc]
    void RotateServerRpc(float yaw) {
        Quaternion targetRotation = Quaternion.Euler(0, yaw, 0);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 8f * Time.fixedDeltaTime));
    }

    void FixedUpdate()
    {
        if (Player.localPlayer == null || Player.localPlayer.dead) return;

        if (IsOwner) {
            if (!isMovementLocked)
            {
                moveHorizontal.Value = Input.GetAxisRaw("Horizontal");
                moveForward.Value = Input.GetAxisRaw("Vertical");
            }
            else
            {
                moveHorizontal.Value = 0f;
                moveForward.Value = 0f;
            }

            camPos.Value = cameraTransform.position;
            camRotation.Value = cameraTransform.rotation;

            isJumping.Value = Input.GetButton("Jump");

            Vector3 camForward = camRotation.Value * Vector3.forward;
            Vector3 camRight = camRotation.Value * Vector3.right;
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();
            movement = (camForward * moveForward.Value + camRight * moveHorizontal.Value).normalized;
            switch (PlayerCamera.instance.cameraState)
            {
                case PlayerCamera.CameraState.FirstPerson:
                    yaw = Mathf.Atan2(camForward.x, camForward.z) * Mathf.Rad2Deg;
                    break;

                case PlayerCamera.CameraState.ThirdPerson:
                    if (movement != Vector3.zero)
                        yaw = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg;
                    break;
            };

            if (!isRotationLocked) {
                RotateServerRpc(yaw);
            }
        }

        if (!IsServer) return;
        
        eyePosition = Player.localPlayer.gameObject.transform.Find("EyePosition");

        if (!isPerformingAbility)
        {
            isGrounded = CheckPlayerGrounded();
            if (isGrounded) {
                pressedSpace.Value = false;
            }
        }    

        if (isMovementLocked)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }

        if (!isPerformingAbility)
        {
            MovePlayer(moveSpeed);
        }

        if (isJumping.Value && isGrounded && !isMovementLocked) {
            Jump(jumpforce, ascendMultiplier, fallMultiplier);
        }

        if (!isGrounded && toggleGravity) {
            rb.useGravity = true;
        }
        
        else if (toggleGravity) {
            rb.useGravity = false;
        }

        speed = new Vector2(moveForward.Value, moveHorizontal.Value).magnitude;
        //Debug.Log("speed: " + speed + ", moveForwards: " + moveForward + ", moveHorizontal: " + moveHorizontal);


        if (transform.tag == "PlayerHuman") {
            lookTarget.transform.position = camPos.Value + camRotation.Value * Vector3.forward;
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void MultiplyMoveSpeedRpc(float multiplier) {
        moveSpeed *= multiplier;
    }

    public void MovePlayer(float moveSpeed) {
        Vector3 camForward = camRotation.Value * Vector3.forward;
        Vector3 camRight = camRotation.Value * Vector3.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();
        movement = (camForward * moveForward.Value + camRight * moveHorizontal.Value).normalized;

        if (!isMovementLocked)
        {
            Vector3 targetVelocity = movement * moveSpeed * movementRecoveryMultiplier;
            Vector3 velocity = rb.linearVelocity;
            velocity.y = 0;

            Vector3 velocityChange = targetVelocity - velocity;
            rb.AddForce(velocityChange * forceMultiplier, ForceMode.Acceleration);
        }
    }

    public void Jump(float jumpHeight, float ascendMultiplier, float fallMultiplier)
    {
        pressedSpace.Value = true;
        isGrounded = false;
        timeAirborne = groundCheckDelay;

        float gravity = Mathf.Abs(Physics.gravity.y) * ascendMultiplier;
        float jumpVelocity = Mathf.Sqrt(2f * gravity * jumpHeight);

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        rb.AddForce(new Vector3(0, jumpVelocity, 0) *
            forceMultiplier * jumpForceMultiplier, ForceMode.Impulse);
    }
}