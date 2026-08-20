using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
using TMPro;
using System.Collections;
using UnityEditor;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using ParrelSync.NonCore;
using UnityEditor.Search;

public class RatClingAbility : Ability {
    Transform clingHead;
    public bool ratAbilityInRange;
    private HunterPlayer localHunterInRange;
    public NetworkVariable<bool> isClinging;
    public bool isSlapping;
    public float ratAbilityHunterStunDuration;
    public float ratAbilityHunterShakeMeter;
    protected GameObject ratAbilityShakeUI;
    BoxCollider boxCollider;

    public override Sprite GetIconSprite() {
        return Assets.instance.ratClingAbilityIcon;
    }

    public override float GetAbilityCooldown() {
        return Constants.maxRatAbilityCooldown;
    }

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();

        boxCollider = GetComponent<BoxCollider>();

        if (!IsOwner) return;

        ratAbilityInRange = false;
        ratAbilityShakeUI = Assets.instance.ratAbilityShakeUI;
        ratAbilityShakeUI.SetActive(false);
        Assets.instance.ratAbilitySlapPrompt?.SetActive(false);

        abilityTimer.AddProgressionCondition(() => !GetComponent<Movement>().isPerformingAbility);
    }

    void OnTriggerStay(Collider other) {
        if (transform.tag == "PlayerMouse" && other.CompareTag("Rat Stun Hitbox")) {
            HunterPlayer hunterPlayer = other.GetComponentInParent<HunterPlayer>();
            localHunterInRange = hunterPlayer;

            if (IsOwner) {
                ratAbilityInRange = true;
            }
        }
    }

    void OnTriggerExit(Collider other) {
        if (IsOwner && transform.tag == "PlayerMouse" && other.CompareTag("Rat Stun Hitbox")) {
            ratAbilityInRange = false;

            if (!GetComponent<Movement>().isPerformingAbility) {
                localHunterInRange = null;
            }
        }
    }

    public override void ExecuteAbility() {
        if (localHunterInRange == null) return; //safety check
        StartCoroutine(RatAbilityCoroutine());
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void SetClingingStateRpc(bool state) {
        isClinging.Value = state;
    }

    IEnumerator RatAbilityCoroutine() {
        Movement movement = GetComponent<Movement>();
        isSlapping = false;
        SetClingingStateRpc(false);

        Vector3 startPos = transform.position;
        Vector3 targetPos = localHunterInRange.ratAbilityTarget.transform.position;

        movement.isPerformingAbility = true; // prevents movement during ability

        float ratAbilityDuration = Constants.ratAbilityDuration;
        float elapsed = 0;

        bool forceApplied = false;

        PlayerCamera.instance.ForceLookAt(targetPos, startPos);

        Rigidbody rb = movement.GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        float originalDrag = rb.linearDamping;

        movement.isGrounded = false;
        movement.pressedSpace = true;

        Vector3 forceToAdd = Vector3.zero;
        forceToAdd.x = (targetPos.x - startPos.x) / ratAbilityDuration;
        forceToAdd.y = (((targetPos.y - startPos.y) - Physics.gravity.y / 2 * Mathf.Pow(ratAbilityDuration, 2)) / ratAbilityDuration) / 1.18f;
        forceToAdd.z = (targetPos.z - startPos.z) / ratAbilityDuration;

        if (!forceApplied) {
            rb.linearDamping = 0;
            rb.AddForce(forceToAdd * rb.mass, ForceMode.Impulse);
            forceApplied = true;
        }

        while (elapsed < ratAbilityDuration) {
            float t = elapsed / ratAbilityDuration;
            elapsed += Time.fixedDeltaTime;

            if (Vector3.Distance(transform.position, targetPos) <= Constants.ratAbilityClingRange) {
                SetColliderStateRpc(false);
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.linearDamping = originalDrag;
                movement.toggleGravity = false;
                clingHead = localHunterInRange.movement.headBone;

                rb.useGravity = false;
                rb.detectCollisions = false;
                UpdateHunterSlapCountServerRpc(localHunterInRange.NetworkObjectId, 0, "Set");

                SetClingingStateRpc(true);
                Assets.instance.ratAbilitySlapPrompt.SetActive(true);
                break;
            }
            yield return new WaitForFixedUpdate();
        }
        Debug.DrawLine(transform.position, targetPos, Color.red, 3f);
    }

    [Rpc(SendTo.Everyone)]
    public void SetColliderStateRpc(bool state) {
        boxCollider.enabled = state;
    }

    void UnCling() {
        Rigidbody rb = GetComponent<Rigidbody>();
        Movement movement = GetComponent<Movement>();

        SetColliderStateRpc(true);
        movement.toggleGravity = true;

        rb.useGravity = true;
        rb.detectCollisions = true;

        SetClingingStateRpc(false);
        movement.isPerformingAbility = false;
        Assets.instance.ratAbilitySlapPrompt.SetActive(false);


        SetHunterClingStateServerRpc(localHunterInRange.NetworkObjectId, false);
        SetHunterDizzyStateServerRpc(localHunterInRange.NetworkObjectId, true);
    }

    void OnMiss() {
        Rigidbody rb = GetComponent<Rigidbody>();
        Movement movement = GetComponent<Movement>();

        SetColliderStateRpc(true);
        movement.toggleGravity = true;

        rb.useGravity = true;
        rb.detectCollisions = true;

        movement.isPerformingAbility = false;
    }

    void OnCollisionEnter(Collision collision) {
        Movement movement = GetComponent<Movement>();

        if (movement.isPerformingAbility && collision.gameObject.layer == LayerMask.NameToLayer("groundLayer")) {
            OnMiss();
        }
    }

    public override bool CheckAbilityExecutable() {
        return (ratAbilityInRange);
    }

    protected override void Update() {
        base.Update();
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Q) && isClinging.Value) {
            isSlapping = !isSlapping;
            UpdateHunterSlapCountServerRpc(localHunterInRange.NetworkObjectId, 1, "Add");
        }
        if (Input.GetKeyDown(KeyCode.U) && isClinging.Value) {
            UnCling();
        }

        if (isClinging.Value && IsOwner) {
            clingHead = localHunterInRange.movement.headBone;
            HunterPlayer hunterPlayer = localHunterInRange.GetComponent<HunterPlayer>();
            SetHunterClingStateServerRpc(localHunterInRange.NetworkObjectId, true);
            localHunterInRange.CheckJustGotClung(true);
            transform.position =
                clingHead.position +
                clingHead.TransformDirection(Vector3.forward * 0.1f) +
                clingHead.TransformDirection(Vector3.down * 0.02f);
            SetViewPositionServerRpc(localHunterInRange.NetworkObjectId, localHunterInRange.ratAbilityTarget.transform.position);


            Quaternion flip = Quaternion.Euler(0, 180f, 0);
            transform.rotation = clingHead.rotation * flip;
            Debug.DrawRay(clingHead.position, clingHead.forward * 0.5f, Color.blue);
            Debug.DrawRay(clingHead.position, clingHead.up * 0.5f, Color.green);
            Debug.DrawRay(clingHead.position, clingHead.right * 0.5f, Color.red);
            if (hunterPlayer.ratAbilityHunterShakeMeter.Value >= Constants.maxRatAbilityHunterShakeMeter) {
                UnCling();
            }
        }
    }

    public void FixedUpdate() {
        if (isClinging.Value && IsServer) {
            HunterPlayer hunterPlayer = localHunterInRange.GetComponent<HunterPlayer>();
        }

    }

    [ServerRpc]
    void SetViewPositionServerRpc(ulong hunterNetworkId, Vector3 pos) {
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(hunterNetworkId, out NetworkObject netObj)) {
            Debug.Log("Hunter not found on server");
            return;
        }

        HunterPlayer hunter = netObj.GetComponent<HunterPlayer>();

        if (hunter == null || hunter.viewPosition == null) {
            Debug.Log("Hunter or viewPosition missing");
            return;
        }

        hunter.viewPosition.transform.position = pos;
    }

    [ServerRpc]
    void SetHunterClingStateServerRpc(ulong hunterId, bool state) {
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(hunterId, out NetworkObject netObj)) {
            HunterPlayer hunter = netObj.GetComponent<HunterPlayer>();
            if (hunter != null) {
                hunter.isBeingClung.Value = state;
            }
        }
    }

    void IncreaseHunterShakeMeterValue(ulong hunterId) {
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(hunterId, out NetworkObject netObj)) {
            HunterPlayer hunter = netObj.GetComponent<HunterPlayer>();
            if (hunter != null) {
                hunter.ratAbilityHunterShakeMeter.Value += Time.fixedDeltaTime;
            }
        }
    }

    [ServerRpc]
    void SetHunterShakeMeterValueServerRpc(ulong hunterId, float value) {
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(hunterId, out NetworkObject netObj)) {
            HunterPlayer hunter = netObj.GetComponent<HunterPlayer>();
            if (hunter != null) {
                hunter.ratAbilityHunterShakeMeter.Value = value;
            }
        }
    }

    [ServerRpc]
    void UpdateHunterSlapCountServerRpc(ulong hunterId, int value, string addOrSet) {
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(hunterId, out NetworkObject netObj)) {
            HunterPlayer hunter = netObj.GetComponent<HunterPlayer>();
            if (hunter != null) {
                if (addOrSet == "Add") {
                    hunter.slapCount.Value += value;
                }
                if (addOrSet == "Set") {
                    hunter.slapCount.Value = value;
                }
            }
        }
    }

    [ServerRpc]
    void SetHunterDizzyStateServerRpc(ulong hunterId, bool state) {
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(hunterId, out NetworkObject netObj)) {
            HunterPlayer hunter = netObj.GetComponent<HunterPlayer>();
            if (hunter != null) {
                hunter.isDizzy.Value = state;
            }
        }
    }

    void OnDrawGizmos() {
        if (transform.position != null) {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, 0.01f);
        }
    }
}