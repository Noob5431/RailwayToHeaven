using System.Collections;
using System.Collections.Generic;
using System;
using Unity.Netcode;
using UnityEngine;
using TMPro;

public class timeLeft : NetworkBehaviour
{
    [SerializeField]
    float startTime;
    [SerializeField]
    NetworkVariable<float> currentTime = new NetworkVariable<float>();
    TimeSpan formattedTime;
    string stringFormattedTime;
    [SerializeField]
    TextMeshProUGUI textBox;

    private void Start()
    {
        if(IsServer)
            currentTime.Value = startTime;
    }

    private void Update()
    {
        if(IsServer)
        {
            currentTime.Value -= Time.deltaTime;
            if(currentTime.Value < 0)
            {
                NetworkManager.Singleton.SceneManager.LoadScene("goodscene", UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
        }
        formattedTime = TimeSpan.FromSeconds(currentTime.Value);
        stringFormattedTime = formattedTime.ToString(@"mm\:ss");
        textBox.text = stringFormattedTime;
    }

    public void Sabotage(float timeAdded)
    {
        if (IsServer)
            currentTime.Value += timeAdded;
    }
}
