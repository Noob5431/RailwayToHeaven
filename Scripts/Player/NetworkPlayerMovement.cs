using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayerInventory))]

public class NetworkPlayerMovement : NetworkBehaviour
{
    [SerializeField]
    NetworkVariable<Vector2> networkVelocity = new NetworkVariable<Vector2>();
    NetworkVariable<float> networkHorizontal = new NetworkVariable<float>();

    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpPower = 16f;
    private bool isFacingRight = true;
    [SerializeField]
    private GameObject canvas;
    private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    Vector2 oldVelocity = Vector2.zero;
    PlayerInventory playerInventory;
    [SerializeField]
    public grabScript grabArea;
    [SerializeField]
    public bool isGrabbing = false;
    [SerializeField]
    private GameObject grabPoint;
    [SerializeField]
    GameObject globalOtherPlayerGrabPoint;
    public bool _boolIsBeingGrabbed = false;
    public bool isInPrison = false;
    GameObject globalPrison;
    public GameObject grabbedPlayer;
    GameObject startPoint;
    [SerializeField]
    public bool isInGame = false;
    public bool isIncapacitated = false;
    public bool isImpostor = false;
    NetworkVariable<bool> networkIsImpostor = new NetworkVariable<bool>();
    Animator playerAnimator;
    NetworkVariable<bool> networkIsJumping = new NetworkVariable<bool>();
    NetworkVariable<bool> networkIsFalling = new NetworkVariable<bool>();
    public inGameUi ui_canvas;
    public itemStorage coalFurnance;
    public GameObject horn;
    //NetworkVariable<bool> networkIsBeingGrabbed = new NetworkVariable<bool>();

    Cinemachine.CinemachineVirtualCamera currentCamera;

