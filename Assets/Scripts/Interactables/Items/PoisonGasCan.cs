using UnityEngine;
using Unity.Netcode;
using System.Collections;
using Unity.VisualScripting;

public class PoisonGasCan : Item {

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SpawnPoisonGasRpc() {
        // Vector3 spawnPos = Player.localPlayer.viewPosition.transform.position + (PlayerCamera.mainCamera.transform.forward * 0.2f) + (-PlayerCamera.mainCamera.transform.up * 0.2f);
        Vector3 spawnPos = Player.localPlayer.transform.Find("Armature/Hip/Spine/Upper Arm.R/Lower Arm.R/Hand.R/Hand.R_end").position;
        Debug.Log(spawnPos);
        Quaternion spawnRotation = PlayerCamera.mainCamera.transform.rotation;
        GameObject poisonGas = Instantiate(Assets.instance.poisonGasPrefab, spawnPos, spawnRotation);
        poisonGas.GetComponent<NetworkObject>().Spawn();
    }


    public override void UseItem() {
        SpawnPoisonGasRpc();
        // GameManager.PlayGlobalSoundEffectInWorld(Assets.SfxType.CrowbarSwing);
        if (humanPlayerRef.Value.TryGet(out NetworkObject playerObj)) {
            Crawl crawl = playerObj.GetComponent<Crawl>();
            HumanPlayer player = playerObj.GetComponent<HumanPlayer>();
            Animator animator = player.GetComponent<Animator>();
            player.isSpraying = true;
            animator.SetBool("isSpraying", true);
            StartCoroutine(SetIsSprayingDelay(player, false));
        }
    }

    public IEnumerator SetIsSprayingDelay(HumanPlayer player, bool state) {
        yield return new WaitForSeconds(cooldown);
        if (!Input.GetMouseButton(0)) {
            player.isSpraying = state;
            player.GetComponent<Animator>().SetBool("isSpraying", false);
        }
    }

    public override string GetInteractionPromptText() {
        return "Hold E to pick up poison spray can.";
    }
}