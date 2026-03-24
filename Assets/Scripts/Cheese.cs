using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;

public class Cheese : NetworkBehaviour
{
    private bool playerInRange = false;
    public GameObject promptUI;
    public int score;
    public TextMeshProUGUI scoreText;
    public ObjectiveScores objectiveScores;


    public override void OnNetworkSpawn()
    {
        score = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            score += objectiveScores.cheeseScore;
            scoreText.text = $"Score: {score.ToString()}";
            promptUI.SetActive(false);
            // Destroy(gameObject);
            DestroyNetworkObjectServerRpc();
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void DestroyNetworkObjectServerRpc()
    {
        GetComponent<NetworkObject>().Despawn();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerMouse"))
        {
            playerInRange = true;
            promptUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerMouse"))
        {
            playerInRange = false;
            promptUI.SetActive(false);
        }
    }
}
