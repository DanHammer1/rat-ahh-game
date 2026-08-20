using UnityEngine;
using Unity.Netcode;
using System.Collections;
public class Taunt : NetworkBehaviour {
    public KeyCode hotkey;
    public float cooldown;
    private bool playable = true;
    public enum TauntType {
        Soft,
        Medium,
        Loud
    }

    public TauntType tauntType;

    void Update() {
        if (!IsOwner) return;

        if (Input.GetKeyDown(hotkey) && playable && !GetComponent<Player>().isInUIMenu) {
            playable = false;
            StartCoroutine(ReactivateTaunt());
            GameManager.PlayGlobalSoundEffectInWorld(GetSfxFromTaunt(tauntType));

            if (tauntType == TauntType.Loud) {
                ObjectManager.MakeObjectSpectralForEveryone(Player.localPlayer.gameObject);
                StartCoroutine(DeactivateGlow());
            }
        }
    }

    private IEnumerator ReactivateTaunt() {
        yield return new WaitForSeconds(cooldown);
        playable = true;
    }

    private IEnumerator DeactivateGlow() {
        yield return new WaitForSeconds(5);
        ObjectManager.TakeObjectSpectralForEveryone(Player.localPlayer.gameObject);
    }

    public Assets.SfxType GetSfxFromTaunt(TauntType taunt) {
        Assets.SfxType sfx = taunt switch {
            TauntType.Soft => Assets.SfxType.ratTauntSoft,
            TauntType.Medium => Assets.SfxType.ratTauntMedium,
            TauntType.Loud => Assets.SfxType.ratTauntLoud,
            _ => default
        };

        return sfx;
    }
}
