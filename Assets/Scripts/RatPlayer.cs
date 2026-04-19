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

public class RatPlayer : Player
{
    Transform clingHead;


    public bool ratAbilityInRange;
    private HumanPlayer localHumanInRange;
    public bool isClinging;
    public bool isSlapping;
    public float ratAbilityCooldown;
    public float ratAbilityHumanStunDuration;
    public float ratAbilityHumanShakeMeter;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner) return;
        ratAbilityInRange = false;

        ratAbilityCooldown = 0;

        abilityIcon.SetActive(true);
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

            if (!movement.isPerformingAbility)
            {
                localHumanInRange = null;
            }
        }
    }

    void ActivateRatAbility()
    {
        if (localHumanInRange == null) return; //safety check
        StartCoroutine(RatAbilityCoroutine());
    }

    IEnumerator RatAbilityCoroutine()
    {
        ClearConsole();

        isSlapping = false;
        isClinging = false;
        SetHumanShakeMeterValueServerRpc(localHumanInRange.NetworkObjectId, 0f);

        Vector3 startPos = transform.position;
        Vector3 targetPos = localHumanInRange.ratAbilityTarget.transform.position;

        movement.isPerformingAbility = true; // prevents movement during ability

        float ratAbilityDuration = Constants.ratAbilityDuration;
        float elapsed = 0;

        bool forceApplied = false;

        // Force camera to look at human (possibly not needed)
        // Vector3 dirToViewPos = targetPos - startPos;
        // dirToViewPos.y = 0;
        // dirToViewPos.Normalize();
        // float targetYaw = Mathf.Atan2(dirToViewPos.x, dirToViewPos.z) * Mathf.Rad2Deg;
        // movement.yaw = targetYaw;
        // playerCamera.SetCameraYaw(targetYaw);

        // Force rat to rotate towards human
        playerCamera.ForceLookAt(targetPos, startPos);

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

                isClinging = true;
                break;
            }
            yield return new WaitForFixedUpdate();
        }
        Debug.DrawLine(transform.position, targetPos, Color.red, 3f);
    }

    void UnCling()
    {
        SetColliderStateServerRpc(true);
        movement.toggleGravity = true;

        rb.useGravity = true;
        rb.detectCollisions = true;

        isClinging = false;
        movement.isPerformingAbility = false;
        ratAbilityCooldown = Constants.maxRatAbilityCooldown;

        SetHumanClingStateServerRpc(localHumanInRange.NetworkObjectId, false);
        SetHumanDizzyStateServerRpc(localHumanInRange.NetworkObjectId, true);
    }
    protected override void Update()
    {
        base.Update();

        if (!IsOwner) return;
        if (ratAbilityCooldown > 0)
        {
            ratAbilityCooldown -= Time.deltaTime;
            abilityIconBackgroundOutlineImage.color = new Color(0.4264151f, 0.4264151f, 0.4264151f); // dark gray
        }
        if (ratAbilityCooldown < 0) ratAbilityCooldown = 0;
        if (ratAbilityCooldown == 0)
        {
            abilityIconBackgroundOutlineImage.color = new Color(0f, 1f, 0.03321505f);
        }
        if (ratAbilityInRange && ratAbilityCooldown == 0)
        {
            abilityTText.color = Color.white;
        }
        else
        {
            abilityTText.color = Color.gray;
        }
        abilityIconBackgroundImage.fillAmount = Mathf.Clamp01((Constants.maxRatAbilityCooldown - ratAbilityCooldown) / Constants.maxRatAbilityCooldown);

        if (ratAbilityInRange && Input.GetKeyDown(KeyCode.T) && ratAbilityCooldown == 0)
        {
            ActivateRatAbility();
        }
        if (Input.GetKeyDown(KeyCode.Q) && isClinging)
        {
            isSlapping = !isSlapping;
            UpdateHumanSlapCountServerRpc(localHumanInRange.NetworkObjectId, 1, "Add");
        }
        if (Input.GetKeyDown(KeyCode.U) && isClinging)
        {
            UnCling();
        }

        if (isClinging && IsOwner)
        {
            clingHead = localHumanInRange.movement.headBone;
            HumanPlayer humanPlayer = localHumanInRange.GetComponent<HumanPlayer>();
            IncreaseHumanShakeMeterValueServerRpc(localHumanInRange.NetworkObjectId, Time.deltaTime);
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

    [ServerRpc]
    void IncreaseHumanShakeMeterValueServerRpc(ulong humanId, float value)
    {
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(humanId, out NetworkObject netObj))
        {
            HumanPlayer human = netObj.GetComponent<HumanPlayer>();
            if (human != null)
            {
                human.ratAbilityHumanShakeMeter.Value += value;
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