using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump : MonoBehaviour
{
    [Range(1, 500)]
    public float jumpVelocity;
    public float fallMultiplier = 2.2f;
    public float lowJumpMultiplier = 2f;
    bool isGrounded;
    bool doubleJumpUsed;

    Vector3 AOVelocity;

    public GameObject attackRing;
    public Rigidbody playerRigidbody;
    // Start is called before the first frame update

    void Awake()
    {
        isGrounded = true;
        doubleJumpUsed = false;
    }

    void Start()
    {
        
    }

    // Update is called once per frame.
    void Update()
    {
        if (isGrounded) attackRing.SetActive(true);
        if (!isGrounded) attackRing.SetActive(false);

        //While repositioning the camera, negate jump.
        if(Input.GetButtonDown("Fire3"))
        {
            jumpVelocity = 0;
        }
        if(Input.GetButtonUp("Fire3"))
        {
            jumpVelocity = 4;
        }

        //Jump        
        if (Input.GetKeyDown("space"))
        {
            if (isGrounded||!doubleJumpUsed)
            {
                AOVelocity = new Vector3(playerRigidbody.velocity.x, 0, playerRigidbody.velocity.z);
                if (!isGrounded)
                {
                    Vector3 Jump = ((Vector3.up) * jumpVelocity * 1.1f);
                    GetComponent<Rigidbody>().velocity = (Jump + AOVelocity);

                    doubleJumpUsed = true;
                }
                else
                {
                    GetComponent<Rigidbody>().velocity = (Vector3.up + AOVelocity) * jumpVelocity;
                    isGrounded = false;
                }
                    
            }
        }
        //Fall physics
        if (playerRigidbody.velocity.y < 0)
        {
            playerRigidbody.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }

        else if (playerRigidbody.velocity.y > 0 && !Input.GetKey("space"))
        {
            playerRigidbody.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        doubleJumpUsed = false;
        if (collision.gameObject.tag == "Ground")
        {
            isGrounded = true;
        }

    }
}
