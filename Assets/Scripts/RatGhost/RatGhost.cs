using Unity.Netcode;
using UnityEngine;

public class RatGhost : NetworkBehaviour {
    SkinnedMeshRenderer playerRenderer;
    RatPlayer ratPlayer;
    GhostShader ghostShader;

    public override void OnNetworkSpawn() {
        ratPlayer = GetComponent<RatPlayer>();
        ghostShader = GetComponent<GhostShader>();
        ratPlayer.onRevive += () => { if (ratPlayer.lives.Value == 0) BecomeGhost(); };
    }

    // void Update() {
    //     if (Input.GetKeyDown(KeyCode.K)) {
    //         BecomeGhost();
    //     }
    // }

    void BecomeGhost() {
        BecomeGhostRpc();
        BecomeGhostClientRpc();
    }

    [Rpc(SendTo.Owner)]
    void BecomeGhostClientRpc() {
        Assets.instance.objectivesUIGameObject.transform.parent.gameObject.SetActive(false);
        Assets.instance.ghostRulesUIGameObject.SetActive(true);
        Assets.instance.abilityParent.SetActive(false);
        Assets.instance.tauntsUI.SetActive(false);
        ProgressManager.instance.RemoveAllObjectives();
    }

    [Rpc(SendTo.Everyone)]
    void BecomeGhostRpc() {
        playerRenderer = transform.Find("Renderer").GetComponent<SkinnedMeshRenderer>();
        playerRenderer.materials = Assets.instance.ratGhostMaterials;
        ratPlayer.isGhost = true;
        ghostShader.enabled = true;
        GetComponent<PoisonGasDamage>().enabled = false;
        GetComponent<RatClingAbility>().isEnabled = false;
        GetComponent<RatDashAbility>().isEnabled = false;
        GetComponent<RatInvisibilityAbility>().isEnabled = false;
    }
}
