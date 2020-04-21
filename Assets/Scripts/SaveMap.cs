using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class SaveMap : MonoBehaviour
{
    internal static string mapName = "TestSave";
    internal static void saveMapToText(hexCell[,] map)
    {
        string[] lines = new string[map.GetLength(1)];
        for (int i = 0; i < map.GetLength(1); i++)
        {
            string line = string.Empty;
            for (int j = 0; j <= map.GetLength(0)-1; j++)
            {
                line += map[j, i].type.ToUpper() + map[j, i].landType.PadLeft(2, '0') + map[j, i].tObject + map[j, i].height.ToString().PadLeft(2, '0') + (j != map.GetLength(0)-1 ? HexMap.delimeter.ToString() : "");
            }
            lines[lines.Length - 1 -i] = line;
        }
        System.IO.File.WriteAllLines(HexMap.path + HexMap.mapPath + mapName + ".txt", lines);
        Debug.Log(mapName + " saved at " + HexMap.path + HexMap.mapPath + mapName + ".txt");
    }
}
