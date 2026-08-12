using UnityEngine;
using Unity.Netcode;
using System.Collections;
using UnityEditor.U2D;

public class Weapon : Item {
    public CrowbarData data;

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
    }
    public override void UseItem() {
        Attack();
        GameManager.PlayGlobalSoundEffectInWorld(Assets.SfxType.CrowbarSwing);
    }

    public override string GetInteractionPromptText() {
        return "Hold E to pick up crowbar.";
    }

    public void Attack() {

        if (humanPlayerRef.Value.TryGet(out NetworkObject playerObj)) {
            Crawl crawl = playerObj.GetComponent<Crawl>();
            HumanPlayer player = playerObj.GetComponent<HumanPlayer>();
            if (crawl.isCrawling) {
                PlayerAnimator.instance.PlayAnimation("Crawl Swing", "isSwinging", 0.15f, 2);
            } else {
                PlayerAnimator.instance.PlayAnimation("Swing", "isSwinging", 0.15f, 1);
            }
            player.isSwinging = true;
            StartCoroutine(SetIsSwingingDelay(player, false));
        }
        Invoke("CheckPlayerCollision", attackDuration);
    }

    public IEnumerator SetIsSwingingDelay(HumanPlayer player, bool state) {
        yield return new WaitForSeconds(cooldown);
        player.isSwinging = state;
    }

    public void CheckPlayerCollision() {
        GameObject mainCamera = PlayerCamera.mainCamera;
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        LayerMask ignoreMask = ~LayerMask.GetMask("Hunter", "groundLayer", "Ignore Raycast");

        Debug.DrawRay(ray.origin, ray.direction * data.attackRange, Color.red, 5f);

        if (Physics.SphereCast(ray, data.rayRadius, out RaycastHit hit, data.attackRange)) Debug.Log(hit.collider.gameObject.name + ", " + hit.collider.gameObject.tag);
        if (Physics.SphereCast(ray, data.rayRadius, out hit, data.attackRange, ignoreMask)) {

            Debug.Log(hit.collider.gameObject.name + ", " + hit.collider.gameObject.tag);

            if (hit.collider.gameObject.tag == "PlayerMouse") {
                RatPlayer colliderRatScript = hit.collider.gameObject.GetComponent<RatPlayer>();
                colliderRatScript.EditHealthServerRpc(colliderRatScript.health.Value - data.damage);
            }
        }
    }
}
