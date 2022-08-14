using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class setName : MonoBehaviour
{
    private userInfo userInfo;
    
    private void Start()
    {
        userInfo = GameObject.Find("userInfo").GetComponent<userInfo>();
    }

    public void SetName()
    {
        userInfo.username = gameObject.GetComponent<TextMeshProUGUI>().text;
    }
}
