using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class DeathHandler : NetworkBehaviour
{
    public void ToggleRagdoll(bool state) {
        Rigidbody[] ragdollRigidbodies = GetComponentsInChildren<Rigidbody>(true);
        Collider[] ragdollColliders = GetComponentsInChildren<Collider>(true);
        Joint[] ragdollJoints = GetComponentsInChildren<Joint>(true);

        foreach (var rb in ragdollRigidbodies)
        {
            if (rb.gameObject != this.gameObject) {
                rb.isKinematic = !state;
                rb.useGravity = state;
            }
        }

        foreach (var col in ragdollColliders)
        {
            if (col.gameObject != this.gameObject) {
                col.enabled = state;
            }
        }

        foreach (var joint in ragdollJoints)
        {
            joint.enableCollision = state;
        }

        GetComponent<Collider>().enabled = !state;
        GetComponent<Animator>().enabled = !state;
        GetComponent<Rigidbody>().useGravity = !state;

        if (state == false)  GetComponent<Animator>().Rebind();
    }

    public void KillPlayer() {
        if (IsServer) {
            GetComponent<Player>().EditHealthServerRpc(0);
            GetComponent<Player>().EditScoreServerRpc(0);
        }
        GetComponent<Player>().dead = true;
        ToggleRagdoll(true);
    }

    public void RevivePlayer() {
        Player player = GetComponent<Player>();
        
        if (IsServer) player.EditHealthServerRpc(player.maxHealth.Value);
        
        player.dead = false;
        ToggleRagdoll(false);
        
        GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        StartCoroutine(ForceResetRatPosition());
    }

    IEnumerator ForceResetRatPosition() {
        int resetFrameMaxCount = 1;
        for (int i = 0; i < resetFrameMaxCount; i++) {
            if (transform.position.magnitude > 0.1f) {
                resetFrameMaxCount++;
                Debug.Log(i);
            }

            transform.position = Vector3.zero;
            yield return null;
        }
    }

    void Start() {
        GetComponent<Player>().onSpawn += () => ToggleRagdoll(false);
        GetComponent<Player>().onDeath += () => {
            KillPlayer();
            Timer.CreateTimer(Constants.respawnTime, Timer.OnFinish.DESTROY, 
                () => { RevivePlayer(); GetComponent<Player>().onRevive?.Invoke(); }, "Rat Revival Timer");
        };
    }
}