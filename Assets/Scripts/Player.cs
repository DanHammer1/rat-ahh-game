using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
using TMPro;
using System.Collections;
using UnityEditor;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;


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
    public Movement movement;
    BoxCollider boxCollider;
    public Rigidbody rb;
    public PlayerCamera playerCamera;
    public ClientNetworkTransform clientNetworkTransform;

    public GameObject abilityIcon;
    public GameObject abilityIconBackgroundOutline;
    public Image abilityIconBackgroundOutlineImage;
    public GameObject abilityT;
    public TextMeshProUGUI abilityTText;
    public GameObject abilityIconBackground;

    // Cheese/score info
    public int score;
    public TextMeshProUGUI scoreText;

    Animator animator;

    public override void OnNetworkSpawn()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<Movement>();
        boxCollider = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
        clientNetworkTransform = GetComponent<ClientNetworkTransform>();

        if (IsServer)
        {
            maxHealth.Value = 100;
            health.Value = maxHealth.Value;
        }

        if (!IsOwner) return;
        localPlayer = this;

        playerCamera = PlayerCamera.instance;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SetupCamera();

        score = 0;
        scoreText = GameObject.FindWithTag("Score").GetComponent<TextMeshProUGUI>();

        abilityIcon = GameObject.FindWithTag("Ability Icon");
        abilityIconBackground = GameObject.FindWithTag("Ability Icon Background");
        abilityIconBackgroundOutline = GameObject.FindWithTag("Ability Icon Background Outline");
        abilityIconBackgroundOutlineImage = abilityIconBackgroundOutline.GetComponent<Image>();
        abilityT = GameObject.FindWithTag("Ability T");
        abilityTText = abilityT.GetComponent<TextMeshProUGUI>();

        abilityIcon.SetActive(false);
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

    [ServerRpc]
    public void SetColliderStateServerRpc(bool state)
    {
        boxCollider.enabled = state;
    }

    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("test");
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            ClearConsole();
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
