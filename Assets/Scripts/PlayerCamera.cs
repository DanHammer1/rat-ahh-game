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
            //player.transform.rotation = Quaternion.Euler(player.transform.eulerAngles.x,
            //transform.eulerAngles.y, player.transform.eulerAngles.z);
        }

        thirdPersonRadius = Mathf.Clamp(thirdPersonRadius, 0, 10);

        if (thirdPersonRadius == 0)
        {
            cameraState = CameraState.FirstPerson;
        }
        else cameraState = CameraState.ThirdPerson;

        cinemachinePositionComposer.CameraDistance = thirdPersonRadius;

        // transform.position = centrePos + (thirdPersonRadius *
        //     (Quaternion.Euler(netY, netX, 0) * -Vector3.forward));
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
