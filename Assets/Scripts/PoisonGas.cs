using UnityEngine;
using Unity.Netcode;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine.Rendering.PostProcessing;

public class PoisonGas : NetworkBehaviour
{
    Collider hitbox;
    ParticleSystem particles;
    ParticleSystem.MainModule main;
    Color minColor;
    Color maxColor;
    float travelDistance = 1f;
    float speed = 1f;
    float deceleration = 1f;
    float lifeTimer = 0f;
    float colorTransitionDuration = 2f;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        hitbox = transform.GetComponentInChildren<BoxCollider>();
        particles = transform.GetComponentInChildren<ParticleSystem>();
        main = particles.main;

        RaycastHit hit;

        if (Physics.SphereCast(
            transform.position,
            0.1f,
            PlayerCamera.mainCamera.transform.forward,
            out hit,
            1f, LayerMask.GetMask("groundLayer")))
        {
            travelDistance = Mathf.Max(hit.distance, 0.01f);
        }

        deceleration = speed * speed / (2f * travelDistance);
    }

    void Update()
    {
        lifeTimer += Time.deltaTime;

        transform.position += transform.forward * speed * Time.deltaTime;
        speed = Mathf.MoveTowards(speed, 0, deceleration * Time.deltaTime);

        float scaleSpeed = speed * 2f;
        hitbox.transform.localScale += Vector3.one * scaleSpeed * Time.deltaTime;
        particles.transform.localScale += Vector3.one * scaleSpeed * Time.deltaTime;

        float t = Mathf.Clamp01(lifeTimer / colorTransitionDuration);
        minColor = Color.Lerp(Hex("#09A100"), Hex("#52F249"), t);
        maxColor = Color.Lerp(Hex("#1B6500"), Hex("#37CC00"), t);
        main.startColor = new ParticleSystem.MinMaxGradient(minColor, maxColor);

        if (lifeTimer >= 10f)
        {
            DespawnServerRpc();
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void DespawnServerRpc()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }

    Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color color);
        return color;
    }


}