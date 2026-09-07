using UnityEngine;
using Unity.Netcode;

public class GhostMovement : NetworkBehaviour {
    public Transform itemBeingPossessed;

    void FixedUpdate() {
        // if (Player.localPlayer != null) {
        //     eyePosition = Player.localPlayer.gameObject.transform.Find("EyePosition");
        // }

        if (!IsOwner || Player.localPlayer.dead || !itemBeingPossessed) return;

        transform.position = itemBeingPossessed.position;

        if (Input.GetKeyDown(KeyCode.Y)) {
            itemBeingPossessed.GetComponent<Rigidbody>().AddForce(Vector3.up * 0.1f, ForceMode.Impulse);
        }
    }
}