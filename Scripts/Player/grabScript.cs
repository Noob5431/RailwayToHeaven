using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class grabScript : MonoBehaviour
{
    public GameObject grabbedPlayer;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
           grabbedPlayer = collision.gameObject;
           /* if (gameObject.GetComponent<NetworkPlayerMovement>().IsOwner && gameObject.GetComponent<NetworkPlayerMovement>().ui_canvas)
                gameObject.GetComponent<NetworkPlayerMovement>().ui_canvas.grab.SetActive(true);*/
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            grabbedPlayer = null;
            /*if (gameObject.GetComponent<NetworkPlayerMovement>().IsOwner && gameObject.GetComponent<NetworkPlayerMovement>().ui_canvas)
                gameObject.GetComponent<NetworkPlayerMovement>().ui_canvas.grab.SetActive(false);*/
        }
    }
}
