using UnityEngine;
using Unity.Netcode;

public class Weapon : NetworkBehaviour
{
    public float cooldown;
    public float attackDuration;
    public float damage;
    public float rayRadius;
    public float attackRange;

    float cooldownTimer;

    // Update is called once per frame
    public virtual void Update()
    {
        CheckInput();
    }

    public void CheckInput() {
        if (!IsOwner) return;

        cooldownTimer -= Time.deltaTime;

        if (Input.GetMouseButtonDown(0) && cooldownTimer < 0) {
            cooldownTimer = cooldown;
            Attack();
        }
    }

    public void Attack() {
        PlayerAnimator.PlayAnimation("Swing", "isSwinging", 0.15f);
        Invoke("CheckPlayerCollision", attackDuration);
    }

    public void CheckPlayerCollision() {
        GameObject mainCamera = PlayerCamera.mainCamera;
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);

        if (Physics.SphereCast(ray, rayRadius, out RaycastHit hit, attackRange)) {
            if (hit.collider.gameObject.tag == "Rat") {
                RatPlayer colliderRatScript = hit.collider.gameObject.GetComponent<RatPlayer>();
                colliderRatScript.EditHealthServerRpc(colliderRatScript.health.Value - damage);
            }
        }
    }
}
