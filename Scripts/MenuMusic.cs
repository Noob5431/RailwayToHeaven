using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuMusic : MonoBehaviour
{
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        //NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplete;
    }
    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            Destroy(gameObject);
        }
    }
    /*private void OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (sceneName == "MainScene")
            gameObject.SetActive(false);
    }*/
}
