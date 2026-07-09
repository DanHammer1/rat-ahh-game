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

public class RatClingAbility : Ability
{
    Transform clingHead;
    public bool ratAbilityInRange;
    private HumanPlayer localHumanInRange;
    public NetworkVariable<bool> isClinging;
    public bool isSlapping;
    public float ratAbilityHumanStunDuration;
    public float ratAbilityHumanShakeMeter;
    protected GameObject ratAbilityShakeUI;
    BoxCollider boxCollider;

    public override Sprite GetIconSprite() {
        return Constants.instance.RatClingAbilityIcon;
    }

    public override float GetAbilityCooldown() {
        return Constants.maxRatAbilityCooldown;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        ratAbilityInRange = false;
        ratAbilityShakeUI = GameObject.FindWithTag("Rat Ability Shake UI");
        ratAbilityShakeUI.SetActive(false);
        boxCollider = GetComponent<BoxCollider>();

        scoreText = GameObject.FindWithTag("Score").GetComponent<TextMeshProUGUI>();
        
        if (!IsOwner) return;

        abilityTimer.AddProgressionCondition(() => !GetComponent<Movement>().isPerformingAbility);
    }

    void OnTriggerStay(Collider other)
    {
        if (transform.tag == "PlayerMouse" && other.CompareTag("Rat Stun Hitbox"))
        {
            HumanPlayer humanPlayer = other.GetComponentInParent<HumanPlayer>();
            localHumanInRange = humanPlayer;

            if (IsOwner)
            {
                ratAbilityInRange = true;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (IsOwner && transform.tag == "PlayerMouse" && other.CompareTag("Rat Stun Hitbox"))
        {
            ratAbilityInRange = false;

            if (!GetComponent<Movement>().isPerformingAbility)
            {
                localHumanInRange = null;
            }
        }
    }

    public override void ExecuteAbility()
    {
        if (localHumanInRange == null) return; //safety check
        StartCoroutine(RatAbilityCoroutine());
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void SetClingingStateRpc(bool state)
    {
        isClinging.Value = state;
    }

    IEnumerator RatAbilityCoroutine()
    {
        Movement movement = GetComponent<Movement>();
        isSlapping = false;
        SetClingingStateRpc(false);
        SetHumanShakeMeterValueServerRpc(localHumanInRange.NetworkObjectId, 0f);

        Vector3 startPos = transform.position;
        Vector3 targetPos = localHumanInRange.ratAbilityTarget.transform.position;

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

        if (!forceApplied)
        {
            rb.linearDamping = 0;
            rb.AddForce(forceToAdd * rb.mass, ForceMode.Impulse);
            forceApplied = true;
        }

        while (elapsed < ratAbilityDuration)
        {
            float t = elapsed / ratAbilityDuration;
            elapsed += Time.fixedDeltaTime;

            if (Vector3.Distance(transform.position, targetPos) <= Constants.ratAbilityClingRange)
            {
                SetColliderStateServerRpc(false);
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.linearDamping = originalDrag;
                movement.toggleGravity = false;
                clingHead = localHumanInRange.movement.headBone;

                rb.useGravity = false;
                rb.detectCollisions = false;
                UpdateHumanSlapCountServerRpc(localHumanInRange.NetworkObjectId, 0, "Set");

                SetClingingStateRpc(true);
                break;
            }
            yield return new WaitForFixedUpdate();
        }
        Debug.DrawLine(transform.position, targetPos, Color.red, 3f);
    }

    [ServerRpc]
    public void SetColliderStateServerRpc(bool state)
    {
        boxCollider.enabled = state;
    }

    void UnCling()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        Movement movement = GetComponent<Movement>();

        SetColliderStateServerRpc(true);
        movement.toggleGravity = true;

        rb.useGravity = true;
        rb.detectCollisions = true;

        SetClingingStateRpc(false);
        movement.isPerformingAbility = false;

        SetHumanClingStateServerRpc(localHumanInRange.NetworkObjectId, false);
        SetHumanDizzyStateServerRpc(localHumanInRange.NetworkObjectId, true);
    }

    public override bool CheckAbilityExecutable() {
        return (ratAbilityInRange);
    }

    protected override void Update()
    {
        base.Update();

        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Q) && isClinging.Value)
        {
            isSlapping = !isSlapping;
            UpdateHumanSlapCountServerRpc(localHumanInRange.NetworkObjectId, 1, "Add");
        }
        if (Input.GetKeyDown(KeyCode.U) && isClinging.Value)
        {
            UnCling();
        }

        if (isClinging.Value && IsOwner)
        {
            clingHead = localHumanInRange.movement.headBone;
            HumanPlayer humanPlayer = localHumanInRange.GetComponent<HumanPlayer>();
            // IncreaseHumanShakeMeterValue(localHumanInRange.NetworkObjectId, Time.deltaTime);
            SetHumanClingStateServerRpc(localHumanInRange.NetworkObjectId, true);
            transform.position =
                clingHead.position +
                clingHead.TransformDirection(Vector3.forward * 0.1f) +
                clingHead.TransformDirection(Vector3.down * 0.02f);
            SetViewPositionServerRpc(localHumanInRange.NetworkObjectId, localHumanInRange.ratAbilityTarget.transform.position);


            Quaternion flip = Quaternion.Euler(0, 180f, 0);
            transform.rotation = clingHead.rotation * flip;
            Debug.DrawRay(clingHead.position, clingHead.forward * 0.5f, Color.blue);
            Debug.DrawRay(clingHead.position, clingHead.up * 0.5f, Color.green);
            Debug.DrawRay(clingHead.position, clingHead.right * 0.5f, Color.red);
            if (humanPlayer.ratAbilityHumanShakeMeter.Value >= Constants.maxRatAbilityHumanShakeMeter)
            {
                UnCling();
            }
        }
    }

