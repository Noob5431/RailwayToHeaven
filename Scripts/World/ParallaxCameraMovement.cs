using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ParallaxCameraMovement : MonoBehaviour
{
    [SerializeField]
    float speed;
    GameObject player;
    GameObject mainCamera;
    Vector2 previousPosition = new Vector2();
    Vector2 currentPosition = new Vector2();

    private void Start()
    {
        player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().gameObject;
        mainCamera = GameObject.Find("MainCamera");
        previousPosition = mainCamera.transform.position;
    }

    private void Update()
    {
        currentPosition = mainCamera.transform.position;
        float deltaX = currentPosition.x - previousPosition.x;
        previousPosition = currentPosition;
        transform.position = new Vector2(transform.position.x + speed * Time.deltaTime + deltaX/2.5f, transform.position.y);
    }
}
