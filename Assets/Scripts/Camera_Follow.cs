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
    public float autoRotateModifier;
    public float zoomSpeed;
    float yRotation;
    
    public Transform obstruction;



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
        yRotation = Camera.main.transform.rotation.y;
    }

    void FixedUpdate()
    {
        CamControl();
        scroll = Input.GetAxis("Mouse ScrollWheel");
        transform.Translate(0, 0, scroll * Time.deltaTime * zoomSpeed);
        if(!(yRotation % 45 == 0))
        {
            CameraSnap();
        }           
    }

    // Update is called once per frame

    void LateUpdate()
    {
        
    }

    void CamControl() //Reposition camera on middle mouse down.
    {        
        if (Input.GetButton("Fire3")) // This case is only neccessary until the new camera type (Using q and e) is implemented.
        {
            if(Input.GetKey("q"))
            {
                //Rotate the player camera -45 degrees about the y-axis.
            }
            if(Input.GetKey("e"))
            {
                //Rotate the player camera 45 degrees about the y-axis.
            }

            // CAMERA FREE-LOOK - Will be stripped out when above code is done.
            yaw += Input.GetAxis("Mouse X") * rotationSpeed * Time.fixedDeltaTime;
            pitch -= Input.GetAxis("Mouse Y") * rotationSpeed * Time.fixedDeltaTime;
            pitch = Mathf.Clamp(pitch, -20, 20);                                
            target.rotation = Quaternion.Euler(pitch, yaw, 0);
            player.rotation = Quaternion.Euler(0, yaw, 0);
        }
    }
    void CameraSnap()
    {
        float temp;
        float targetRotation;
        float currentRotation = Camera.main.transform.rotation.y;
        targetRotation = ((int)(currentRotation/45))*45;


        temp = Mathf.MoveTowards(Camera.main.transform.rotation.y, targetRotation, rotationSpeed*Time.fixedDeltaTime*autoRotateModifier);
        player.rotation = Quaternion.Euler(0, temp, 0);
    }
}
