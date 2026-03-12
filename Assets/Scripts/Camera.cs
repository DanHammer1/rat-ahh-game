using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class Camera : MonoBehaviour
{
    public GameObject player;
    private float xMovement;
    private float yMovement;
    private float netY;
    private float netX;

    void Update()
    {
        if (Player.localPlayer != null)
        {
            player = Player.localPlayer.gameObject;
        }
        else return;

        gameObject.transform.position = player.transform.GetChild(1).position;

        xMovement = Input.GetAxis("Mouse X");
        yMovement = Input.GetAxis("Mouse Y");

        netX += Input.GetAxis("Mouse X");
        netY -= Input.GetAxis("Mouse Y");
        transform.rotation = Quaternion.Euler(netY, netX, 0);
        player.transform.rotation = Quaternion.Euler(player.transform.eulerAngles.x, transform.eulerAngles.y, player.transform.eulerAngles.z);
    }
}
