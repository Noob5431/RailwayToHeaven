using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(itemStorage))]
public class networkFurnance : NetworkBehaviour
{
    itemStorage currentItemStorage;
    [SerializeField]
    private float consumptionTime;
    [SerializeField]
    private float currentTime;
    NetworkVariable<int> networkCapacity = new NetworkVariable<int>();
    NetworkVariable<bool> removedFromStorage = new NetworkVariable<bool>();

    private void Awake()
    {
        currentItemStorage = GetComponent<itemStorage>();
    }
    private void Start()
    {
        if (IsServer)
            UpdateFurnanceValueServerRpc(currentItemStorage.currentCapacity);
        else if (IsOwner) currentItemStorage.currentCapacity = networkCapacity.Value;
    }

    private void Update()
    {
        if (IsServer)
        {
            currentTime += Time.deltaTime;
            if (currentTime > consumptionTime)
            {
                if (currentItemStorage.checkItemStorage() == 1)
                {
                    currentTime = 0;
                    RemoveFromStorageClientRpc();
                    UpdateFurnanceValueServerRpc(currentItemStorage.currentCapacity);
                }
                else if (currentItemStorage.checkItemStorage() == 0)
                {
                    NetworkManager.Singleton.SceneManager.LoadScene("badscene", UnityEngine.SceneManagement.LoadSceneMode.Single);
                }
            }
        }
        /*if (removedFromStorage.Value == true)
        {
            currentItemStorage.removeFromStorage();
            if (IsServer)
            {
                UpdateRemovedFromStorageServerRpc(false);
                
            }
        }*/
    }

    [ServerRpc]
    void UpdateFurnanceValueServerRpc(int value)
    {
        networkCapacity.Value = value;
    }
    [ServerRpc]
    void UpdateRemovedFromStorageServerRpc(bool value)
    {
        removedFromStorage.Value = value;
    }
    [ClientRpc]
    void RemoveFromStorageClientRpc()
    {
        currentItemStorage.removeFromStorage();
    }
}
