using UnityEngine;
using Unity.Netcode;

public class PoisonGasCan : Item
{

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SpawnPoisonGasRpc()
    {
        Vector3 spawnPos = Player.localPlayer.viewPosition.transform.position + (PlayerCamera.mainCamera.transform.forward * 0.5f) + (-PlayerCamera.mainCamera.transform.up * 0.2f);
        Debug.Log(spawnPos);
        Quaternion spawnRotation = PlayerCamera.mainCamera.transform.rotation;
        GameObject poisonGas = Instantiate(Assets.instance.poisonGasPrefab, spawnPos, spawnRotation);
        poisonGas.GetComponent<NetworkObject>().Spawn();
    }


    public override void UseItem()
    {
        SpawnPoisonGasRpc();
        // GameManager.PlayGlobalSoundEffectInWorld(Assets.SfxType.CrowbarSwing);
    }

    public override string GetInteractionPromptText()
    {
        return "Hold E to pick up poison spray can.";
    }
}