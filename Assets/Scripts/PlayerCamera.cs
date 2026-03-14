using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    public GameObject player;
    private float xMovement;
    private float yMovement;
    private float netY;
    private float netX;
    public float scrollInput;
    public float cameraDistanceMultiplier = 0;
    public float cameraDistanceMultiplierReal = 0;
    private Mouse _mouse;
    private void OnEnable() => _mouse = Mouse.current;

    void Update()
    {
        if (Player.localPlayer != null)
        {
            player = Player.localPlayer.gameObject;
        }
        else return;

        scrollInput = _mouse.scroll.ReadValue().y;
        if (scrollInput < 0) cameraDistanceMultiplier += 0.5f;
        else if (scrollInput > 0) cameraDistanceMultiplier -= 0.5f;
        cameraDistanceMultiplier = Mathf.Clamp(cameraDistanceMultiplier, 0, 10);
        if (cameraDistanceMultiplier < 3) cameraDistanceMultiplierReal = 0;
        else cameraDistanceMultiplierReal = cameraDistanceMultiplier;


        gameObject.transform.position = player.transform.GetChild(1).position - transform.forward * cameraDistanceMultiplierReal;

        xMovement = Input.GetAxis("Mouse X");
        yMovement = Input.GetAxis("Mouse Y");

        netX += xMovement;
        netY -= yMovement;

        netY = Mathf.Clamp(netY, -90, 90);

        transform.rotation = Quaternion.Euler(netY, netX, 0);
        player.transform.rotation = Quaternion.Euler(player.transform.eulerAngles.x, transform.eulerAngles.y, player.transform.eulerAngles.z);
    }
}
