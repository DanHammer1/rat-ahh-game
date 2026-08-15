using UnityEngine;
using Unity.Netcode;
using System.Collections;
using Unity.VisualScripting;

public class PoisonGasCan : Item {

    Vector3 spawnPos;

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SpawnPoisonGasRpc(Quaternion cameraRotation) {
        if (hunterPlayerRef.Value.TryGet(out NetworkObject playerObj)) {
            spawnPos = playerObj.transform.Find("Armature/Hip/Spine/Upper Arm.R/Lower Arm.R/Hand.R/Hand.R_end").position;
        }
        GameObject poisonGas = Instantiate(Assets.instance.poisonGasPrefab, spawnPos, cameraRotation);
        poisonGas.GetComponent<NetworkObject>().Spawn();
    }


    public override void UseItem() {
        SpawnPoisonGasRpc(PlayerCamera.mainCamera.transform.rotation);
        // GameManager.PlayGlobalSoundEffectInWorld(Assets.SfxType.CrowbarSwing);
        if (hunterPlayerRef.Value.TryGet(out NetworkObject playerObj)) {
            Crawl crawl = playerObj.GetComponent<Crawl>();
            HunterPlayer player = playerObj.GetComponent<HunterPlayer>();
            Animator animator = player.GetComponent<Animator>();
            player.isSpraying = true;
            animator.SetBool("isSpraying", true);
            StartCoroutine(SetIsSprayingDelay(player, false));
        }
    }

    public IEnumerator SetIsSprayingDelay(HunterPlayer player, bool state) {
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