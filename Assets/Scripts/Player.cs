using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    public static Player localPlayer;
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        localPlayer = this;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {

    }
}
