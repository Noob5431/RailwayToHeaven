using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class userInfo : MonoBehaviour
{
    public string username;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

}
