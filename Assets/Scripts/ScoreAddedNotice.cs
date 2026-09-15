using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreAddedNotice : MonoBehaviour {
    TextMeshProUGUI text;
    void Awake() {
        text = GetComponent<TextMeshProUGUI>();
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut() {
        yield return new WaitForSeconds(2);

        float elapsed = 0;
        float duration = 1;
        while (elapsed < duration) {
            float alpha = Mathf.Lerp(1, 0, elapsed / duration);
            text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }
}
