using UnityEngine;
using Unity.Netcode;

public class ObjectManager : NetworkBehaviour
{
    public static void MakeObjectSpectral(GameObject objectReference) {
        if (objectReference == null) return;

        if (objectReference.transform.Find("Renderer") != null)
            objectReference = objectReference.transform.Find("Renderer").gameObject;

        objectReference.layer = LayerMask.NameToLayer("SpectralObjects");
    }

    public static void TakeAwaySpectral(GameObject objectReference) {
        if (objectReference == null) return;
        
        if (objectReference.transform.Find("Renderer") != null)
            objectReference = objectReference.transform.Find("Renderer").gameObject;

        objectReference.layer = LayerMask.NameToLayer("Default");
    }

    public static bool CheckPlayerSeesObject(GameObject objectReference) {
        if (Player.localPlayer == null) return false;
        
        Vector3 startingPoint = Player.localPlayer.viewPosition.transform.position;
        Vector3 endPoint = objectReference.transform.position;

        if (Physics.Raycast(startingPoint,
                            (endPoint - startingPoint).normalized,
                            out RaycastHit hit,
                            (endPoint - startingPoint).magnitude,
                            LayerMask.GetMask("groundLayer"))) {

            return (hit.transform.gameObject == objectReference);
        }

        return true;
    }
}