using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class JoinButton : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI ip;
    private GameObject networkManager;
    private userInfo userInfo;
    [SerializeField]
    private TextMeshProUGUI username;

    private void Start()
    {
        networkManager = GameObject.Find("NetworkManager");
        userInfo = GameObject.Find("userInfo").GetComponent<userInfo>();
    }

    public void JoinServer()
    {
        userInfo.username = username.text;
        networkManager.GetComponent<ui_networkManager>().Join(ip.text);
    }
}
