using UnityEngine;
using TMPro;
using Unity.Netcode;
using System.Collections;
using Unity.Netcode.Components;
using System;

public class DeathHandler : NetworkBehaviour {
    bool isRagdolled = false;
    public void ToggleRagdoll(bool state) {
        isRagdolled = state;
        Rigidbody[] ragdollRigidbodies = GetComponentsInChildren<Rigidbody>(true);
        Collider[] ragdollColliders = GetComponentsInChildren<Collider>(true);
        Joint[] ragdollJoints = GetComponentsInChildren<Joint>(true);

        foreach (var rb in ragdollRigidbodies) {
            if (rb.gameObject != this.gameObject) {
                rb.isKinematic = !state;
                rb.useGravity = state;
            }
        }

        foreach (var col in ragdollColliders) {
            if (col.gameObject != this.gameObject) {
                col.enabled = state;
            }
        }

        foreach (var joint in ragdollJoints) {
            joint.enableCollision = state;
        }

        GetComponent<Collider>().enabled = !state;
        GetComponent<Animator>().enabled = !state;
        GetComponent<Rigidbody>().useGravity = !state;

        if (state == false) GetComponent<Animator>().Rebind();
    }

    public void KillPlayer() {
        if (IsServer) {
            RatPlayer ratPlayer = GetComponent<RatPlayer>();
            ratPlayer.EditHealthServerRpc(0);
            ratPlayer.SetDeadStateRpc(true);
            if (ratPlayer.isInvisible) {
                ratPlayer.GetComponent<RatInvisibilityAbility>().SetVisibleRpc();
            }

            // deduct score
            float scoreToDeduct = Mathf.Floor(0.2f * ratPlayer.score.Value);
            ratPlayer.EditScoreServerRpc((int)(ratPlayer.score.Value - scoreToDeduct));

            // drop coin
            if (ratPlayer.isCarryingCoin.Value) {
                NetworkObject coinObj = ratPlayer.coinBeingCarried.Value;
                Coin coin = coinObj.GetComponent<Coin>();
                coin.DropCoinRpc();
            }

            ratPlayer.lives.Value--;
            GameManager.PlayGlobalSoundEffectInWorld(Assets.SfxType.RatDie, transform.position);
        }
        ToggleRagdoll(true);
    }

    public void RevivePlayer() {
        Player player = GetComponent<Player>();

        if (IsServer) {
            player.EditHealthServerRpc(player.maxHealth.Value);
            player.SetDeadStateRpc(false);
        }

        if (!IsServer) return;
        TeleportPlayerClientRpc(RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void TeleportPlayerClientRpc(RpcParams rpcParams = default) {
        Rigidbody body = GetComponent<Rigidbody>();
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.position = Vector3.zero;

        transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        Physics.SyncTransforms();

        GetComponent<ClientNetworkTransform>().Teleport(Vector3.zero, Quaternion.identity, transform.localScale);
    }

    void Start() {
        Player player = GetComponent<Player>();
        player.onSpawn += () => ToggleRagdoll(false);
        player.onDeath += () => {
            if (!IsServer) return;
            KillPlayer();
            Timer.CreateTimer(Constants.respawnTime, Timer.OnFinish.DESTROY,
                () => { RevivePlayer(); GetComponent<Player>().onRevive?.Invoke(); }, "Rat Revival Timer");
        };
        player.dead.OnValueChanged += (bool oldState, bool newState) => {
            if (newState) ToggleRagdoll(true);
            else ToggleRagdoll(false);
        };
    }
}