using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Coin : NetworkBehaviour, IInteractable
{
    public NetworkVariable<bool> isBeingCarried = new NetworkVariable<bool>(false);
    public NetworkVariable<NetworkObjectReference> playerCarryingCoin = new NetworkVariable<NetworkObjectReference>();
    bool hasBeenDelivered = false;

    private float pickUpProgress = 0;
    private float totalInteractionTime = 0.8f;

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SetCoinParentRpc(NetworkObjectReference parentRef)
    {
        if (parentRef.TryGet(out NetworkObject parent))
        {
            NetworkObject.TrySetParent(parent);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ToggleIsBeingCarriedRpc()
    {
        isBeingCarried.Value = !isBeingCarried.Value;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SetPlayerCarryingCoinRpc(NetworkObjectReference playerReference)
    {
        playerCarryingCoin.Value = playerReference;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void DropCoinRpc()
    {
        NetworkObject player;
        playerCarryingCoin.Value.TryGet(out player);

        ToggleIsBeingCarriedRpc();
        player.transform.GetComponent<Player>().ToggleIsCarryingCoinRpc();
        // player.transform.GetComponent<Player>().ToggleIsCarryingCoinClientRpc();
        transform.position = player.transform.TransformPoint(new Vector3(0, 2f, 3f));
        SetCoinParentRpc(GameObject.Find("Coin Container").GetComponent<NetworkObject>());
        ToggleBoxColliderClientRpc();
        ToggleRigidbodyGravityClientRpc();
        player.transform.GetComponent<Movement>().MultiplyMoveSpeedRpc(1 / Constants.carryingCoinMoveSpeedMultiplier);

        Debug.Log(Player.localPlayer.isCarryingCoin.Value);
        pickUpProgress = 0;
    }

    public void OnInteractingExit()
    {
        pickUpProgress = 0;
    }

    public bool CheckExtraInteractionConditions()
    {
        return (GameManager.GetLocalRole() == GameManager.PlayerRole.HIDER);
    }

    public void Update()
    {
        ((IInteractable)this).TryInteract();

        // If coin is being carried:
        if (!(Player.localPlayer && isBeingCarried.Value)) return;

        NetworkObject player;
        playerCarryingCoin.Value.TryGet(out player);

        Transform spine = player.transform.Find("Armature/Hip/Spine");
        transform.position = spine.TransformPoint(new Vector3(0.005f, 0, 0));
        transform.rotation = spine.rotation * Quaternion.Euler(0, 0, 90);

        if (Player.localPlayer.NetworkObject == player && Input.GetKeyDown(KeyCode.Q))
        {
            DropCoinRpc();
        }
    }

    public string GetInteractionPromptText()
    {
        return Player.localPlayer.isCarryingCoin.Value ? "Already carrying coin" : "Hold E to pick up coin";
    }

    public void Interact()
    {
        // Pickup coin
        if (!Player.localPlayer.isCarryingCoin.Value)
        {
            SetCoinParentRpc(Player.localPlayer.GetComponent<NetworkObject>());
            Player.localPlayer.ToggleIsCarryingCoinRpc();
            // Player.localPlayer.ToggleIsCarryingCoinClientRpc();
            ToggleBoxColliderClientRpc();
            ToggleRigidbodyGravityClientRpc();
            ToggleIsBeingCarriedRpc();
            SetPlayerCarryingCoinRpc(Player.localPlayer.gameObject);
            Player.localPlayer.transform.GetComponent<Movement>().MultiplyMoveSpeedRpc(Constants.carryingCoinMoveSpeedMultiplier);
            Debug.Log(Player.localPlayer.isCarryingCoin.Value);
        }
    }

    [ClientRpc]
    private void ToggleRigidbodyGravityClientRpc()
    {
        this.GetComponent<Rigidbody>().useGravity = !this.GetComponent<Rigidbody>().useGravity;
    }
    [ClientRpc]
    private void ToggleBoxColliderClientRpc()
    {
        this.GetComponent<BoxCollider>().enabled = !this.GetComponent<BoxCollider>().enabled;
    }


    public void UpdateProgress()
    {
        if (!Player.localPlayer.isCarryingCoin.Value) pickUpProgress += Time.deltaTime / totalInteractionTime;
    }

    public float GetProgress()
    {
        return pickUpProgress;
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
            Player.localPlayer.EditScoreServerRpc(Player.localPlayer.score.Value + ObjectiveScores.deliveryScore);
            Debug.Log("Coin delivered");
        }
    }
}







