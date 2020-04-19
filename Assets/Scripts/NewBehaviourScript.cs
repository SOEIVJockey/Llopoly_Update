using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        HexMap.LoadMap("Assets/Scripts/TestMap01.txt", "Map/");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
