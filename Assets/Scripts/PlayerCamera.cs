using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class PlayerCamera : MonoBehaviour
{
    public static PlayerCamera instance;
    public GameObject player;
    CinemachinePositionComposer cinemachinePositionComposer;
    Movement movement;
    SkinnedMeshRenderer playerRenderer;

    float thirdPersonRadius; // If 0 then first person.
    public bool isCameraLocked;

    void Awake()
    {
        instance = this;
        isCameraLocked = false;
    }

    public float thirdPersonScrollSensitivity;
    private float xMovement;
    private float yMovement;
    private float netY;
    private float netX;

    public enum CameraState
    {
        FirstPerson,
        ThirdPerson
    };

    public CameraState cameraState = CameraState.FirstPerson;


    void Start()
    {
        cinemachinePositionComposer = this.GetComponent<CinemachinePositionComposer>();
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

        xMovement = Input.GetAxis("Mouse X");
        yMovement = Input.GetAxis("Mouse Y");

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

        thirdPersonRadius = Mathf.Clamp(thirdPersonRadius, 0, 10);

        if (thirdPersonRadius == 0)
        {
            cameraState = CameraState.FirstPerson;
        }
        else cameraState = CameraState.ThirdPerson;

        cinemachinePositionComposer.CameraDistance = thirdPersonRadius;

        // Make rat invisible in first person.
        if (cameraState == CameraState.FirstPerson) {
            playerRenderer.shadowCastingMode = 
                UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                cinemachinePositionComposer.Damping = new Vector3(0.05f, 0.05f, 0.05f);
        }
        else {
            playerRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            cinemachinePositionComposer.Damping = new Vector3(0.4f, 0.4f, 0.4f);
        }
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
            Player.localPlayer.transform.rotation = Quaternion.Euler(0, targetYaw, 0);
        }
    }

    public void SetCameraYaw(float yaw)
    {
        netX = yaw;
    }
}
