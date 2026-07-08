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
        ToggleIsBeingCarriedRpc();
        Player.localPlayer.ToggleIsCarryingCoinRpc();
        Player.localPlayer.ToggleIsCarryingCoinClientRpc();
        transform.position = Player.localPlayer.transform.TransformPoint(new Vector3(0, 2f, 3f));
        SetCoinParentRpc(GameObject.Find("Coin Container").GetComponent<NetworkObject>());
        this.GetComponent<BoxCollider>().enabled = true;
        this.GetComponent<Rigidbody>().useGravity = true;
        Player.localPlayer.transform.GetComponent<Movement>().MultiplyMoveSpeedRpc(1 / Constants.carryingCoinMoveSpeedMultiplier);

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

        if (Input.GetKeyDown(KeyCode.Q))
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
            Player.localPlayer.ToggleIsCarryingCoinClientRpc();
            this.GetComponent<BoxCollider>().enabled = false;
            this.GetComponent<Rigidbody>().useGravity = false;
            ToggleIsBeingCarriedRpc();
            SetPlayerCarryingCoinRpc(Player.localPlayer.gameObject);
            Player.localPlayer.transform.GetComponent<Movement>().MultiplyMoveSpeedRpc(Constants.carryingCoinMoveSpeedMultiplier);
        }
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
            Player.localPlayer.score += ObjectiveScores.deliveryScore;
            Player.localPlayer.scoreText.text = $"Score: {Player.localPlayer.score}";
            Debug.Log("Coin delivered");
        }
    }
}







