using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadMap : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        HexMap.loadMap(transform, "TestMap01.txt");
    }
}
