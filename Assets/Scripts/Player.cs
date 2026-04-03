using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
using TMPro;

public class Player : NetworkBehaviour
{
    public static Player localPlayer;
    CinemachineCamera cam;
    PlayerCamera playerCamera;
    public Transform cameraTarget;

    public NetworkVariable<float> maxHealth = new NetworkVariable<float>(100);
    public NetworkVariable<float> health = new NetworkVariable<float>();



    // Cheese/score info
    public GameObject eatCheesePrompt;
    public int score;
    public TextMeshProUGUI scoreText;



    // Rat info
    public bool ratAbilityInRange;
    public GameObject activateRatAbilityPrompt;
    private Player localHumanInRange;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            maxHealth.Value = 100;
            health.Value = maxHealth.Value;
        }

        if (!IsOwner) return;
        localPlayer = this;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SetupCamera();


        eatCheesePrompt = GameObject.FindWithTag("Eat Cheese Prompt");
        eatCheesePrompt.SetActive(false);

        score = 0;
        scoreText = GameObject.FindWithTag("Score").GetComponent<TextMeshProUGUI>();

        activateRatAbilityPrompt = GameObject.FindWithTag("Activate Rat Ability Prompt");
        activateRatAbilityPrompt.SetActive(false);
        ratAbilityInRange = false;
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
        }
    }

    void ActivateRatAbility()
    {

    }

    void Update()
    {
        if (ratAbilityInRange && Input.GetKeyDown(KeyCode.T))
        {
            ActivateRatAbility();
        }
    }
}
