using UnityEngine;

public class DeathHandler : MonoBehaviour
{
    public void ToggleRagdoll(bool state) {

    }

    void Start() {
        gameObject.GetComponent<Player>().onDeath += () => ToggleRagdoll(true);
    }
}
