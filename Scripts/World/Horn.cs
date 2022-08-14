using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Horn : MonoBehaviour
{
    AudioSource hornSound;

    private void Start()
    {
        hornSound = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            collision.GetComponent<NetworkPlayerMovement>().horn = gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<NetworkPlayerMovement>().horn = null;
        }
    }

    public void activateSound()
    {
        if(!hornSound.isPlaying)
            hornSound.Play();
    }
}
