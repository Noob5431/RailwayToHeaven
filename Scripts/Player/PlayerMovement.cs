using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    public NetworkVariable<Vector2> position = new NetworkVariable<Vector2>();

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Move();
        }
    }

    public void Move()
    {
        if(NetworkManager.Singleton.IsServer)
        {
            var randomPosition = GetRandomPositionOnPlane();
            transform.position = randomPosition;
            position.Value = randomPosition;
        }
        else
        {
            SubmitPositionRequestServerRpc();
        }
    }
    [ServerRpc]
    void SubmitPositionRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        position.Value = GetRandomPositionOnPlane();
    }    

    Vector2 GetRandomPositionOnPlane()
    {
        return new Vector2(Random.Range(-3f, 3f), Random.Range(-3f, 3f));
    }

    void Update()
    {
        transform.position = position.Value;
    }
}
