using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerInventory : NetworkBehaviour
{
    /* 0 checksum
     * 1 raw food
     * 2 cooked food
     * 3 coal
     * 
     */

    [SerializeField]
    public bool[] inventory = new bool[5];
    [SerializeField]
    private GameObject[] icons = new GameObject[5];
    public GameObject currentPrison;
    public itemStorage currentItemStorage;
    NetworkPlayerMovement networkPlayerMovement;
    grabScript grab;
    [SerializeField]
    NetworkVariable<float> timeToHunger = new NetworkVariable<float>();
    [SerializeField]
    float maxTimeToHunger;
    [SerializeField]
    float timeFromFood;
    float currentFoodLevel;

    private void Start()
    {
        timeFromFood = maxTimeToHunger / 5;
        if(IsServer)
        {
            timeToHunger.Value = maxTimeToHunger;
        }
        networkPlayerMovement = GetComponent<NetworkPlayerMovement>();
        grab = networkPlayerMovement.grabArea.GetComponent<grabScript>();
    }

    private void Update()
    {
        if (networkPlayerMovement.isInGame)
        {
            if (IsServer)
            {
                timeToHunger.Value -= Time.deltaTime;
                if (timeToHunger.Value > maxTimeToHunger)
                    timeToHunger.Value = maxTimeToHunger;
            }
            if (timeToHunger.Value <= 0)
            {
                if (IsServer)
                    timeToHunger.Value = 0;
                networkPlayerMovement.isIncapacitated = true;
            }
            else networkPlayerMovement.isIncapacitated = false;
        }
        if (networkPlayerMovement.isInGame && IsOwner)
        {
            if ((networkPlayerMovement.grabArea.grabbedPlayer || networkPlayerMovement.grabbedPlayer)
                && !networkPlayerMovement.grabArea.grabbedPlayer.GetComponent<NetworkPlayerMovement>().isInPrison)
                networkPlayerMovement.ui_canvas.grab.SetActive(true);
            else networkPlayerMovement.ui_canvas.grab.SetActive(false);
            if (inventory[1] || inventory[2] || inventory[3])
                networkPlayerMovement.ui_canvas.drop.SetActive(true);
            else networkPlayerMovement.ui_canvas.drop.SetActive(false);
            if(timeToHunger.Value < maxTimeToHunger)
                currentFoodLevel = timeToHunger.Value / timeFromFood;
            int i;
            for (i = 0; i < currentFoodLevel; i++)
            {
                networkPlayerMovement.ui_canvas.foodIcons[i].SetActive(true);
            }
            for (; i < 5; i++)
            {
                networkPlayerMovement.ui_canvas.foodIcons[i].SetActive(false);
            }
            for(i = 0; i<networkPlayerMovement.coalFurnance.currentCapacity; i++)
            {
                networkPlayerMovement.ui_canvas.coalIcons[i].SetActive(true);
            }
            for (; i < 5; i++)
            {
                networkPlayerMovement.ui_canvas.coalIcons[i].SetActive(false);
            }
            if(currentItemStorage || currentPrison)
            {
                if(currentItemStorage && !currentItemStorage.CompareTag("radio"))
                    networkPlayerMovement.ui_canvas.use.SetActive(true);
                if (currentPrison)
                    networkPlayerMovement.ui_canvas.use.SetActive(true);
                if (currentItemStorage && networkPlayerMovement.isImpostor)
                {
                    networkPlayerMovement.ui_canvas.sabotage.SetActive(true);
                }
            }
            else
            {
                networkPlayerMovement.ui_canvas.sabotage.SetActive(false);
                if(networkPlayerMovement.isImpostor && !gameObject.GetComponent<networkPlayerTP>().currentTeleporter)
                    networkPlayerMovement.ui_canvas.use.SetActive(false);
                else if (!networkPlayerMovement.isImpostor) 
                    networkPlayerMovement.ui_canvas.use.SetActive(false);
            }
            if (networkPlayerMovement.horn)
                networkPlayerMovement.ui_canvas.use.SetActive(true);
            if (networkPlayerMovement.isGrabbing)
                networkPlayerMovement.ui_canvas.use.SetActive(false);
        }
        
    }

    public void addItem(int itemId,int value)
    {
        if(CheckValue(value) && itemId>0 && !currentItemStorage.isSabotaged)
        {
            if (value == 1)
            {
                inventory[itemId] = true;
                inventory[0] = true;
                icons[itemId].SetActive(true);
            }
            else
            {
                if (inventory[itemId] == true)
                {
                    if (currentItemStorage.addToStorage() == 1)
                    {
                        inventory[0] = false;

                        inventory[itemId] = false;
                        icons[itemId].SetActive(false);
                    }
                }
                if(currentItemStorage.CompareTag("cooking"))
                {
                    inventory[2] = true;
                    inventory[0] = true;
                    icons[2].SetActive(true);
                }
            }
        }
    }

    public void throwItem()
    {
        inventory[0] = false;
        
        for(int i = 1; i<5 ;i++)
        {
            if(inventory[i] == true)
            {
                inventory[i] = false;
                icons[i].SetActive(false);
            }
        }
    }

    private bool CheckValue(int value)
    {
        bool boolValue;

        if (value == 1) boolValue = false;
        else if (value == -1) boolValue = true;
        else return false;

        return boolValue == inventory[0];
    }

    public void updateInventory()
    {
        if (networkPlayerMovement.horn)
            networkPlayerMovement.horn.GetComponent<Horn>().activateSound();
        else if (!networkPlayerMovement.isInPrison)
        {
            if (currentPrison && networkPlayerMovement.isGrabbing && currentPrison.GetComponent<PrisonScript>().prisoner == null)
            {
                networkPlayerMovement.grabbedPlayer.GetComponent<NetworkPlayerMovement>().GoToPrison(currentPrison);
            }
            else if (currentPrison && currentPrison.GetComponent<PrisonScript>().prisoner != null && !networkPlayerMovement.isGrabbing)
            {
                currentPrison.GetComponent<PrisonScript>().prisoner.GetComponent<NetworkPlayerMovement>().ExitPrison(gameObject);
            }
            if (inventory[2] == true)
            {
                if (IsServer)
                {
                    timeToHunger.Value += timeFromFood;
                }
                inventory[0] = false;
                inventory[2] = false;
                icons[2].SetActive(false);
            }
            if (currentItemStorage)
            {
                if (currentItemStorage.CompareTag("radio"))
                    addItem(currentItemStorage.storageItemId, currentItemStorage.value);
                else addItem(currentItemStorage.storageItemId, currentItemStorage.value);
            }

        }
    }

    public void Sabotage()
    {
        if(currentItemStorage)
            currentItemStorage.Sabotage();
    }

}
