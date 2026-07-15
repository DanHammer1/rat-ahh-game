using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
using TMPro;
using System.Collections;
using UnityEditor;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using System;

#if UNITY_EDITOR
using System.Reflection;
#endif

public class Player : NetworkBehaviour
{
    public static Player localPlayer;

    CinemachineCamera cam;
    public Transform cameraTarget;

    public NetworkVariable<ulong> clientId = new NetworkVariable<ulong>();
    public NetworkVariable<float> maxHealth = new NetworkVariable<float>(100);
    public NetworkVariable<float> health = new NetworkVariable<float>();

    public Action onSpawn;
    private bool spawned = false;
    public Action onDeath;
    public bool dead = false;
    public Action onRevive;

    public Movement movement;
    BoxCollider boxCollider;
    public Rigidbody rb;
    public PlayerCamera playerCamera;
    public ClientNetworkTransform clientNetworkTransform;
    public GameObject viewPosition;

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

    Animator animator;
    public CinemachineImpulseSource impulseSource;

    public GameObject eatCheesePrompt;
    public GameObject pickUpCoinPrompt;

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ToggleIsCarryingCoinRpc()
    {
        isCarryingCoin.Value = !isCarryingCoin.Value;
    }

    [ClientRpc]
    public void ToggleIsCarryingCoinClientRpc()
    {
        isCarryingCoin.Value = !isCarryingCoin.Value;
    }

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
            score.Value = 0;
        }

        if (!IsOwner) return;

        localPlayer = this;

        playerCamera = PlayerCamera.instance;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SetupCamera();

        scoreText = GameObject.FindWithTag("Score").GetComponent<TextMeshProUGUI>();

        ratAbilityShakeUI = GameObject.FindWithTag("Rat Ability Shake UI");
        shakeProgressBar = GameObject.FindWithTag("Shake Progress Bar");
        shakeProgressBarImage = shakeProgressBar?.GetComponent<Image>();

        impulseSource = GetComponent<CinemachineImpulseSource>();
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

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void EditHealthServerRpc(float newHealth)
    {
        health.Value = newHealth;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void EditScoreServerRpc(int newScore)
    {
        score.Value = newScore;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void AddScoreServerRpc(int newScore)
    {
        score.Value += newScore;
    }

    protected virtual void Update()
    {
        if (!spawned) {
            onSpawn?.Invoke();
            spawned = true;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("test");
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            ClearConsole();
        }

        if (health.Value <= 0 && !dead) {
            onDeath?.Invoke();
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