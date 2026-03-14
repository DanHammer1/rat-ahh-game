using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerCamera : MonoBehaviour
{
    GameObject player;
    
    float thirdPersonRadius; // If 0 then first person.
    
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

        netX += xMovement;
        netY -= yMovement;    
        
        netY = Mathf.Clamp(netY, -90, 90);

        transform.rotation = Quaternion.Euler(netY, netX, 0);
        player.transform.rotation = Quaternion.Euler(player.transform.eulerAngles.x, 
            transform.eulerAngles.y, player.transform.eulerAngles.z);

        thirdPersonRadius = Mathf.Clamp(thirdPersonRadius, 0, 10);

        if (thirdPersonRadius == 0)
        {
            cameraState = CameraState.FirstPerson;
        }
        else cameraState = CameraState.ThirdPerson;

        transform.position = centrePos + (thirdPersonRadius * 
            (Quaternion.Euler(netY, netX, 0) * -Vector3.forward)); 
    }
}
