using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;

public class lobby_script : NetworkBehaviour
{
    [SerializeField]
    int numberOfPlayers;
    [SerializeField]
    GameObject[] playerNamesTextBox = new GameObject[4];
    [SerializeField]
    GameObject startButton;
    NetworkVariable<NetworkString> playerName1 = new NetworkVariable<NetworkString>();
    NetworkVariable<NetworkString> playerName2 = new NetworkVariable<NetworkString>();
    NetworkVariable<NetworkString> playerName3 = new NetworkVariable<NetworkString>();
    NetworkVariable<NetworkString> playerName4 = new NetworkVariable<NetworkString>();

    bool hasConnected = false;
    [SerializeField]
    string currentPlayerName;

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        currentPlayerName = GameObject.Find("userInfo").GetComponent<userInfo>().username;
        if (IsServer)
        {
            numberOfPlayers = 1;
            playerNamesTextBox[0].GetComponent<TextMeshProUGUI>().text = currentPlayerName;
            playerName1.Value = currentPlayerName;
            hasConnected = true;
        }
    }

    void OnClientConnectedCallback(ulong obj)
    {
        if (IsServer)
        {
            numberOfPlayers += 1;
            UpdateNetworkNameClientRpc();
        }
    }

    [ClientRpc]
    void UpdateNetworkNameClientRpc()
    {
        if (!IsServer)
            startButton.SetActive(false);
        if(!hasConnected)
        {
            UpdateNetworkNameServerRpc(currentPlayerName);
            hasConnected = true;
        }
    }
    [ServerRpc(RequireOwnership =false)]
    void UpdateNetworkNameServerRpc(string name)
    {
        if (numberOfPlayers == 1)
            playerName1.Value = name;
        if (numberOfPlayers == 2)
            playerName2.Value = name;
        if (numberOfPlayers == 3)
            playerName3.Value = name;
        if (numberOfPlayers == 4)
            playerName4.Value = name;
    }

    void Update()
    {
        if (playerName1.Value != "")  playerNamesTextBox[0].GetComponent<TextMeshProUGUI>().text = playerName1.Value;
        if (playerName2.Value != "")  playerNamesTextBox[1].GetComponent<TextMeshProUGUI>().text = playerName2.Value;
        if (playerName3.Value != "")  playerNamesTextBox[2].GetComponent<TextMeshProUGUI>().text = playerName3.Value;
        if (playerName4.Value != "")  playerNamesTextBox[3].GetComponent<TextMeshProUGUI>().text = playerName4.Value;
    }

    public void StartGame()
    {
        if (IsServer && numberOfPlayers == 4)
        {
                System.Random rnd = new System.Random();
                int i = rnd.Next(1, 4);
                switch (i)
                {
                    case 1:
                        UpdateImpostorClientRpc(playerName1.Value);
                        break;
                    case 2:
                        UpdateImpostorClientRpc(playerName2.Value);
                        break;
                    case 3:
                        UpdateImpostorClientRpc(playerName3.Value);
                        break;
                    case 4:
                        UpdateImpostorClientRpc(playerName4.Value);
                        break;
                }
            NetworkManager.SceneManager.LoadScene("MainScene",LoadSceneMode.Single);
        }
    }

    [ClientRpc]
    void UpdateImpostorClientRpc(string nameToCheck)
    {
        if (currentPlayerName == nameToCheck)
            NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().GetComponent<NetworkPlayerMovement>().isImpostor = true;
    }

}
