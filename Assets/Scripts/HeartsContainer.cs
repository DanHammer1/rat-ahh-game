using Unity.Netcode;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class HeartsContainer : MonoBehaviour {


    public void DrawHearts() {
        ClearHearts();
        for (int i = 0; i < Player.localPlayer.GetComponent<RatPlayer>().lives.Value; i++) {
            GameObject heart = Instantiate(Assets.instance.heartPrefab);
            heart.transform.SetParent(transform);
        }
    }

    void ClearHearts() {
        for (int i = transform.childCount - 1; i >= 0; i--) {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}