    public void FixedUpdate()
    {
        if (isClinging.Value && IsServer)
        {
            HumanPlayer humanPlayer = localHumanInRange.GetComponent<HumanPlayer>();
            IncreaseHumanShakeMeterValue(localHumanInRange.NetworkObjectId);
        }

    }

    [ServerRpc]
    void SetViewPositionServerRpc(ulong humanNetworkId, Vector3 pos)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(humanNetworkId, out NetworkObject netObj))
        {
            Debug.Log("Human not found on server");
            return;
        }

        HumanPlayer human = netObj.GetComponent<HumanPlayer>();

        if (human == null || human.viewPosition == null)
        {
            Debug.Log("Human or viewPosition missing");
            return;
        }

        human.viewPosition.transform.position = pos;
    }

    [ServerRpc]
    void SetHumanClingStateServerRpc(ulong humanId, bool state)
    {
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(humanId, out NetworkObject netObj))
        {
            HumanPlayer human = netObj.GetComponent<HumanPlayer>();
            if (human != null)
            {
                human.isBeingClung.Value = state;
            }
        }
    }

    void IncreaseHumanShakeMeterValue(ulong humanId)
    {
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(humanId, out NetworkObject netObj))
        {
            HumanPlayer human = netObj.GetComponent<HumanPlayer>();
            if (human != null)
            {
                human.ratAbilityHumanShakeMeter.Value += Time.fixedDeltaTime;
            }
        }
    }

    [ServerRpc]
    void SetHumanShakeMeterValueServerRpc(ulong humanId, float value)
    {
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(humanId, out NetworkObject netObj))
        {
            HumanPlayer human = netObj.GetComponent<HumanPlayer>();
            if (human != null)
            {
                human.ratAbilityHumanShakeMeter.Value = value;
            }
        }
    }

    [ServerRpc]
    void UpdateHumanSlapCountServerRpc(ulong humanId, int value, string addOrSet)
    {
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(humanId, out NetworkObject netObj))
        {
            HumanPlayer human = netObj.GetComponent<HumanPlayer>();
            if (human != null)
            {
                if (addOrSet == "Add")
                {
                    human.slapCount.Value += value;
                }
                if (addOrSet == "Set")
                {
                    human.slapCount.Value = value;
                }
            }
        }
    }

    [ServerRpc]
    void SetHumanDizzyStateServerRpc(ulong humanId, bool state)
    {
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(humanId, out NetworkObject netObj))
        {
            HumanPlayer human = netObj.GetComponent<HumanPlayer>();
            if (human != null)
            {
                human.isDizzy.Value = state;
            }
        }
    }

    void OnDrawGizmos()
    {
        if (transform.position != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, 0.01f);
        }
    }
}