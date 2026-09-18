using UnityEngine;
using Unity.Netcode;
using TMPro;

public class HunterReleasing : NetworkBehaviour {
    NetworkVariable<float> timeRemaining = new NetworkVariable<float>(Constants.huntersReleasingTime);
    float elapsedTime;

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        timeRemaining.OnValueChanged += UpdateHuntersReleasingRpc;
        if (!IsServer) return;
    }

    [Rpc(SendTo.Everyone)]
    public void UpdateHuntersReleasingRpc(float oldValue, float newValue) {
        GameObject huntersReleasingNotice = GameObject.FindWithTag("HuntersReleasingNotice");
        if (newValue == 0) {
            Door door = Assets.instance.hunterReleaseDoor.GetComponent<Door>();
            door.enabled = true;
            if (IsServer) door.Interact();
            Destroy(gameObject);
        }
        if (huntersReleasingNotice != null) {
            huntersReleasingNotice.transform.Find("HuntersReleasingText").GetComponent<TextMeshProUGUI>().text = $"HUNTERS ARRIVE IN {newValue}...";
        }
    }

    void Update() {
        if (!IsServer) return;

        elapsedTime += Time.deltaTime;
        if (elapsedTime >= 1f) {
            timeRemaining.Value = Mathf.Max(0f, timeRemaining.Value - 1f);
            elapsedTime -= 1f;
        }
    }
}
