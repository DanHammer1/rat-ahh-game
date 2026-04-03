using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Cheese : NetworkBehaviour
{

    // Update is called once per frame

    public bool playerInRange = false;
    private Player localPlayerInRange;


    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && localPlayerInRange != null)
        {
            localPlayerInRange.eatCheesePrompt.SetActive(false);
            localPlayerInRange.score += ObjectiveScores.cheeseScore;
            localPlayerInRange.scoreText.text = $"Score: {localPlayerInRange.score}";
            DespawnServerRpc();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerMouse"))
        {
            Player player = other.GetComponentInParent<Player>();
            localPlayerInRange = player;

            if (player != Player.localPlayer) return;

            playerInRange = true;
            localPlayerInRange.eatCheesePrompt.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerMouse"))
        {
            Player player = other.GetComponentInParent<Player>();
            localPlayerInRange = player;

            if (player != Player.localPlayer) return;

            playerInRange = false;
            localPlayerInRange.eatCheesePrompt.SetActive(false);

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







