using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
using TMPro;
using System.Collections;
using UnityEditor;

#if UNITY_EDITOR
using System.Reflection;
#endif

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
    public bool isClinging;
    public bool isSlapping;
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
        if (IsOwner && transform.tag == "PlayerMouse" && other.CompareTag("Rat Stun Hitbox"))
        {
            Player player = other.GetComponentInParent<Player>();
            localHumanInRange = player;

            ratAbilityInRange = true;
            activateRatAbilityPrompt.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (IsOwner && transform.tag == "PlayerMouse" && other.CompareTag("Rat Stun Hitbox"))
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

    IEnumerator RatAbilityCoroutine()
    {
        ClearConsole();

        isSlapping = false;
        isClinging = false;

        Vector3 startPos = transform.position;
        Vector3 targetPos = localHumanInRange.viewPosition.transform.position;

        movement.isPerformingAbility = true; // prevents movement during ability

        float ratAbilityDuration = Constants.ratAbilityDuration;
        float elapsed = 0;

        bool forceApplied = false;

        // Force camera to look at human (possibly not needed)
        // Vector3 dirToViewPos = targetPos - startPos;
        // dirToViewPos.y = 0;
        // dirToViewPos.Normalize();
        // float targetYaw = Mathf.Atan2(dirToViewPos.x, dirToViewPos.z) * Mathf.Rad2Deg;
        // movement.yaw = targetYaw;
        // playerCamera.SetCameraYaw(targetYaw);

        // Force rat to rotate towards human
        playerCamera.ForceLookAt(targetPos, startPos);

        Rigidbody rb = movement.GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        float originalDrag = rb.linearDamping;

        movement.isGrounded = false;
        movement.pressedSpace = true;

        Vector3 forceToAdd = Vector3.zero;
        forceToAdd.x = (targetPos.x - startPos.x) / ratAbilityDuration;
        forceToAdd.y = (((targetPos.y - startPos.y) - Physics.gravity.y / 2 * Mathf.Pow(ratAbilityDuration, 2)) / ratAbilityDuration) / 1.18f;
        forceToAdd.z = (targetPos.z - startPos.z) / ratAbilityDuration;

        if (!forceApplied)
        {
            rb.linearDamping = 0;
            rb.AddForce(forceToAdd * rb.mass, ForceMode.Impulse);
            forceApplied = true;
        }

        while (elapsed < ratAbilityDuration)
        {
            float t = elapsed / ratAbilityDuration;
            elapsed += Time.fixedDeltaTime;

            // Debug.DrawLine(transform.position, targetPos, Color.red, 0.02f);
            // if (Vector3.Distance(transform.position, targetPos) <= Constants.ratAbilityClingRange)
            // {
            //     Debug.Log(Vector3.Distance(transform.position, targetPos));
            // }

            if (Vector3.Distance(transform.position, targetPos) <= Constants.ratAbilityClingRange)
            {
                rb.linearVelocity = Vector3.zero;
                rb.useGravity = false;
                movement.toggleGravity = false;

                Transform humanHead = localHumanInRange.animator.GetBoneTransform(HumanBodyBones.Head);
                transform.SetParent(humanHead);

                rb.constraints = RigidbodyConstraints.FreezeAll;
                isClinging = true;
                break;
            }
            yield return new WaitForFixedUpdate();
        }

        rb.linearVelocity = Vector3.zero;
        rb.linearDamping = originalDrag;
        // movement.isPerformingAbility = false;
        Debug.DrawLine(transform.position, targetPos, Color.red, 3f);
        // playerCamera.isCameraLocked = false;
    }

    void Update()
    {
        if (ratAbilityInRange && Input.GetKeyDown(KeyCode.T))
        {
            ActivateRatAbility();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("test");
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            ClearConsole();
        }

        if (Input.GetKeyDown(KeyCode.Q) && isClinging)
        {
            isSlapping = !isSlapping;
        }
    }

#if UNITY_EDITOR
    public void ClearConsole()
    {
        var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }
#endif
}
