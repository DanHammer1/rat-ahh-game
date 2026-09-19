using UnityEngine;
using TMPro;
using Unity.Netcode;
using System.Collections;
using Unity.Netcode.Components;
using System;
using Unity.VisualScripting;

public class DeathHandler : NetworkBehaviour {
    bool isRagdolled = false;
    RatPlayer ratPlayer;
    Timer respawnTimer;
    NetworkVariable<float> respawnTimeRemaining = new NetworkVariable<float>(Constants.respawnTime);
    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        ratPlayer = GetComponent<RatPlayer>();
    }
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

            ratPlayer.lives.Value = Mathf.Max(0, ratPlayer.lives.Value - 1);
            GameManager.PlayGlobalSoundEffectInWorld(Assets.SfxType.RatDie, transform.position);
            ActivateRespawnPromptClientRpc(true, RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
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
        ActivateRespawnPromptClientRpc(false, RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
        respawnTimeRemaining.Value = Constants.respawnTime;
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void ActivateRespawnPromptClientRpc(bool state, RpcParams rpcParams = default) {
        Assets.instance.respawnPrompt.SetActive(state);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void TeleportPlayerClientRpc(RpcParams rpcParams = default) {
        Rigidbody body = GetComponent<Rigidbody>();
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.position = Vector3.zero;

        GameObject respawnLocation = Assets.instance.ratNests.transform.GetChild(UnityEngine.Random.Range(1, Assets.instance.ratNests.transform.childCount)).gameObject;
        Vector3 respawnPos = respawnLocation.transform.position;
        respawnPos += respawnLocation.transform.forward * 0.2f;
        respawnPos += new Vector3(UnityEngine.Random.Range(-0.02f, 0.02f), 0, UnityEngine.Random.Range(-0.02f, 0.02f));
        Quaternion respawnRotation = respawnLocation.transform.rotation;

        transform.SetPositionAndRotation(respawnPos, respawnRotation);
        Physics.SyncTransforms();

        GetComponent<ClientNetworkTransform>().Teleport(respawnPos, respawnRotation, transform.localScale);
        PlayerCamera.instance.SetCameraRotation(respawnRotation.eulerAngles.y);
        if (PlayerCamera.instance.cameraState == PlayerCamera.CameraState.ThirdPerson) {
            PlayerCamera.instance.thirdPersonRadius = Constants.ratMaxCameraThirdPersonRadius / 2;
        }
    }

    void Start() {
        Player player = GetComponent<Player>();
        player.onSpawn += () => ToggleRagdoll(false);
        player.onDeath += () => {
            if (!IsServer) return;
            KillPlayer();
            respawnTimer = Timer.CreateTimer(Constants.respawnTime, Timer.OnFinish.DESTROY,
                () => { RevivePlayer(); GetComponent<Player>().onRevive?.Invoke(); }, "Rat Revival Timer").GetComponent<Timer>();
        };
        player.dead.OnValueChanged += (bool oldState, bool newState) => {
            if (newState) ToggleRagdoll(true);
            else ToggleRagdoll(false);
        };
    }

    void Update() {
        if (IsServer && ratPlayer.dead.Value) {
            respawnTimeRemaining.Value -= Time.deltaTime;
        }

        if (!IsOwner) return;
        if (ratPlayer.dead.Value) {
            Assets.instance.respawnPrompt.transform.Find("RespawnCountdown").GetComponent<TextMeshProUGUI>().text = $"RESPAWNING IN {Math.Ceiling(respawnTimeRemaining.Value)}";
        }
    }
}