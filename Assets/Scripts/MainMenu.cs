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

    public NetworkList<ulong> clientIds = new NetworkList<ulong>();
    public NetworkList<FixedString32Bytes> clientNames = new NetworkList<FixedString32Bytes>();

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

        if (clientIds == null) clientIds = new NetworkList<ulong>();
        if (clientNames == null) clientNames = new NetworkList<FixedString32Bytes>();

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
        clientIds.Add(clientId);
        clientNames.Add(name);
    }

    private void ClientIdsChanged(NetworkListEvent<ulong> changeEvent)
    {
        updateCount++;
    }

    private void ClientNamesChanged(NetworkListEvent<FixedString32Bytes> changeEvent)
    {
        updateCount++;
    }
    void UpdateLobbyText()
    {
        string wantedLobbyText = $@"Host: {clientNames[
            clientIds.IndexOf(NetworkManager.ServerClientId)]}
                                 Clients: ";

        int i = 0;
        foreach (FixedString32Bytes name in clientNames)
        {
            if (clientIds[clientNames.IndexOf(name)] != NetworkManager.ServerClientId)
            {
                wantedLobbyText += name;
                if (i + 1 < clientNames.Count) wantedLobbyText += ", ";
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

        clientIds.Clear();
        clientNames.Clear();
    }

    void Update()
    {
        if (updateCount == 2 && joined)
        {
            UpdateLobbyText();
            updateCount = 0;
        }
        // If statement will be satisfied every 2 times one of the clientId or clientName
        // Lists gets updated and as they both get updated at the same time with the rest of the code
        // They should be synced. 
    }
}