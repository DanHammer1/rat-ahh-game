using UnityEngine;

public class OutOfBoundsHandler : MonoBehaviour
{
    void Update() {
        if (this.transform.position.y < -10) {
            this.transform.position = Vector3.zero;
        }
    }
}
