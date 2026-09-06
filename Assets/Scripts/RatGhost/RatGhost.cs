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

    void BecomeGhost() {
        playerRenderer = transform.Find("Renderer").GetComponent<SkinnedMeshRenderer>();
        playerRenderer.materials = Assets.instance.ratGhostMaterials;
        ratPlayer.isGhost = true;
        ghostShader.enabled = true;

        Assets.instance.objectivesUIGameObject.transform.parent.gameObject.SetActive(false);
        Assets.instance.ghostRulesUIGameObject.SetActive(true);

        ProgressManager.instance.RemoveAllObjectives();
    }
}
