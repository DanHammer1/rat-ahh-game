using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
using TMPro;
using System.Collections;
using UnityEditor;
using UnityEngine.SocialPlatforms;

public class RatPlayer : Player
{
    Transform clingHead;


    public bool ratAbilityInRange;
    public GameObject activateRatAbilityPrompt;
    private HumanPlayer localHumanInRange;
    public bool isClinging;
    public bool isSlapping;

    public GameObject eatCheesePrompt;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        eatCheesePrompt = GameObject.FindWithTag("Eat Cheese Prompt");
        eatCheesePrompt.SetActive(false);

        activateRatAbilityPrompt = GameObject.FindWithTag("Activate Rat Ability Prompt");
        activateRatAbilityPrompt.SetActive(false);
        ratAbilityInRange = false;
    }

    void OnTriggerStay(Collider other)
    {
        if (IsOwner && transform.tag == "PlayerMouse" && other.CompareTag("Rat Stun Hitbox"))
        {
            HumanPlayer player = other.GetComponentInParent<HumanPlayer>();
            localHumanInRange = player;

            ratAbilityInRange = true;
            activateRatAbilityPrompt.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (IsOwner && transform.tag == "PlayerMouse" && other.CompareTag("Rat Stun Hitbox"))
        {
            ratAbilityInRange = false;
            activateRatAbilityPrompt.SetActive(false);

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
                rb.useGravity = false;
                movement.toggleGravity = false;
                clingHead = localHumanInRange.movement.headBone;

                rb.constraints = RigidbodyConstraints.FreezeAll;
                isClinging = true;
                break;
            }
            yield return new WaitForFixedUpdate();
        }

        rb.linearVelocity = Vector3.zero;
        rb.linearDamping = originalDrag;
        // movement.isPerformingAbility = false;
        Debug.DrawLine(transform.position, targetPos, Color.red, 3f);
        // playerCamera.isCameraLocked = false;
    }

    protected override void Update()
    {
        base.Update();

        if (ratAbilityInRange && Input.GetKeyDown(KeyCode.T)) // whole ratplayer
        {
            ActivateRatAbility();
        }

        if (Input.GetKeyDown(KeyCode.Q) && isClinging)  // whole ratplayer
        {
            isSlapping = !isSlapping;
        }

        if (isClinging)  // whole ratplayer
        {
            transform.position = clingHead.position + clingHead.TransformDirection(Vector3.forward * 0.05f);
            localHumanInRange.viewPosition.transform.position = localHumanInRange.ratAbilityTarget.transform.position;

            Quaternion flip = Quaternion.Euler(0, 180f, 0);
            transform.rotation = clingHead.rotation * flip;
            Debug.DrawRay(clingHead.position, clingHead.forward * 0.5f, Color.blue);
            Debug.DrawRay(clingHead.position, clingHead.up * 0.5f, Color.green);
            Debug.DrawRay(clingHead.position, clingHead.right * 0.5f, Color.red);
        }
    }
}
