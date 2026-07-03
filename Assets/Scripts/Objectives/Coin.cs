using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Coin : NetworkBehaviour
{

    // Update is called once per frame

    public bool playerInRange = false;
    private RatPlayer localPlayerInRange;


    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && localPlayerInRange != null)
        {
            localPlayerInRange.pickUpCoinPrompt.SetActive(false);
            localPlayerInRange.score += ObjectiveScores.deliveryScore;
            localPlayerInRange.scoreText.text = $"Score: {localPlayerInRange.score}";
            DespawnServerRpc();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerMouse"))
        {
            RatPlayer player = other.GetComponentInParent<RatPlayer>();
            localPlayerInRange = player;

            if (player != Player.localPlayer) return;

            playerInRange = true;
            localPlayerInRange.pickUpCoinPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerMouse"))
        {
            RatPlayer player = other.GetComponentInParent<RatPlayer>();
            localPlayerInRange = player;

            if (player != Player.localPlayer) return;

            playerInRange = false;
            localPlayerInRange.pickUpCoinPrompt.SetActive(false);

            localPlayerInRange = null;
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void DespawnServerRpc()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }
}







