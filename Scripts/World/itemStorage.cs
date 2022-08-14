using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class itemStorage : NetworkBehaviour
{
    [SerializeField]
    public int storageItemId, value, currentCapacity;
    public bool isSabotaged = false;
    public float maxTimeSabotaged = 20;
    [SerializeField]
    NetworkVariable<float> timeLeftSabotaged = new NetworkVariable<float>();
    [SerializeField]
    Image sabotagedImage;
    [SerializeField]
    timeLeft currentTimeLeft;
    [SerializeField]
    float addedRadioTime = 0;

    [SerializeField]
    private int maxCapacity;

    private void Update()
    {
        if (IsServer)
        {
            if (isSabotaged)
            {
                timeLeftSabotaged.Value -= Time.deltaTime;
                if (timeLeftSabotaged.Value < 0)
                    timeLeftSabotaged.Value = 0;
            }
        }
        if (timeLeftSabotaged.Value <= 0)
        {
            isSabotaged = false;
        }
        sabotagedImage.fillAmount = (timeLeftSabotaged.Value / maxTimeSabotaged);
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            collider.gameObject.GetComponent<PlayerInventory>().currentItemStorage = gameObject.GetComponent<itemStorage>();
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            collider.gameObject.GetComponent<PlayerInventory>().currentItemStorage = null;
        }
    }

    public int addToStorage()
    {
        if (isSabotaged) 
            return 0;
        if (currentCapacity == maxCapacity && maxCapacity != 0)
            return 0;
        currentCapacity++;
        return 1;
    }

    public int removeFromStorage()
    {
        if (isSabotaged) 
            return 0;
        if (currentCapacity <= 0)
            return 0;
        currentCapacity--;
        return 1;
    }
    public int checkItemStorage()
    {
        if (currentCapacity <= 0)
            return 0;
        return 1;
    }

    public void Sabotage()
    {
        if (!isSabotaged)
        {
            if (IsServer)
                timeLeftSabotaged.Value = maxTimeSabotaged;
            isSabotaged = true;
            if (gameObject.CompareTag("radio"))
                currentTimeLeft.Sabotage(addedRadioTime);
        }
    }
}
