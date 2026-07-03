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

    [SerializeField] AudioSource SFXSource;
    public AudioClip swingHit;
    public AudioClip swingMiss;

    // Update is called once per frame
    public virtual void Update()
    {
        CheckInput();
    }

    public void CheckInput()
    {
        if (!IsOwner) return;

        cooldownTimer -= Time.deltaTime;

        if (Input.GetMouseButtonDown(0) && cooldownTimer < 0)
        {
            cooldownTimer = cooldown;
            Attack();
        }
    }

    public void Attack()
    {
        PlayerAnimator.instance.PlayAnimation("Swing", "isSwinging", 0.15f);
        Invoke("CheckPlayerCollision", attackDuration);
    }

    public void CheckPlayerCollision()
    {
        GameObject mainCamera = PlayerCamera.mainCamera;
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        LayerMask ignoreMask = ~LayerMask.GetMask("Hunter", "groundLayer");

        Debug.DrawRay(ray.origin, ray.direction * attackRange, Color.red, 5f);

        if (Physics.SphereCast(ray, rayRadius, out RaycastHit hit, attackRange)) Debug.Log(hit.collider.gameObject.name + ", " + hit.collider.gameObject.tag);
        if (Physics.SphereCast(ray, rayRadius, out hit, attackRange, ignoreMask))
        {

            Debug.Log(hit.collider.gameObject.name + ", " + hit.collider.gameObject.tag);

            if (hit.collider.gameObject.tag == "PlayerMouse")
            {
                RatPlayer colliderRatScript = hit.collider.gameObject.GetComponent<RatPlayer>();
                colliderRatScript.EditHealthServerRpc(colliderRatScript.health.Value - damage);

                // temp sound effect
                SFXSource.clip = swingHit;
                SFXSource.Play();
            }
        }
        else // also temp sound effect
        {
            SFXSource.clip = swingMiss;
            SFXSource.Play();
        }
    }
}
