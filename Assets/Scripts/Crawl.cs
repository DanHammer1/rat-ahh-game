using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;

public class Crawl : NetworkBehaviour
{
    public Action onCrawlStart;
    public Action onCrawlEnd;
    bool isCrawling = false;
    Animator animator;
    GameObject viewPosition;

    public override void OnNetworkSpawn()
    {
        animator = GetComponent<Animator>();
        viewPosition = transform.Find("ViewPosition").gameObject;
        onCrawlStart += () => isCrawling = true;
        onCrawlStart += () => animator.SetBool("isCrawling", isCrawling);
        onCrawlStart += () => viewPosition.transform.position -= new Vector3(0, 0.6f, 0);
        onCrawlEnd += () => isCrawling = false;
        onCrawlEnd += () => animator.SetBool("isCrawling", isCrawling);
        onCrawlEnd += () => viewPosition.transform.position -= new Vector3(0, -0.6f, 0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            onCrawlStart?.Invoke();
        }
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            onCrawlEnd?.Invoke();
        }
    }
}
