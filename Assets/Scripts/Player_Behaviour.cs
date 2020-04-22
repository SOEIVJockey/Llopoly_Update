using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Behaviour : MonoBehaviour
{
    public float currentPlayerSpeed;
    Rigidbody playerRigidbody;

    public float horizontal;
    public float vertical;

    //Player movement state flags
    bool sprinting=false;
    bool sneaking=false;
    bool climbing = false;
    bool readiedUp=false;
    bool cameraActive=false;
    bool collidingWall=false;

    //Magic numbers for movement speeds.
    const float stationarySpeed = 0f;
    const float sneakSpeed = 2.5f;
    const float walkSpeed = 4f;
    const float runSpeed = 6f;

    //Double-Tap code.
    const float doubleTapSpeed = 0.5f; //in seconds
    float timer = 10;
    public int doubleTapKey = 0;
    public KeyCode doubleT;

    // Start is called before the first frame update
    void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();

    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        //Interact
        //Inventory 
    }

    void FixedUpdate()
    {
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");
        if(!climbing)
        {
            playerRigidbody.transform.Translate(horizontal * currentPlayerSpeed * Time.deltaTime, 0, vertical * currentPlayerSpeed * Time.deltaTime);
        }
        else if (climbing)
        {
            playerRigidbody.transform.Translate(horizontal * currentPlayerSpeed * Time.deltaTime, vertical * currentPlayerSpeed * Time.deltaTime, 0);

        }
    }

    void LateUpdate()
    {
        //Camera controls
        if(!climbing)
        {
            if (Input.GetButtonDown("Fire3"))
            {
                currentPlayerSpeed = 0;
                cameraActive = true;
            }
            if (Input.GetButtonUp("Fire3"))
            {
                currentPlayerSpeed = walkSpeed;
                cameraActive = false;
            }
        }

        //Movement
        if(!cameraActive)
        {
            //Dash
            if (Input.GetKeyDown(KeyCode.W))
            {
                if ((timer < doubleTapSpeed) && (doubleTapKey == 1))
                {
                    Debug.Log("Double tap W");
                }
                timer = 0;
                doubleTapKey = 1;
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                if ((timer < doubleTapSpeed) && (doubleTapKey == 2))
                {
                    Debug.Log("Double tap S");
                }
                timer = 0;
                doubleTapKey = 2;
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                if ((timer < doubleTapSpeed) && (doubleTapKey == 3))
                {
                    Debug.Log("Double tap A");
                }
                timer = 0;
                doubleTapKey = 3;
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                if ((timer < doubleTapSpeed) && (doubleTapKey == 4))
                {
                    Debug.Log("Double tap D");
                }
                timer = 0;
                doubleTapKey = 4;
            }
            //Sprinting
            if (Input.GetKeyDown("left shift"))
            {
                if(!climbing)
                {
                    currentPlayerSpeed = runSpeed;
                    sprinting = true;
                }
            }
            if (Input.GetKeyUp("left shift"))
            {
                if (!sneaking) currentPlayerSpeed = walkSpeed;

                else if (sneaking) currentPlayerSpeed = sneakSpeed;
                sprinting = false;
            }
            //Sneaking
            if (Input.GetKeyDown("left ctrl"))
            {
                currentPlayerSpeed = sneakSpeed;
                sneaking = true;
            }
            if (Input.GetKeyUp("left ctrl"))
            {
                if (!sprinting) currentPlayerSpeed = walkSpeed;
                else if (sprinting) currentPlayerSpeed = runSpeed;
                sneaking = false;
            }
        }

        //Climbing
        if(sprinting) sprinting = !sprinting;
        if (sneaking) sneaking = !sneaking;
        currentPlayerSpeed = 4f;
        if (Input.GetKey("space") && collidingWall)
        {
            climbing = true;
            playerRigidbody.useGravity = false;
            playerRigidbody.velocity = Vector3.zero;
        }
        if (Input.GetKey("space") && !collidingWall)
        {
            climbing = false;
            playerRigidbody.useGravity = true;
        }
        if (Input.GetKeyUp("space"))
        {
            playerRigidbody.useGravity = true;
            climbing = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            collidingWall = true;
        }
        if (collision.gameObject.tag == "Ground")
        {
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            collidingWall = false;
        }
    }
}
