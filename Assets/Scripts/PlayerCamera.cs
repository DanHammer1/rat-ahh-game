using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using Unity.Netcode;

public class PlayerCamera : MonoBehaviour
{
    public static PlayerCamera instance;
    public static GameObject mainCamera;
    public GameObject player;
    CinemachinePositionComposer cinemachinePositionComposer;
    CinemachineInputAxisController cinemachineInputAxisController;
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
        mouseSensitivity = Constants.mouseSensitivity;
    }

    void Update()
    {
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

        HumanPlayer human = Player.localPlayer as HumanPlayer;

        if (!isCameraLocked)
        {
            netX += xMovement;
            netY -= yMovement;
            netY = Mathf.Clamp(netY, -90, 90);

            // Only sync player rotation when camera is not locked (i.e., not during ability)

            if (cameraState == CameraState.FirstPerson)
                movement.yaw = transform.eulerAngles.y;
        }

        thirdPersonRadius = Mathf.Clamp(thirdPersonRadius, 0, 10);

        if (thirdPersonRadius == 0)
        {
            cameraState = CameraState.FirstPerson;
        }
        else cameraState = CameraState.ThirdPerson;

        cinemachinePositionComposer.CameraDistance = thirdPersonRadius;

        // Make rat invisible in first person.
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

        if (Input.GetKeyDown(KeyCode.Alpha1))
            Time.timeScale = 0.05f;

        if (Input.GetKeyDown(KeyCode.Alpha2))
            Time.timeScale = 0.2f;

        if (Input.GetKeyDown(KeyCode.Alpha3))
            Time.timeScale = 1f;

        cinemachineInputAxisController.Controllers[0].Input.Gain = mouseSensitivity * movement.movementRecoveryMultiplier;
        cinemachineInputAxisController.Controllers[1].Input.Gain = -mouseSensitivity * movement.movementRecoveryMultiplier;
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
}
