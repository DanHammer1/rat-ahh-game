using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode.Transports.UTP;
using System.Collections;

public class Loading : NetworkBehaviour
{

    public GameObject playerPrefab;
    void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            LoadGameScene();
        }
    }
    

    private void LoadGameScene()
    {
        NetworkManager.Singleton.SceneManager.LoadScene(
            "Game",
            LoadSceneMode.Single
        );

        NetworkManager.Singleton.SceneManager.OnLoadComplete += (clientId, sceneName, loadMode) =>
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                foreach (ulong client in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    GameObject playerInstance = Instantiate(playerPrefab);
                    NetworkObject netObj = playerInstance.GetComponent<NetworkObject>();
                    netObj.SpawnAsPlayerObject(client, true);
                }
            }
        };
    }
}
