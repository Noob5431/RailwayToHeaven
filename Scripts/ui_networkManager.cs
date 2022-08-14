using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine;

public class ui_networkManager : MonoBehaviour
{
    bool isHost = false;
    string globalIp;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //change buildIndex to lobby scene
        //!!!!!!!!
        //!!!!!!!!
        //!!!!!!!!
        //!!!!!!!!
        if(scene.buildIndex == 1)
        {
            if (isHost)
                NetworkManager.Singleton.StartHost();
            else
            {
                globalIp = globalIp.Remove(globalIp.Length - 1, 1);
                gameObject.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>().ConnectionData.Address = globalIp;
                NetworkManager.Singleton.StartClient();
            }
        }
    }
    public void Host()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        isHost = true;
    }
    
    public void Join(string ip)
    {
        globalIp = ip;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
