using UnityEngine;

public class ShowUiOnRole : MonoBehaviour {
    public GameManager.PlayerRole roleToShowOn;
    void Start() {
        if (GameManager.GetLocalRole() != roleToShowOn) this.gameObject.SetActive(false);
    }
}