using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Daylight_Behavior : MonoBehaviour
{
    public Transform sun;
    public Light sunSettings;
    public float sunSpeed;
    float rotateSun;

    // Start is called before the first frame update
    void Start()
    {
        rotateSun = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        rotateSun += sunSpeed * Time.fixedDeltaTime;
        sun.rotation = Quaternion.Euler(0, rotateSun, 0);
        sunSettings.intensity = 0.5f + Mathf.Sin(sun.rotation.y / Mathf.PI) / 2;
    }
}
