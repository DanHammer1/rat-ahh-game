using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
using TMPro;
using System.Collections;
using UnityEditor;
using UnityEngine.SocialPlatforms;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

#if UNITY_EDITOR
using System.Reflection;
#endif

public class Player : NetworkBehaviour {
    public static Player localPlayer;

    CinemachineCamera cam;
    public Transform cameraTarget;

    public NetworkVariable<ulong> clientId = new NetworkVariable<ulong>();
    public NetworkVariable<Vector3> initialSpawnPosition = new NetworkVariable<Vector3>();
    public NetworkVariable<float> maxHealth = new NetworkVariable<float>(100);
    public NetworkVariable<float> health = new NetworkVariable<float>();

    public NetworkVariable<bool> dead = new NetworkVariable<bool>(false);
    public Action onSpawn;
    private bool spawned = false;
    public Action onDeath;
    public Action onRevive;

    public Movement movement;
    public CapsuleCollider capsuleCollider;
    public Rigidbody rb;
    public PlayerCamera playerCamera;
    public ClientNetworkTransform clientNetworkTransform;
    public GameObject viewPosition;
    public SkinnedMeshRenderer skinnedMeshRenderer;

    // Ability Icon
    public GameObject abilityIcon;
    public GameObject abilityIconBackgroundOutline;
    public Image abilityIconBackgroundOutlineImage;
    public GameObject abilityT;
    public TextMeshProUGUI abilityTText;
    public GameObject abilityIconBackground;
    public Image abilityIconBackgroundImage;

    // Rat Ability Shake Meter
    public GameObject ratAbilityShakeUI;
    public GameObject shakeProgressBar;
    public Image shakeProgressBarImage;
    public NetworkVariable<bool> isCarryingCoin = new NetworkVariable<bool>(false);

    // Cheese/score info
    public NetworkVariable<int> score = new NetworkVariable<int>();
    public TextMeshProUGUI scoreText;

    public bool isInUIMenu = false;

    Animator animator;
    public CinemachineImpulseSource impulseSource;


    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ToggleIsCarryingCoinRpc() {
        isCarryingCoin.Value = !isCarryingCoin.Value;
    }

    [ClientRpc]
    public void ToggleIsCarryingCoinClientRpc() {
        isCarryingCoin.Value = !isCarryingCoin.Value;
    }

    public override void OnNetworkSpawn() {
        animator = GetComponent<Animator>();
        movement = GetComponent<Movement>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        clientNetworkTransform = GetComponent<ClientNetworkTransform>();
        skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        transform.SetPositionAndRotation(initialSpawnPosition.Value, transform.rotation);
        rb.position = initialSpawnPosition.Value;
        Physics.SyncTransforms();

        if (IsServer) {
            maxHealth.Value = 100;
            health.Value = maxHealth.Value;
            score.Value = 0;
        }

        if (!IsOwner) return;

        localPlayer = this;

        playerCamera = PlayerCamera.instance;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SetupCamera();

        if (SceneManager.GetActiveScene().name == "Game") {
            scoreText = GameObject.FindWithTag("Score").GetComponent<TextMeshProUGUI>();
        }

        ratAbilityShakeUI = GameObject.FindWithTag("Rat Ability Shake UI");
        shakeProgressBar = GameObject.FindWithTag("Shake Progress Bar");
        shakeProgressBarImage = shakeProgressBar?.GetComponent<Image>();

        impulseSource = GetComponent<CinemachineImpulseSource>();

        score.OnValueChanged += (int oldValue, int newValue) => {
            int difference = newValue - oldValue;
            GameObject scoreAddedNotice = Instantiate(Assets.instance.scoreAddedNotice);
            scoreAddedNotice.transform.SetParent(GameObject.FindWithTag("ScoreAddedParent").transform);
            if (newValue - oldValue > 0) {
                scoreAddedNotice.GetComponent<TextMeshProUGUI>().text = $"+{difference}";
            } else {
                scoreAddedNotice.GetComponent<TextMeshProUGUI>().text = $"{difference}";
                scoreAddedNotice.GetComponent<TextMeshProUGUI>().color = new Color(1, 0, 0, 1);
            }
        };
    }

    void SetupCamera() {
        CinemachineCamera cam = FindFirstObjectByType<CinemachineCamera>();

        if (cam == null) {
            Debug.LogError("CinemachineCamera not found in scene!");
            return;
        }

        if (cameraTarget == null) {
            Debug.LogError("CameraTarget not assigned on Player!");
            return;
        }

        cam.Follow = cameraTarget;
        cam.LookAt = cameraTarget;
    }

    [ServerRpc]
    public void SetColliderStateServerRpc(bool state) {
        capsuleCollider.enabled = state;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void EditHealthServerRpc(float newHealth) {
        health.Value = newHealth;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void EditScoreServerRpc(int newScore) {
        score.Value = newScore;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void AddScoreServerRpc(int newScore) {
        score.Value += newScore;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SetDeadStateRpc(bool state) {
        dead.Value = state;
    }

    protected virtual void Update() {
        if (!spawned) {
            onSpawn?.Invoke();
            spawned = true;
        }

        // if (Input.GetKeyDown(KeyCode.O)) {
        //     ClearConsole();
        // }

        if (IsServer && health.Value <= 0 && !dead.Value) {
            onDeath?.Invoke();
        }
    }

#if UNITY_EDITOR
    public void ClearConsole() {
        var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }
#endif
}