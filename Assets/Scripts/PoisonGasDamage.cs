using UnityEngine;
using Unity.Netcode;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime;
using System.Collections.Generic;

public class PoisonGasDamage : NetworkBehaviour {
    public bool isPoisoned = false;
    public int poisonZonesCount = 0;
    float poisonTimer = 0f;
    float poisonInterval = 0.5f;
    public RatPlayer ratPlayer;



    void Update() {
        if (!IsServer) return;

        isPoisoned = poisonZonesCount >= 1;

        if (ratPlayer.health.Value <= 0) {
            isPoisoned = false;
            poisonZonesCount = 0;
        }
        Debug.Log(poisonZonesCount);

        if (isPoisoned && ratPlayer.health.Value > 0) {
            poisonTimer += Time.deltaTime;
            if (poisonTimer >= poisonInterval) {
                ratPlayer.EditHealthServerRpc(ratPlayer.health.Value - (Constants.poisonDPS * poisonInterval));
                int randomSFX = Random.Range(1, 4);
                if (randomSFX == 1) GameManager.PlayGlobalSoundEffectInWorld(Assets.SfxType.poisonDamage1, ratPlayer.transform.position);
                else if (randomSFX == 2) GameManager.PlayGlobalSoundEffectInWorld(Assets.SfxType.poisonDamage2, ratPlayer.transform.position);
                else if (randomSFX == 3) GameManager.PlayGlobalSoundEffectInWorld(Assets.SfxType.poisonDamage3, ratPlayer.transform.position);
                poisonTimer = 0f;
            }
        } else {
            poisonTimer = 0f;
        }
    }
}