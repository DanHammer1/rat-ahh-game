using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;

public class Crawl : NetworkBehaviour {
    public Action onCrawlStart;
    public Action onCrawlEnd;
    bool isCrawling = false;
    bool isTryingToStand = false;
    Animator animator;
    BoxCollider boxCollider;
    GameObject viewPosition;
    HumanPlayer humanPlayer;
    [SerializeField] private LayerMask standCheckMask;

    public override void OnNetworkSpawn() {
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider>();
        viewPosition = transform.Find("ViewPosition").gameObject;
        humanPlayer = transform.GetComponent<HumanPlayer>();

        // On Crawl Start
        onCrawlStart += () => isCrawling = true;
        onCrawlStart += () => animator.SetBool("isCrawling", isCrawling);
        onCrawlStart += () => viewPosition.transform.position -= new Vector3(0, 0.6f, 0);
        onCrawlStart += () => {
            boxCollider.size = new Vector3(boxCollider.size.x, Constants.boxColliderCrawlingSizeY, Constants.boxColliderCrawlingSizeZ);
            boxCollider.center = new Vector3(boxCollider.center.x, Constants.boxColliderCrawlingCenterY, boxCollider.center.z);
        };
        onCrawlStart += () => humanPlayer.DisableRigBuilderRpc();

        // On Crawl End
        onCrawlEnd += () => isCrawling = false;
        onCrawlEnd += () => animator.SetBool("isCrawling", isCrawling);
        onCrawlEnd += () => viewPosition.transform.position -= new Vector3(0, -0.6f, 0);
        onCrawlEnd += () => {
            boxCollider.size = new Vector3(boxCollider.size.x, Constants.boxColliderStandingSizeY, Constants.boxColliderStandingSizeZ);
            boxCollider.center = new Vector3(boxCollider.center.x, Constants.boxColliderStandingCenterY, boxCollider.center.z);
        };
        onCrawlEnd += () => humanPlayer.EnableRigBuilderRpc();
    }

    void Update() {
        if (!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.LeftShift)) {
            if (!isCrawling) {
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
        return !Physics.CheckBox(
            transform.position + new Vector3(0, (Constants.boxColliderStandingSizeY / 2) + 0.01f, 0),
            new Vector3(Constants.boxColliderStandingSizeX / 2, Constants.boxColliderStandingSizeY / 2, Constants.boxColliderStandingSizeZ / 2),
            transform.rotation,
            standCheckMask
            );
    }
}
