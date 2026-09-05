using Unity.Netcode;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class HeartsContainer : MonoBehaviour {

    public void DrawHearts() {
        ClearHearts();
        Debug.Log(Player.localPlayer.GetComponent<RatPlayer>().lives);
        for (int i = 0; i < Player.localPlayer.GetComponent<RatPlayer>().lives; i++) {
            GameObject heart = Instantiate(Assets.instance.heartPrefab);
            heart.transform.SetParent(transform);
        }
    }

    void ClearHearts() {
        while (transform.childCount != 0) {
            Destroy(transform.GetChild(0));
        }
    }
}
