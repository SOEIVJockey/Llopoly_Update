using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Follow : MonoBehaviour
{
    public Transform target, player;
    public float yaw; //l/r
    public float pitch; //u/d
    public float scroll; //Mouse wheel
    public float rotationSpeed;
    public float zoomSpeed;
    
    public Transform obstruction;
    public GameObject cameraReference;


    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        obstruction = target;
        transform.LookAt(target);
    }

    void Update()
    {
        
    }

    void FixedUpdate()
    {
    }

    // Update is called once per frame

    void LateUpdate()
    {
        CamControl();
        //ViewObstructed(); - Basic Script for seeing through walls.
        scroll = Input.GetAxis("Mouse ScrollWheel");
        transform.Translate(0,0,scroll*Time.deltaTime * zoomSpeed);
    }

    void CamControl() //Reposition camera on middle mouse down.
    {        
        if (Input.GetButton("Fire3"))
        {
            if(Input.GetKey("q"))
            {
                target.localRotation = Quaternion.Euler(0, 1.414f, 0);
            }
            if(Input.GetKey("e"))
            {

            }

            /* CAMERA FREE-LOOK
             * 
             * yaw += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
             * pitch -= Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
             * pitch = Mathf.Clamp(pitch, 0, 50);                                
             * target.rotation = Quaternion.Euler(pitch, yaw, 0);
             * player.rotation = Quaternion.Euler(0, yaw, 0);
             *
             */

            yaw += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            pitch -= Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, 0, 50);                                
            target.rotation = Quaternion.Euler(pitch, yaw, 0);
            player.rotation = Quaternion.Euler(0, yaw, 0);
        }
    }

    void ViewObstructed()
    {        
        RaycastHit hit;
        if (Physics.Raycast(transform.position, target.position - transform.position, out hit, 4.5f))
        {
            if (hit.collider.gameObject.tag != "Player")
            {
                obstruction = hit.transform;
                obstruction.gameObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;

                if (Vector3.Distance(obstruction.position, transform.position) >= 3f && Vector3.Distance(transform.position, target.position) >= 1.5f)
                {
                    transform.Translate(Vector3.forward * zoomSpeed * Time.deltaTime);
                }
            }
            else
            {
                obstruction.gameObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                if (Vector3.Distance(transform.position, target.position) < 4.5f)
                {
                    transform.Translate(Vector3.back * zoomSpeed * Time.deltaTime);
                }
            }
        }        
    }
}
