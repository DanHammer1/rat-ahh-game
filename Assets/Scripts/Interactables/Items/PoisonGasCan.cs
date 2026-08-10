using UnityEngine;
using Unity.Netcode;

public class PoisonGasCan : NetworkBehaviour
{
    float spawnTimer = 0f;
    float spawnInterval = 0.03f;


    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                SpawnPoisonGasRpc();
                spawnTimer = 0f;
            }
        }
        else
        {
            spawnTimer = 0f;
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SpawnPoisonGasRpc()
    {
        Vector3 spawnPos = Player.localPlayer.viewPosition.transform.position + (PlayerCamera.mainCamera.transform.forward * 0.5f) + (-PlayerCamera.mainCamera.transform.up * 0.2f);
        Debug.Log(spawnPos);
        Quaternion spawnRotation = PlayerCamera.mainCamera.transform.rotation;
        GameObject poisonGas = Instantiate(Assets.instance.poisonGasPrefab, spawnPos, spawnRotation);
        poisonGas.GetComponent<NetworkObject>().Spawn();
    }
}
