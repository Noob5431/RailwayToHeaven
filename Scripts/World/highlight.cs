using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class highlight : MonoBehaviour
{
    [SerializeField]
    GameObject border_glow;
    public bool isStairs;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (isStairs)
            {
                if (collision.GetComponent<NetworkPlayerMovement>().isImpostor)
                    if (collision.GetComponent<NetworkPlayerMovement>().IsOwner)
                        border_glow.SetActive(true);
            }
            else if (collision.GetComponent<NetworkPlayerMovement>().IsOwner)
                border_glow.SetActive(true);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (collision.GetComponent<NetworkPlayerMovement>().IsOwner)
                border_glow.SetActive(false);
        }
    }
}
