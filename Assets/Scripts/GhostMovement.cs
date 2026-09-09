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

        if (Input.GetKeyDown(KeyCode.Z)) {
            Vector3 direction = Camera.main.transform.forward;
            itemBeingPossessed.GetComponent<Rigidbody>().AddForce(direction * 5, ForceMode.Impulse);
        }
        if (Input.GetKeyDown(KeyCode.Q)) {
            GetComponent<RatPlayer>().unPossessedItem.Invoke();
        }
    }
}