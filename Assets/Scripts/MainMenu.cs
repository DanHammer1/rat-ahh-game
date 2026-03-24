using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode.Transports.UTP;
using System.Collections.Generic;
using System.Collections;

public class MainMenu : NetworkBehaviour
{
    public TMP_InputField ipInput;
    public TMP_InputField nameInput;

    public TextMeshProUGUI lobbyText;

    bool joined = false;

    private int updateCount;

    public override void OnNetworkSpawn()
    {
        if (clientIds != null)
            clientIds.OnListChanged -= ClientIdsChanged;

        if (clientNames != null)
            clientNames.OnListChanged -= ClientNamesChanged;

        clientIds.OnListChanged += ClientIdsChanged;

        clientNames.OnListChanged += ClientNamesChanged;

        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

    }
    public void Host()
    {
        if (joined) return;

        NetworkManager.Singleton.StartHost();

        if (GameManager.Instance.clientIds == null) GameManager.Instance.clientIds = new NetworkList<ulong>();
        if (GameManager.Instance.clientNames == null) GameManager.Instance.clientNames = new NetworkList<FixedString32Bytes>();

        AddSelfToLobby();
    }

    // Update is called once per frame
    public void Join()
    {
        if (joined) return;

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        transport.SetConnectionData(ipInput.text, 7777);

        NetworkManager.Singleton.StartClient();

        AddSelfToLobby();
    }

    public void BecomeHider()
    {
        preference = GameManager.PlayerRole.Hider;
        hasPreference = true;

        if (joined)
        {
            ulong clientId = NetworkManager.Singleton.LocalClientId;
            EditClientRoleServerRpc(clientId, preference);
        }
    }

    public void BecomeHunter()
    {
        preference = GameManager.PlayerRole.Hunter;
        hasPreference = true;

        if (joined)
        {
            ulong clientId = NetworkManager.Singleton.LocalClientId;
            EditClientRoleServerRpc(clientId, preference);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void EditClientRoleServerRpc(ulong clientId, GameManager.PlayerRole preference)
    {
        GameManager.Instance.clientRoles[GameManager.Instance.clientIds.IndexOf(clientId)]
                = (int)(preference);
    }

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    public void StartGame()
    {
        if (!joined) return;

        NetworkManager.Singleton.SceneManager.LoadScene(
        "LoadingScreen",
        LoadSceneMode.Single);
    }

    void AddSelfToLobby()
    {
        StartCoroutine(ExecuteWhenConnected(() =>
        {

            joined = true;

            if (nameInput.text == "") nameInput.text = NetworkManager.Singleton.LocalClientId.ToString();

            AddNameToLobbyListServerRpc(NetworkManager.
                Singleton.LocalClientId, nameInput.text);

            if (IsServer) UpdateLobbyText();
        }));
    }

    IEnumerator ExecuteWhenConnected(System.Action function)
    {
        while (NetworkManager.Singleton == null ||
            !NetworkManager.Singleton.IsListening ||
            !NetworkManager.Singleton.IsConnectedClient ||
            !NetworkManager.Singleton.IsClient)
            yield return null;

        function();
    }
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void AddNameToLobbyListServerRpc(ulong clientId, FixedString32Bytes name)
    {
        GameManager.Instance.clientIds.Add(clientId);
        GameManager.Instance.clientNames.Add(name);
        if (!hasPreference) GameManager.Instance.clientRoles.Add(-1);
        else GameManager.Instance.clientRoles.Add((int)preference);
    }

    void UpdateLobbyText()
    {
        string wantedLobbyText = $@"Host: {GameManager.Instance.clientNames[
            GameManager.Instance.clientIds.IndexOf(NetworkManager.ServerClientId)]}
                                 Clients: ";

        int i = 0;
        foreach (FixedString32Bytes name in GameManager.Instance.clientNames)
        {
            if (clientIds[clientNames.IndexOf(name)] != NetworkManager.ServerClientId)
            {
                wantedLobbyText += name;
                if (i + 1 < GameManager.Instance.clientNames.Count) wantedLobbyText += ", ";
            }
            i++;
        }

        lobbyText.text = wantedLobbyText;
    }
    void OnClientDisconnected(ulong clientId)
    {
        if (!joined) return;

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            lobbyText.text = "Disconnected.";
            joined = false;
            return;
        }
        else if (clientId == NetworkManager.ServerClientId)
        {
            lobbyText.text = "Server/Host Disconnected.";
            joined = false;
        }

        if (!IsServer) return;

        clientNames.RemoveAt(clientIds.IndexOf(clientId));
        clientIds.Remove(clientId);
    }

    public void Disconnect()
    {
        if (!joined) return;

        joined = false;

        lobbyText.text = "Disconnected.";

        NetworkManager.Singleton.Shutdown();

        if (!IsServer) return;

        GameManager.Instance.clientIds.Clear();
        GameManager.Instance.clientNames.Clear();
    }

    void Update()
    {
        if (updateCount == 2 && joined)
        {
            UpdateLobbyText();
            GameManager.updateCount = 0;
        }
        // If statement will be satisfied every 3 times one of the clientId or clientName or clientRole
        // Lists gets updated and as they both get updated at the same time with the rest of the code
        // They should be synced. 
    }
}