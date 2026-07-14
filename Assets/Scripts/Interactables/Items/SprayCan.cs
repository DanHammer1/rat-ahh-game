using UnityEngine;
using Unity.Netcode;

public class SprayCan : Item
{
    public int width;
    public int height;

    [SerializeField] Material paintMaterial;

    public override void UseItem() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("groundLayer"))) {
            
        }
    }

    public void SetPixel(Vector2 uv, Color color, RenderTexture renderTexture, float brushSize)
    {
        
    }

    public int GetMaterialIndex(RaycastHit hit) {
        GameObject hitObject = hit.collider.gameObject;
        Mesh mesh = hitObject.GetComponent<MeshCollider>().sharedMesh;

        int triangleIndex = hit.triangleIndex;

        int materialIndex = 0;
        int indexStart = 0;

        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            int[] triangles = mesh.GetTriangles(i);

            if (triangleIndex * 3 >= indexStart &&
                triangleIndex * 3 < indexStart + triangles.Length)
            {
                materialIndex = i;
                break;
            }

            indexStart += triangles.Length;
        }

        return materialIndex;
    }

    public override string GetInteractionPromptText() {
        return "Hold E to pick up spray can.";
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void DespawnServerRpc()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }
}