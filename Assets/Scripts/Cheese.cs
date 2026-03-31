using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Cheese : MonoBehaviour
{
<<<<<<< Updated upstream
    private bool playerInRange = false;
    public GameObject promptUI;
    public int score;
    public TextMeshProUGUI scoreText;
    public ObjectiveScores objectiveScores;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && localPlayerInRange != null)
        {
<<<<<<< Updated upstream
            score += objectiveScores.cheeseScore;
            scoreText.text = $"Score: {score.ToString()}";
            promptUI.SetActive(false);
            // Destroy(gameObject);
            DestroyNetworkObjectServerRpc();
=======
            localPlayerInRange.promptUI.SetActive(false);
            localPlayerInRange.score += ObjectiveScores.cheeseScore;
            localPlayerInRange.scoreText.text = $"Score: {localPlayerInRange.score}";
            DespawnServerRpc();
>>>>>>> Stashed changes
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
            Player player = other.GetComponentInParent<Player>();
            localPlayerInRange = player;

            playerInRange = true;
            localPlayerInRange.promptUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerMouse"))
        {
            Player player = other.GetComponentInParent<Player>();
            localPlayerInRange = player;

            player.promptUI.SetActive(false);
            playerInRange = false;

            localPlayerInRange = null;
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
}
