using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using TMPro;
using UnityEngine;

public class playerName : NetworkBehaviour
{
    [SerializeField]
    private userInfo currentUserInfo;
    [SerializeField]
    private TextMeshProUGUI nameText;
    NetworkVariable<NetworkString> username = new NetworkVariable<NetworkString>();

    private void Start()
    {
        currentUserInfo = GameObject.Find("userInfo").GetComponent<userInfo>();
    }

    void Update()
    {
            if (IsOwner)
            {
                if(currentUserInfo.username!=username.Value)
                    UpdateUsernameServerRpc(currentUserInfo.username);
            }
            nameText.text = username.Value;
    }
   
    [ServerRpc]
    void UpdateUsernameServerRpc(string newName)
    {
        username.Value = newName;
    }



}

public struct NetworkString : INetworkSerializable
{
    private Unity.Collections.FixedString64Bytes info;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref info);
    }
    public override string ToString()
    {
        return info.ToString();
    }
    public static implicit operator string(NetworkString s) => s.ToString();
    public static implicit operator NetworkString(string s) => new NetworkString() { info = new Unity.Collections.FixedString32Bytes(s) };
}
