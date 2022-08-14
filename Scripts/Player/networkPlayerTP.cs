using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class networkPlayerTP : NetworkBehaviour
{
    public GameObject currentTeleporter;
    NetworkVariable<Vector3> tpCoordinates = new NetworkVariable<Vector3>();
    NetworkVariable<bool> activated = new NetworkVariable<bool>();
    Unity.Netcode.Components.NetworkTransform networkTransform;

    private void Awake()
    {
        networkTransform = gameObject.GetComponent<Unity.Netcode.Components.NetworkTransform>();
    }
    void Update()
    {
        if (IsClient && IsOwner && gameObject.GetComponent<NetworkPlayerMovement>().isImpostor)
        {
            if (Input.GetButtonDown("Use"))
            {
                if (currentTeleporter != null)
                {
                    UpdatePlayerPositionServerRpc(currentTeleporter.GetComponent<teleporter>().GetDestination().position);
                    UpdateActivatedServerRpc(true);
                }
            }
        }
        
        if (activated.Value)
        {
            transform.position = tpCoordinates.Value;
            UpdateActivatedServerRpc(false);
        }
        
          
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("teleporter"))
        {
            currentTeleporter = collision.gameObject;
            gameObject.GetComponent<NetworkPlayerMovement>().ui_canvas.use.SetActive(true);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("teleporter"))
        {
            if (collision.gameObject == currentTeleporter)
            {
                currentTeleporter = null;
                //gameObject.GetComponent<NetworkPlayerMovement>().ui_canvas.use.SetActive(false);
            }
        }
    }
    [ServerRpc]
    void UpdatePlayerPositionServerRpc(Vector3 newPosition)
    {
        tpCoordinates.Value = newPosition;
    }
    [ServerRpc]
    void UpdateActivatedServerRpc(bool newValue)
    {
        activated.Value = newValue;
    }
}
