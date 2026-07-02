using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using Unity.Netcode;
using System;

public class PlayerCamera : MonoBehaviour
{
    public static PlayerCamera instance;
    public static GameObject mainCamera;
    public GameObject player;
    CinemachinePositionComposer cinemachinePositionComposer;
    CinemachineInputAxisController cinemachineInputAxisController;
    CinemachineDecollider cinemachineDecollider;
    CinemachineCamera cinemachineCamera;
    CinemachineBasicMultiChannelPerlin cinemachineNoise;
    Movement movement;
    SkinnedMeshRenderer playerRenderer;

    float thirdPersonRadius; // If 0 then first person.
    public bool isCameraLocked;

    void Awake()
    {
        instance = this;
        isCameraLocked = false;
        mainCamera = this.gameObject;
    }

    public float thirdPersonScrollSensitivity;
    private float xMovement;
    private float yMovement;
    private float netY;
    private float netX;
    public float mouseSensitivity;

    public Action onThirdPersonEnter;
    public Action onFirstPersonEnter;

    public enum CameraState
    {
        FirstPerson,
        ThirdPerson
    };

    public CameraState cameraState = CameraState.FirstPerson;

    void Start()
    {
        cinemachinePositionComposer = this.GetComponent<CinemachinePositionComposer>();
        cinemachineInputAxisController = this.GetComponent<CinemachineInputAxisController>();
        cinemachineDecollider = this.GetComponent<CinemachineDecollider>();
        cinemachineCamera = this.GetComponent<CinemachineCamera>();
        cinemachineNoise = this.GetComponent<CinemachineBasicMultiChannelPerlin>();
        mouseSensitivity = Constants.mouseSensitivity;

        if (GameManager.GetLocalRole() == GameManager.PlayerRole.Hunter) {
            cinemachineCamera.Lens.FieldOfView = Constants.humanCameraFOV;
        }
        else if (GameManager.GetLocalRole() == GameManager.PlayerRole.Hider) {
            cinemachineCamera.Lens.FieldOfView = Constants.ratCameraFOV;
        }

        onFirstPersonEnter += disableCameraCollision;
        onThirdPersonEnter += enableCameraCollision;

        disableCameraCollision();
    }

    void Update()
    {
        // Test screen shake - press K
        if (Input.GetKeyDown(KeyCode.K))
        {
            TestScreenShake();
        }

        if (Player.localPlayer != null)
        {
            player = Player.localPlayer.gameObject;
            movement = player.GetComponent<Movement>();
            playerRenderer = player.transform.Find("Renderer").GetComponent<SkinnedMeshRenderer>();
        }
        else return;

        Vector3 centrePos = player.transform.GetChild(1).position;

        if (!isCameraLocked)
        {
            xMovement = Input.GetAxis("Mouse X");
            yMovement = Input.GetAxis("Mouse Y");
        }
        else
        {
            xMovement = 0;
            yMovement = 0;
        }


        thirdPersonRadius -= Input.GetAxis("Mouse ScrollWheel") * thirdPersonScrollSensitivity;

        if (!isCameraLocked)
        {
            netX += xMovement;
            netY -= yMovement;
            netY = Mathf.Clamp(netY, -90, 90);

            // Only sync player rotation when camera is not locked (i.e., not during ability)

            if (cameraState == CameraState.FirstPerson)
                movement.yaw = transform.eulerAngles.y;
        }

        thirdPersonRadius = GameManager.GetLocalRole() switch {
            GameManager.PlayerRole.Hunter => Mathf.Clamp(thirdPersonRadius, 0, Constants.humanMaxCameraThirdPersonRadius),
            GameManager.PlayerRole.Hider => Mathf.Clamp(thirdPersonRadius, 0, Constants.ratMaxCameraThirdPersonRadius),
            _ => thirdPersonRadius
        };


        if (thirdPersonRadius == 0)
        {
            if (cameraState != CameraState.FirstPerson)
                onFirstPersonEnter?.Invoke();
            
            cameraState = CameraState.FirstPerson;
        }
        else {
            if (cameraState != CameraState.ThirdPerson) {
                onThirdPersonEnter?.Invoke();
            }

            cameraState = CameraState.ThirdPerson;
        }
        cinemachinePositionComposer.CameraDistance = thirdPersonRadius;

        // Make player invisible in first person.
        if (cameraState == CameraState.FirstPerson)
        {
            playerRenderer.shadowCastingMode =
                UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            cinemachinePositionComposer.Damping = new Vector3(0.00f, 0.00f, 0.00f);
        }
        else
        {
            playerRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            cinemachinePositionComposer.Damping = new Vector3(0.4f, 0.4f, 0.4f);
        }

        /*if (Input.GetKeyDown(KeyCode.Alpha1))
            Time.timeScale = 0.05f;

        if (Input.GetKeyDown(KeyCode.Alpha2))
            Time.timeScale = 0.2f;

        if (Input.GetKeyDown(KeyCode.Alpha3))
            Time.timeScale = 1f;*/

        cinemachineInputAxisController.Controllers[0].Input.Gain = mouseSensitivity * movement.movementRecoveryMultiplier;
        cinemachineInputAxisController.Controllers[1].Input.Gain = -mouseSensitivity * movement.movementRecoveryMultiplier;
    }

    public void enableCameraCollision() {
        cinemachineDecollider.CameraRadius = Constants.cameraCollisionRadius;
    }

    public void disableCameraCollision() {
        cinemachineDecollider.CameraRadius = 0;
    }

    public void ForceLookAt(Vector3 targetPosition, Vector3 originalPosition)
    {
        Vector3 dir = (targetPosition - originalPosition).normalized;

        float targetYaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        float targetPitch = -Mathf.Asin(dir.y) * Mathf.Rad2Deg;

        netX = targetYaw;
        netY = targetPitch;

        if (Player.localPlayer != null)
        {
            Rigidbody rb = Player.localPlayer.GetComponent<Rigidbody>();
            rb.rotation = Quaternion.Euler(0, targetYaw, 0);
        }
    }

    public void SetCameraYaw(float yaw)
    {
        netX = yaw;
    }

    // Test method - press K to trigger screen shake
    public void TestScreenShake()
    {
        if (cinemachineNoise != null)
        {
            Debug.Log($"Noise component found!");
            TriggerScreenShake(100f, 50f, 0.5f);  // Extreme shake for 0.5 seconds
        }
        else
        {
            Debug.LogError("CinemachineBasicMultiChannelPerlin component not found!");
        }
    }

    /// <summary>
    /// Triggers a screen shake effect
    /// </summary>
    public void TriggerScreenShake(float amplitude, float frequency, float duration)
    {
        if (cinemachineNoise == null)
            return;

        StartCoroutine(ScreenShakeCoroutine(amplitude, frequency, duration));
    }

    private System.Collections.IEnumerator ScreenShakeCoroutine(float amplitude, float frequency, float duration)
    {
        // Store original values
        float originalAmplitude = cinemachineNoise.AmplitudeGain;
        float originalFrequency = cinemachineNoise.FrequencyGain;
        bool wasEnabled = cinemachineNoise.enabled;

        // Enable and set shake values
        cinemachineNoise.enabled = true;
        cinemachineNoise.AmplitudeGain = amplitude;
        cinemachineNoise.FrequencyGain = frequency;

        yield return new WaitForSeconds(duration);

        // Restore original values
        cinemachineNoise.AmplitudeGain = originalAmplitude;
        cinemachineNoise.FrequencyGain = originalFrequency;
        cinemachineNoise.enabled = wasEnabled;
    }
}
