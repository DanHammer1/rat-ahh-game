using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Coin : NetworkBehaviour, IInteractable
{
    bool isBeingCarried = false;
    bool hasBeenDelivered = false;
    public void Update()
    {
        ((IInteractable)this).TryInteract();

        // If coin is being carried:
        if (Player.localPlayer && Player.localPlayer.isCarryingCoin && isBeingCarried)
        {
            // Lock position to rats spine
            Transform spine = Player.localPlayer.transform.Find("Armature/Hip/Spine");
            transform.position = spine.TransformPoint(new Vector3(0.005f, 0, 0));
            transform.rotation = spine.rotation * Quaternion.Euler(0, 0, 90);

            // Drop coin
            if (Input.GetKeyDown(KeyCode.Q))
            {
                isBeingCarried = false;
                Player.localPlayer.isCarryingCoin = false;
                transform.position = Player.localPlayer.transform.TransformPoint(new Vector3(0, 2f, 3f));
                transform.SetParent(GameObject.Find("Coin Container").transform);
                this.GetComponent<BoxCollider>().enabled = true;
                this.GetComponent<Rigidbody>().useGravity = true;
            }
        }
    }
    public string GetInteractionPromptText()
    {
        return Player.localPlayer.isCarryingCoin ? "Already carrying coin" : "Press E to pick up coin";
    }

    public void Interact()
    {
        if (!Player.localPlayer.isCarryingCoin)
        {
            transform.SetParent(Player.localPlayer.transform);
            Player.localPlayer.isCarryingCoin = true;
            this.GetComponent<BoxCollider>().enabled = false;
            this.GetComponent<Rigidbody>().useGravity = false;
            isBeingCarried = true;
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

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CoinDeliveryLocation"))
        {
            Player.localPlayer.score += ObjectiveScores.deliveryScore;
            Player.localPlayer.scoreText.text = $"Score: {Player.localPlayer.score}";
            Debug.Log("Coin delivered");
        }
    }
}







