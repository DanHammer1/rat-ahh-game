using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
using TMPro;
using System.Collections;
using UnityEditor;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class RatPlayer : Player
{
    Transform clingHead;


    public bool ratAbilityInRange;
    private HumanPlayer localHumanInRange;
    public bool isClinging;
    public bool isSlapping;
    public int slapCount;
    public float ratAbilityCooldown;



    public GameObject eatCheesePrompt;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner) return;
        eatCheesePrompt = GameObject.FindWithTag("Eat Cheese Prompt");
        eatCheesePrompt.SetActive(false);
        ratAbilityInRange = false;


        slapCount = 0;
        ratAbilityCooldown = 0;

        Debug.Log("Outline GO: " + abilityIconBackgroundOutline);
        Debug.Log("Outline Image: " + abilityIconBackgroundOutlineImage);
        Debug.Log("Ability T: " + abilityT);
        Debug.Log("Ability T Text: " + abilityTText);

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

            // Debug.DrawLine(transform.position, targetPos, Color.red, 0.02f);
            // if (Vector3.Distance(transform.position, targetPos) <= Constants.ratAbilityClingRange)
            // {
            //     Debug.Log(Vector3.Distance(transform.position, targetPos));
            // }

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
                slapCount = 0;

                isClinging = true;
                break;
            }
            yield return new WaitForFixedUpdate();
        }
        // movement.isPerformingAbility = false;
        Debug.DrawLine(transform.position, targetPos, Color.red, 3f);
        // playerCamera.isCameraLocked = false;
    }

    void UnCling()
    {
        SetColliderStateServerRpc(true);
        movement.toggleGravity = true;

        rb.useGravity = true;
        rb.detectCollisions = true;

        isClinging = false;
        movement.isPerformingAbility = false;
        ratAbilityCooldown = 10f;
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

        if (ratAbilityInRange && Input.GetKeyDown(KeyCode.T) && ratAbilityCooldown == 0)
        {
            ActivateRatAbility();
        }
        if (Input.GetKeyDown(KeyCode.Q) && isClinging)
        {
            isSlapping = !isSlapping;
            slapCount += 1;
        }
        if (Input.GetKeyDown(KeyCode.U) && isClinging)
        {
            UnCling();
        }

        if (isClinging && IsOwner)
        {
            clingHead = localHumanInRange.movement.headBone;
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

    void OnDrawGizmos()
    {
        if (transform.position != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, 0.01f);
        }
    }
}