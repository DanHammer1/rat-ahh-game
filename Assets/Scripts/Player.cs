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



    // Cheese/score info
    public GameObject promptUI;
    public int score;
    public TextMeshProUGUI scoreText;



    public NetworkVariable<float> maxHealth = new NetworkVariable<float>(100);
    public NetworkVariable<float> health = new NetworkVariable<float>();

    void Awake()
    {
    }

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

        score = 0;

        SetupCamera();

        promptUI = GameObject.FindWithTag("Interact Prompt");
        scoreText = GameObject.FindWithTag("Score").GetComponent<TextMeshProUGUI>();
        promptUI.SetActive(false);
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


    void Update()
    {

    }
}
