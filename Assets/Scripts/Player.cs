using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
using TMPro;
using System.Collections;

public class Player : NetworkBehaviour
{
    public static Player localPlayer;

    CinemachineCamera cam;
    public Transform cameraTarget;

    public NetworkVariable<float> maxHealth = new NetworkVariable<float>(100);
    public NetworkVariable<float> health = new NetworkVariable<float>();
    public GameObject viewPosition;
    Movement movement;
    private Rigidbody rb;
    private PlayerCamera playerCamera;



    // Cheese/score info
    public GameObject eatCheesePrompt;
    public int score;
    public TextMeshProUGUI scoreText;



    // Rat info
    public bool ratAbilityInRange;
    public GameObject activateRatAbilityPrompt;
    private Player localHumanInRange;
    Animator animator;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            maxHealth.Value = 100;
            health.Value = maxHealth.Value;
        }

        if (!IsOwner) return;
        localPlayer = this;

        animator = GetComponent<Animator>();

        playerCamera = PlayerCamera.instance;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SetupCamera();

        rb = GetComponent<Rigidbody>();


        eatCheesePrompt = GameObject.FindWithTag("Eat Cheese Prompt");
        eatCheesePrompt.SetActive(false);

        score = 0;
        scoreText = GameObject.FindWithTag("Score").GetComponent<TextMeshProUGUI>();

        activateRatAbilityPrompt = GameObject.FindWithTag("Activate Rat Ability Prompt");
        activateRatAbilityPrompt.SetActive(false);
        ratAbilityInRange = false;

        movement = GetComponentInParent<Movement>();
    }

    void SetupCamera()
    {
        CinemachineCamera cam = FindFirstObjectByType<CinemachineCamera>();

        if (cam == null)
        {
            Debug.LogError("CinemachineCamera not found in scene!");
            return;
        }

        if (cameraTarget == null)
        {
            Debug.LogError("CameraTarget not assigned on Player!");
            return;
        }

        cam.Follow = cameraTarget;
        cam.LookAt = cameraTarget;
    }


    void OnTriggerStay(Collider other)
    {
        if (transform.tag == "PlayerMouse" && other.CompareTag("Rat Stun Hitbox"))
        {
            Player player = other.GetComponentInParent<Player>();
            localHumanInRange = player;

            ratAbilityInRange = true;
            activateRatAbilityPrompt.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (transform.tag == "PlayerMouse" && other.CompareTag("Rat Stun Hitbox"))
        {
            ratAbilityInRange = false;
            activateRatAbilityPrompt.SetActive(false);

            localHumanInRange = null;
        }
    }

    void ActivateRatAbility()
    {
        if (localHumanInRange == null) return; //safety check
        StartCoroutine(RatAbilityCoroutine());
    }

    // IEnumerator RatAbilityCoroutine()
    // {
    //     Vector3 localHumanViewPosition = localHumanInRange.viewPosition.transform.position;

    //     // Calculate target yaw to face the view position and set camera immediately
    //     Vector3 dirToViewPos = (localHumanViewPosition - transform.position);
    //     dirToViewPos.y = 0;
    //     dirToViewPos.Normalize();
    //     float targetYaw = Mathf.Atan2(dirToViewPos.x, dirToViewPos.z) * Mathf.Rad2Deg;
    //     movement.yaw = targetYaw;
    //     playerCamera.SetCameraYaw(targetYaw);

    //     float ratAbilityJumpHeight = localHumanViewPosition.y - transform.position.y;
    //     if (ratAbilityJumpHeight < 0.3f) ratAbilityJumpHeight = 0.3f; //Clamp height at minimum of 0.3

    //     // if jumpheignt > (localHumanViewPosition.y - transform.position.y), the rat will have to ascend and fall before reaching view position. Recalculate tempAscendMultiplier and tempFallMultiplier to make the whole ability last abilityDuration seconds regardless of where it is activated from.

    //     Vector3 tempRatVector3 = new Vector3(transform.position.x, 0, transform.position.z);
    //     Vector3 tempLocalHumanVector3 = new Vector3(localHumanViewPosition.x, 0, localHumanViewPosition.z);
    //     float differenceMagnitude = Vector3.Distance(tempRatVector3, tempLocalHumanVector3);
    //     float abilityDuration = 1f; //How long jump animation lasts
    //     float tempMoveSpeed = differenceMagnitude / abilityDuration;
    //     Vector3 direction = localHumanViewPosition - transform.position;
    //     direction.y = 0;
    //     direction.Normalize();

    //     movement.isPerformingAbility = true;
    //     playerCamera.isCameraLocked = true;
    //     playerCamera.ForceLookAt(localHumanViewPosition, transform.position);
    //     float tempAscendMultiplier = movement.ascendMultiplier;
    //     float tempFallMultiplier = movement.fallMultiplier;
    //     movement.Jump(ratAbilityJumpHeight, tempAscendMultiplier, tempFallMultiplier);

    //     float elapsed = 0f;
    //     while (elapsed < abilityDuration)
    //     {
    //         movement.MovePlayer(direction, tempMoveSpeed);
    //         elapsed += Time.fixedDeltaTime;
    //         yield return new WaitForFixedUpdate();
    //     }
    //     movement.isPerformingAbility = false;
    //     playerCamera.isCameraLocked = false;
    // }

    IEnumerator RatAbilityCoroutine()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = localHumanInRange.viewPosition.transform.position;

        float abilityDuration = 0.5f;
        float elapsed = 0;

        // Face target immediately
        Vector3 dirToViewPos = targetPos - startPos;
        dirToViewPos.y = 0;
        dirToViewPos.Normalize();
        float targetYaw = Mathf.Atan2(dirToViewPos.x, dirToViewPos.z) * Mathf.Rad2Deg;
        movement.yaw = targetYaw;
        playerCamera.SetCameraYaw(targetYaw);

        // Ambiguity between setting camera yaw and ForceLookAt()?

        movement.isPerformingAbility = true;
        playerCamera.isCameraLocked = true;
        playerCamera.ForceLookAt(targetPos, startPos);

        Rigidbody rb = movement.GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.useGravity = false;


        movement.isGrounded = false;
        movement.pressedSpace = true;

        while (elapsed < abilityDuration)
        {
            float t = elapsed / abilityDuration;

            // Horizontal movement
            Vector3 horizonatal = Vector3.Lerp(startPos, targetPos, t);

            // Vertical movement
            float heightDifference = targetPos.y - startPos.y;
            float minJumpHeight = 0.3f;
            float jumpHeight = Mathf.Max(minJumpHeight, heightDifference);

            // Decide where peak happens
            float normalized = Mathf.InverseLerp(minJumpHeight, 2f, heightDifference); // What is 2 doing here, and what is InverseLerp
            float peakT = Mathf.Lerp(0.9f, 0.5f, normalized); // wtf is this

            // Shifted parabola
            float x = (t - peakT) / peakT;
            float arc = jumpHeight * (t / peakT) * (1 - t);
            arc = Mathf.Max(0f, arc);

            // Combine base height + arc
            float y = Mathf.Lerp(startPos.y, targetPos.y, t) + arc;
            Vector3 newPos = new Vector3(horizonatal.x, y, horizonatal.z);
            rb.MovePosition(newPos);

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        rb.MovePosition(targetPos);
        rb.linearVelocity = Vector3.zero;
        rb.useGravity = true;
        movement.isPerformingAbility = false;
        playerCamera.isCameraLocked = false;
    }



    void Update()
    {
        if (ratAbilityInRange && Input.GetKeyDown(KeyCode.T))
        {
            ActivateRatAbility();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {

        }
    }
}
