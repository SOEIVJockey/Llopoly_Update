using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Behaviour : MonoBehaviour
{
    public float playerSpeed;

    public Rigidbody projectile1;
    Rigidbody playerRigidbody;
    public Transform projectileSpawn;

    public float horizontal;
    public float vertical;

    bool sprinting;
    bool sneaking;
    bool climbing;
    bool readiedUp;
    bool cameraActive;
    bool collidingWall;

    public float doubleTapSpeed = 0.5f; //in seconds
    public float timer;
    public int doubleTapKey;

    public GameObject attackRing;

    // Start is called before the first frame update
    void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        timer = 10;
        doubleTapKey = 0;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        /*
        if (Input.GetButtonDown("Fire1"))
        {
            Rigidbody projectile1Instance;
            projectile1Instance = Instantiate(projectile1, projectileSpawn.position, projectileSpawn.rotation) as Rigidbody;
            projectile1Instance.AddForce(projectileSpawn.forward * 1000);
        }
        */       
    }

    void FixedUpdate()
    {
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");
        if(!climbing)
        {
            playerRigidbody.transform.Translate(horizontal * playerSpeed * Time.deltaTime, 0, vertical * playerSpeed * Time.deltaTime);
        }
        else if (climbing)
        {
            playerRigidbody.transform.Translate(horizontal * playerSpeed * Time.deltaTime, vertical * playerSpeed * Time.deltaTime, 0);

        }

        //Interact
        //Inventory        
    }

    void LateUpdate()
    {
        //Camera controls
        if(!climbing)
        {
            if (Input.GetButtonDown("Fire3"))
            {
                playerSpeed = 0;
                attackRing.SetActive(false);
                cameraActive = true;
            }
            if (Input.GetButtonUp("Fire3"))
            {
                playerSpeed = 4f;
                attackRing.SetActive(true);
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
                    playerSpeed = 6f;
                    sprinting = true;
                }
            }
            if (Input.GetKeyUp("left shift"))
            {
                if (!sneaking) playerSpeed = 4f;

                else if (sneaking) playerSpeed = 2.5f;
                sprinting = false;
            }
            //Sneaking
            if (Input.GetKeyDown("left ctrl"))
            {
                playerSpeed = 2.5f;
                sneaking = true;
            }
            if (Input.GetKeyUp("left ctrl"))
            {
                if (!sprinting) playerSpeed = 4f;
                else if (sprinting) playerSpeed = 6f;
                sneaking = false;
            }
        }

        //Climbing
        if(sprinting) sprinting = !sprinting;
        if (sneaking) sneaking = !sneaking;
        playerSpeed = 4f;
        if (Input.GetKey("space") && collidingWall)
        {
            climbing = true;
            playerRigidbody.useGravity = false;
            playerRigidbody.velocity = Vector3.zero;
        }
        else if (Input.GetKey("space") && !collidingWall)
        {
            climbing = false;
            playerRigidbody.useGravity = true;
        }
        else if (Input.GetKeyUp("space"))
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
