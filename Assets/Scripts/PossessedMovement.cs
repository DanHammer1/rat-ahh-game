using UnityEngine;
using Unity.Netcode;
using System.Collections;
using NUnit.Framework;
using UnityEngine.UI;

public class PossessedMovement : NetworkBehaviour {
    public NetworkVariable<NetworkObjectReference> itemBeingPossessedObject = new NetworkVariable<NetworkObjectReference>();
    private float jumpForce;
    private float maxJumpForce = 10f;
    private bool isChargingJump = false;
    private float cooldown = 0;
    private bool isOnCooldown = false;
    public GameObject ratPossessedJumpMeterUI;
    public GameObject jumpMeter;
    public Image jumpMeterImage;
    RatPlayer ratPlayer;

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        if (!IsOwner) return;
        ratPlayer = GetComponent<RatPlayer>();
        ratPossessedJumpMeterUI = Assets.instance.ratPossessedJumpMeterUI;
        ratPlayer.possessedItem += AssignJumpMeterVariables;
        ratPossessedJumpMeterUI.SetActive(false);
    }

    void AssignJumpMeterVariables() {
        jumpMeter = ratPossessedJumpMeterUI.transform.Find("JumpMeter/JumpProgressBar").gameObject;
        jumpMeterImage = jumpMeter?.GetComponent<Image>();
        ratPossessedJumpMeterUI.SetActive(true);
    }

    void FixedUpdate() {
        if (!IsOwner || Player.localPlayer.dead || !itemBeingPossessedObject.Value.TryGet(out NetworkObject itemBeingPossessed)) return;

        transform.position = itemBeingPossessed.transform.position;
    }

    void Update() {
        if (!IsOwner || Player.localPlayer.dead || !itemBeingPossessedObject.Value.TryGet(out NetworkObject itemBeingPossessed)) return;

        if (Input.GetKeyDown(KeyCode.Q)) {
            GetComponent<RatPlayer>().unPossessedItem.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.Space) && !isChargingJump && !isOnCooldown) {
            isChargingJump = true;
            StartCoroutine(ChargeJump());
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void PossessedJumpRpc(Vector3 direction, float jumpForce) {
        if (!itemBeingPossessedObject.Value.TryGet(out NetworkObject itemBeingPossessed)) return;
        itemBeingPossessed.GetComponent<Rigidbody>().AddForce(direction * jumpForce, ForceMode.Impulse);
    }

    IEnumerator ChargeJump() {
        // charge jump
        jumpForce = 0;
        while (true) {
            if (Input.GetKeyUp(KeyCode.Space)) break;
            jumpForce += Time.deltaTime * 6;
            jumpMeterImage.fillAmount = Mathf.Min(jumpForce / maxJumpForce, 1);
            yield return null;
        }

        // activate jump
        jumpForce = Mathf.Clamp(jumpForce, 2.4f, maxJumpForce);
        Vector3 direction = Camera.main.transform.forward;
        direction.y = 0f;
        direction.Normalize();

        float angle = 45f;
        direction.y = Mathf.Tan(angle * Mathf.Deg2Rad);
        direction.Normalize();
        isChargingJump = false;
        jumpMeterImage.fillAmount = 0;
        PossessedJumpRpc(direction, jumpForce);

        // activate cooldown
        cooldown = Constants.possessedJumpCooldown;
        isOnCooldown = true;
        while (cooldown > 0) {
            cooldown -= Time.deltaTime;
            yield return null;
        }
        cooldown = 0;
        isOnCooldown = false;
    }
}