    private void Awake()
    {
        playerAnimator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerInventory = GetComponent<PlayerInventory>();
    }
    private void Start()
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete += ResetPosition;
        grabArea.gameObject.SetActive(false);
        if (IsOwner && SceneManager.GetActiveScene().buildIndex == 1)
        {
            rb.isKinematic = true;
        }
    }

    private void ResetPosition(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (sceneName == "MainScene")
        {
            grabArea.gameObject.SetActive(true);
            coalFurnance = GameObject.Find("CoalFurnance").GetComponent<itemStorage>();
            ui_canvas = GameObject.Find("ui_canvas").GetComponent<inGameUi>();
            isInGame = true;
            startPoint = GameObject.Find("StartPoint");
            currentCamera = GameObject.Find("MainCamera").GetComponent<Cinemachine.CinemachineVirtualCamera>();
            if (IsOwner)
            {
                UpdateIsImpostorServerRpc(isImpostor);
                rb.isKinematic = false;
                transform.position = startPoint.transform.position;
                currentCamera.Follow = gameObject.transform;
            }
            
        }
    }
    [ServerRpc]
    void UpdateIsImpostorServerRpc(bool value)
    {
        UpdateIsImpostorClientRpc(value);
    }
    [ClientRpc]
    void UpdateIsImpostorClientRpc(bool value)
    {
        isImpostor = value;
    }
    void Update()
    {
        if (isInGame)
        {
            //move to ResetPosition in production for efficiency
            //!!
            //!!
            //!!
            //!!
            
            if (IsOwner && isImpostor)
            {
                currentCamera.gameObject.GetComponent<Cinemachine.CinemachineConfiner>().m_ConfineScreenEdges = false;
            }
            if (IsClient && IsOwner && !_boolIsBeingGrabbed)
            {
                if (!isIncapacitated)
                {
                    float horizontal;
                    Vector2 newVelocity = Vector2.zero;

                    horizontal = Input.GetAxisRaw("Horizontal");
                    newVelocity.x = horizontal * speed;
                    if (Input.GetButtonDown("Jump") && IsGrounded())
                    {
                        newVelocity = new Vector2(newVelocity.x, jumpPower);
                    }
                    if (newVelocity != oldVelocity)
                    {
                        //for client network transform (client authoritative)
                        if (newVelocity.y < 0.1f)
                            rb.velocity = new Vector2(newVelocity.x, rb.velocity.y);
                        else rb.velocity = new Vector2(newVelocity.x, newVelocity.y);
                        Flip(horizontal);
                        //UpdatePlayerVelocityServerRpc(newVelocity); <<<< in case of network transorm(server authoritative)
                        UpdateHorizontalServerRpc(horizontal);
                        oldVelocity = newVelocity;
                    }
                }
                else
                {
                    rb.velocity = Vector2.zero;
                }
            }
            if (IsOwner)
            {
                if (Input.GetButtonDown("Use"))
                {
                    UpdateIsUsingServerRpc(true);
                }
                if (Input.GetButtonDown("Throw"))
                {
                    UpdateIsThrowingServerRpc(true);
                }
                if (Input.GetButtonDown("Grab") && !isIncapacitated)
                {
                    GrabServerRpc();
                }
                if (isImpostor && Input.GetButtonDown("Sabotage"))
                {
                    SabotageServerRpc();
                }
            }
            if (IsOwner && _boolIsBeingGrabbed)
            {
                rb.velocity = Vector2.zero;

            }
            if (_boolIsBeingGrabbed)
            {
                canvas.transform.localScale = new Vector3(Math.Sign( transform.localScale.x), canvas.transform.localScale.y, canvas.transform.localScale.z);
                transform.position = globalOtherPlayerGrabPoint.transform.position;
                transform.localScale = globalOtherPlayerGrabPoint.GetComponentInParent<NetworkPlayerMovement>().gameObject.transform.localScale;
                transform.rotation = globalOtherPlayerGrabPoint.transform.rotation;
                grabPoint.transform.localScale = new Vector3(transform.localScale.x, grabPoint.transform.localScale.y, grabPoint.transform.localScale.z);
            }
            if (!IsOwner)
            {
                Flip(networkHorizontal.Value);
            }
            //in case of server authoritative
            /*if (IsServer)
            {
                if (networkVelocity.Value.y<0.1f)
                    rb.velocity = new Vector2(networkVelocity.Value.x, rb.velocity.y);
                else rb.velocity = new Vector2(networkVelocity.Value.x, networkVelocity.Value.y);
            }*/
            if (networkHorizontal.Value != 0)
                playerAnimator.SetBool("isWalking", true);
            else playerAnimator.SetBool("isWalking", false);
            if (IsOwner)
            {
                if (rb.velocity.y > 0.1f)
                {
                    playerAnimator.SetBool("isJumping", true);
                    UpdateIsJumpingServerRpc(true);
                }
                else
                {
                    playerAnimator.SetBool("isJumping", false);
                    UpdateIsJumpingServerRpc(false);
                }
                if (rb.velocity.y < -0.1f)
                {
                    playerAnimator.SetBool("isFalling", true);
                    UpdateIsFallingServerRpc(true);                }
                else
                {
                    playerAnimator.SetBool("isFalling", false);
                    UpdateIsFallingServerRpc(false);
                }

            }
            else
            {
                playerAnimator.SetBool("isJumping", networkIsJumping.Value);
                playerAnimator.SetBool("isFalling", networkIsFalling.Value);
            }
            playerAnimator.SetBool("isBeingGrabbed", _boolIsBeingGrabbed);
            playerAnimator.SetBool("isHurt", isIncapacitated);

        }
    }
    [ServerRpc]
    void UpdateIsJumpingServerRpc(bool value)
    {
        networkIsJumping.Value = value;
    }
    [ServerRpc]
    void UpdateIsFallingServerRpc(bool value)
    {
        networkIsFalling.Value = value;
    }
    [ServerRpc]
    private void SabotageServerRpc()
    {
        SabotageClientRpc();
    }

    [ClientRpc]
    private void SabotageClientRpc()
    {
        playerInventory.Sabotage();
    }

    [ServerRpc]
    public void GrabServerRpc()
    {
        GrabClientRpc();
    }

    [ClientRpc]
    private void GrabClientRpc()
    {
            if (grabArea.grabbedPlayer && !isGrabbing && playerInventory.inventory[0] == false && !grabArea.grabbedPlayer.GetComponent<NetworkPlayerMovement>()._boolIsBeingGrabbed)
            {
                grabbedPlayer = grabArea.grabbedPlayer;
                playerInventory.inventory[0] = true;
                isGrabbing = true;
                grabArea.grabbedPlayer.GetComponent<NetworkPlayerMovement>().globalOtherPlayerGrabPoint = grabPoint;
                grabArea.grabbedPlayer.GetComponent<NetworkPlayerMovement>()._boolIsBeingGrabbed = true;
            }
            else if (isGrabbing && grabbedPlayer.GetComponent<NetworkPlayerMovement>()._boolIsBeingGrabbed)
            {
                isGrabbing = false;
                playerInventory.inventory[0] = false;
                grabbedPlayer.GetComponent<NetworkPlayerMovement>().globalOtherPlayerGrabPoint = null;
                grabbedPlayer.GetComponent<NetworkPlayerMovement>()._boolIsBeingGrabbed = false;
                grabbedPlayer.transform.rotation = Quaternion.identity;
                grabbedPlayer.transform.localScale = new Vector3(Math.Abs(transform.localScale.x), Math.Abs(transform.localScale.y), Math.Abs(transform.localScale.z));
                grabbedPlayer.GetComponent<NetworkPlayerMovement>().isFacingRight = true;
                grabbedPlayer = null;
            }
        
    }

    void LocalGrab()
    {
        if (!isGrabbing && playerInventory.inventory[0] == false && (!grabbedPlayer.GetComponent<NetworkPlayerMovement>()._boolIsBeingGrabbed || grabbedPlayer.GetComponent<NetworkPlayerMovement>().isInPrison))
        {
            playerInventory.inventory[0] = true;
            isGrabbing = true;
            grabbedPlayer.GetComponent<NetworkPlayerMovement>().globalOtherPlayerGrabPoint = grabPoint;
            grabbedPlayer.GetComponent<NetworkPlayerMovement>()._boolIsBeingGrabbed = true;
        }
        else if (isGrabbing && grabbedPlayer.GetComponent<NetworkPlayerMovement>()._boolIsBeingGrabbed)
        {
            isGrabbing = false;
            playerInventory.inventory[0] = false;
            grabbedPlayer.GetComponent<NetworkPlayerMovement>().globalOtherPlayerGrabPoint = null;
            grabbedPlayer.GetComponent<NetworkPlayerMovement>()._boolIsBeingGrabbed = false;
            grabbedPlayer.transform.rotation = Quaternion.identity;
            grabbedPlayer.transform.localScale = new Vector3(Math.Abs(transform.localScale.x), Math.Abs(transform.localScale.y), Math.Abs(transform.localScale.z));
            grabbedPlayer.GetComponent<NetworkPlayerMovement>().isFacingRight = true;
            grabbedPlayer = null;
        }
    }    
    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
    }
    private void Flip(float horizontal)
    {
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
            canvas.transform.localScale = new Vector3(Math.Sign(localScale.x), canvas.transform.localScale.y, canvas.transform.localScale.z);
            grabPoint.transform.localScale = new Vector3(localScale.x*-1f, grabPoint.transform.localScale.y, grabPoint.transform.localScale.z);
        }
    }


    [ServerRpc]
    void UpdateIsGrabbingServerRpc(bool value)
    {
        playerInventory.inventory[0] = value;
        isGrabbing = value;
        UpdateIsGrabbingClientRpc(value);
    }

    [ClientRpc]
    void UpdateIsGrabbingClientRpc(bool value)
    {
        if (!IsOwner)
        {
            playerInventory.inventory[0] = value;
            isGrabbing = value;
        }
    }
    [ServerRpc]
    public void UpdatePlayerVelocityServerRpc(Vector2 newVelocity)
    {
        networkVelocity.Value = newVelocity;
    }
    [ServerRpc]
    public void UpdateHorizontalServerRpc(float horizontal)
    {
        networkHorizontal.Value = horizontal;
    }
    [ServerRpc]
    public void UpdateIsUsingServerRpc(bool currentIsUsing)
    {
        updateInventoryClientRpc();
    }
    [ServerRpc]
    public void UpdateIsThrowingServerRpc(bool currentIsThrowing)
    {
        playerInventory.throwItem();
        throwItemClientRpc();
    }
    [ClientRpc]
    public void updateInventoryClientRpc()
    {
        playerInventory.updateInventory();
    }
    [ClientRpc]
    public void throwItemClientRpc()
    {
        playerInventory.throwItem();
    }

    public void GoToPrison(GameObject prison)
    {
        globalOtherPlayerGrabPoint.GetComponentInParent<NetworkPlayerMovement>().LocalGrab();
        _boolIsBeingGrabbed = true;
        globalOtherPlayerGrabPoint = prison.GetComponent<PrisonScript>().grabPoint;
        prison.GetComponent<PrisonScript>().prisoner = gameObject;
        GetComponent<SpriteRenderer>().sortingOrder = 0;
        globalPrison = prison;
        isInPrison = true;
    }

    public void ExitPrison(GameObject player)
    {
        player.GetComponent<NetworkPlayerMovement>().grabArea.grabbedPlayer = gameObject;
        player.GetComponent<NetworkPlayerMovement>().grabbedPlayer = gameObject;
        player.GetComponent<NetworkPlayerMovement>().LocalGrab();
        globalOtherPlayerGrabPoint = player.GetComponent<NetworkPlayerMovement>().grabPoint;
        _boolIsBeingGrabbed = true;
        globalPrison.GetComponent<PrisonScript>().prisoner = null;
        GetComponent<SpriteRenderer>().sortingOrder = 3;
        isInPrison = false;
    }
}
