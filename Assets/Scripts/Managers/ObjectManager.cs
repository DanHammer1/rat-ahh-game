using UnityEngine;
using Unity.Netcode;

public class ObjectManager : NetworkBehaviour
{
    public static ObjectManager instance;

    void Start()
    {
        if (instance == null) instance = this;
    }
    public static void MakeObjectSpectral(GameObject objectReference)
    {
        if (objectReference == null) return;

        if (objectReference.transform.Find("Renderer") != null)
            objectReference = objectReference.transform.Find("Renderer").gameObject;

        objectReference.layer = LayerMask.NameToLayer("SpectralObjects");
    }

    public static void TakeAwaySpectral(GameObject objectReference)
    {
        if (objectReference == null) return;

        if (objectReference.transform.Find("Renderer") != null)
            objectReference = objectReference.transform.Find("Renderer").gameObject;

        objectReference.layer = LayerMask.NameToLayer("Default");
    }

    public static void MakeObjectSpectralForEveryone(GameObject objectReference)
    {
        ObjectManager.instance.MakeObjectSpectralRpc(objectReference);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void MakeObjectSpectralRpc(NetworkObjectReference networkObjectReference)
    {
        networkObjectReference.TryGet(out NetworkObject networkObject);
        MakeObjectSpectral(networkObject.gameObject);
    }

    public static void TakeObjectSpectralForEveryone(GameObject objectReference)
    {
        ObjectManager.instance.TakeObjectSpectralRpc(objectReference);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void TakeObjectSpectralRpc(NetworkObjectReference networkObjectReference)
    {
        networkObjectReference.TryGet(out NetworkObject networkObject);
        TakeAwaySpectral(networkObject.gameObject);
    }

    public static bool CheckPlayerSeesObject(GameObject objectReference)
    {
        if (Player.localPlayer == null) return false;

        Vector3 startingPoint = Player.localPlayer.viewPosition.transform.position;
        Vector3 endPoint = objectReference.transform.position;

        if (Physics.Raycast(startingPoint,
                            (endPoint - startingPoint).normalized,
                            out RaycastHit hit,
                            (endPoint - startingPoint).magnitude,
                            LayerMask.GetMask("groundLayer")))
        {

            return (hit.transform.gameObject == objectReference);
        }

        return true;
    }
}