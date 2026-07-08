using UnityEngine;

public class DeathHandler : MonoBehaviour
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

    void Start() {
        GetComponent<Player>().onSpawn += () => ToggleRagdoll(false);
        GetComponent<Player>().onDeath += () => ToggleRagdoll(true);
    }
}
