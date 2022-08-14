using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrisonScript : MonoBehaviour
{
    public GameObject prisoner = null;
    public GameObject grabPoint;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerInventory>().currentPrison = gameObject;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerInventory>().currentPrison = null;
        }
    }
}
