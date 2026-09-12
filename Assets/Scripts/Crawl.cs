using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;

public class Crawl : NetworkBehaviour {
    public Action onCrawlStart;
    public Action onCrawlEnd;
    public bool isCrawling = false;
    bool isTryingToStand = false;
    Animator animator;
    CapsuleCollider capsuleCollider;
    GameObject viewPosition;
    HunterPlayer hunterPlayer;
    [SerializeField] private LayerMask standCheckMask;

    public override void OnNetworkSpawn() {
        animator = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        viewPosition = transform.Find("ViewPosition").gameObject;
        hunterPlayer = transform.GetComponent<HunterPlayer>();

        // On Crawl Start
        onCrawlStart += () => isCrawling = true;
        onCrawlStart += () => animator.SetBool("isCrawling", isCrawling);
        onCrawlStart += () => viewPosition.transform.position -= new Vector3(0, 0.6f, 0);
        onCrawlStart += () => {
            capsuleCollider.center = new Vector3(capsuleCollider.center.x, Constants.capsuleColliderCrawlingCenterY, capsuleCollider.center.z);
            capsuleCollider.height = Constants.capsuleColliderCrawlingHeight;
        };
        onCrawlStart += () => hunterPlayer.DisableRigBuilderRpc();

        // On Crawl End
        onCrawlEnd += () => isCrawling = false;
        onCrawlEnd += () => animator.SetBool("isCrawling", isCrawling);
        onCrawlEnd += () => viewPosition.transform.position -= new Vector3(0, -0.6f, 0);
        onCrawlEnd += () => {
            capsuleCollider.center = new Vector3(capsuleCollider.center.x, Constants.capsuleColliderStandingCenterY, capsuleCollider.center.z);
            capsuleCollider.height = Constants.capsuleColliderStandingHeight;
        };
        onCrawlEnd += () => hunterPlayer.EnableRigBuilderRpc();
    }

    void Update() {
        if (!IsOwner) return;
        if (Input.GetKey(KeyCode.LeftShift)) {
            if (!isCrawling && !hunterPlayer.isSwinging) {
                onCrawlStart?.Invoke();
                isTryingToStand = false;
            }
        }
        if (Input.GetKeyUp(KeyCode.LeftShift)) {
            if (isCrawling) {
                if (CanStand()) {
                    onCrawlEnd?.Invoke();
                } else isTryingToStand = true;
            }
        }

        if (isTryingToStand) {
            if (CanStand()) {
                onCrawlEnd?.Invoke();
                isTryingToStand = false;
            }
        }
    }

    bool CanStand() {
        return !Physics.CheckCapsule(
            transform.position + new Vector3(0, Constants.capsuleColliderStandingCenterY - (Constants.capsuleColliderStandingHeight / 2) + capsuleCollider.radius, 0),
            transform.position + new Vector3(0, Constants.capsuleColliderStandingCenterY + (Constants.capsuleColliderStandingHeight / 2) - capsuleCollider.radius, 0),
            capsuleCollider.radius,
            standCheckMask
            ) && !hunterPlayer.isSwinging;
    }
}
