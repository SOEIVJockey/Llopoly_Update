using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMap : MonoBehaviour
{
    //private string sysPath = "Assets/" + "1.txt";;
    private static char delimeter = ',';
    private static hexCell[,] map;

    internal static void LoadMap(string sysPath)//call this to load map
    {
        readMap(sysPath); //setup tiles
        foreach (hexCell cell in map) //apply mesh's
        {
            if (cell.obj != null)
            {
                cell.obj.GetComponent<MeshFilter>().mesh = addMesh(cell);
                cell.obj.GetComponent<MeshCollider>().sharedMesh = cell.obj.GetComponent<MeshFilter>().mesh;
                cell.obj.transform.position = new Vector3(cell.x * cell.innerRadius() * 2 + (cell.y % 2 == 0 ? cell.innerRadius() : 0), 0, cell.y * 3 / 2f);
            }
        }
    }

    private static void readMap(string path)
    {
        int mapHeight = 0, mapWidth = 1;
        string[] lines = System.IO.File.ReadAllLines(path);
        foreach (string line in lines) //mapheight is no. of lines
        {
            mapHeight++;
        }
        foreach (char c in lines[0]) //map width is no of csv phrases
        {
            if (c == delimeter)
            {
                mapWidth++;
            }
        }
        map = new hexCell[mapWidth, mapHeight];
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++) //create tile
            {
                if (lines[j].Split(delimeter)[i] != "x")
                {
                    map[i, j] = new hexCell()
                    {
                        obj = new GameObject("HexCell:" + i + delimeter + j + delimeter + "H" + int.Parse(lines[j].Split(delimeter)[i]), typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider)),
                        height = int.Parse(lines[j].Split(delimeter)[i]),
                        x = i,
                        y = j
                    };
                    map[i, j].obj.GetComponent<Renderer>().material.shader = Shader.Find("Standard");
                }
                else
                {
                    map[i, j] = new hexCell();
                }
            }
        }
    }
    private static Mesh addMesh(hexCell hexCell)
    {
        Mesh mesh = new Mesh();
        float innerRadius = hexCell.innerRadius(), outerRadius = hexCell.outerRadius;
        Vector3[] corners = new Vector3[] //hexagon coordinates from centre + maxheight of point neighbours + self
        {
            new Vector3(0f, Math.Max(Math.Max(hexCell.height, getHeight(hexCell, 1)), getHeight(hexCell, 2)), outerRadius),
            new Vector3(innerRadius, Math.Max(Math.Max(hexCell.height, getHeight(hexCell, 2)), getHeight(hexCell, 3)), 0.5f * outerRadius),
            new Vector3(innerRadius, Math.Max(Math.Max(hexCell.height, getHeight(hexCell, 3)), getHeight(hexCell,4)), -0.5f * outerRadius),
            new Vector3(0f, Math.Max(Math.Max(hexCell.height, getHeight(hexCell, 4)), getHeight(hexCell, 5)), -outerRadius),
            new Vector3(-innerRadius, Math.Max(Math.Max(hexCell.height, getHeight(hexCell, 5)), getHeight(hexCell, 6)), -0.5f * outerRadius),
            new Vector3(-innerRadius, Math.Max(Math.Max(hexCell.height, getHeight(hexCell, 6)), getHeight(hexCell, 1)), 0.5f * outerRadius),
            new Vector3(0f, Math.Max(Math.Max(hexCell.height, getHeight(hexCell, 1)), getHeight(hexCell, 2)), outerRadius)
        };

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        List<int> triangles = new List<int>();
        Vector3 center = hexCell.obj.transform.localPosition;
        for (int i = 0; i < 6; i++) //loop through triangles
        {
            int vertexIndex = vertices.Count;
            vertices.Add(center);
            vertices.Add(center + corners[i]);
            vertices.Add(center + corners[i + 1]);
            uv.Add(center);
            uv.Add(center + corners[i]);
            uv.Add(center + corners[i + 1]);
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);
        }
        for (int i = 0; i < vertices.Count; i++)
        {
           if (vertices[i] == hexCell.obj.transform.localPosition) //set centre point height
            {
                float x = (getHeight(hexCell, 1) + getHeight(hexCell, 2) + getHeight(hexCell, 3) + getHeight(hexCell, 4) + getHeight(hexCell, 5) + getHeight(hexCell, 6)) / 6;
                vertices[i] = new Vector3(vertices[i].x, Math.Max(x, hexCell.height), vertices[i].y);
            }
        }
        mesh.vertices = vertices.ToArray(); //set mesh properties to mesh
        mesh.triangles = triangles.ToArray();
        mesh.uv = uv.ToArray();
        mesh.RecalculateNormals(); //redraw
        return mesh;
    }
    private static float getHeight(hexCell hexCell, int cell = 0)
    {
        bool xOverMax = hexCell.x + 1 > map.GetLength(0) - 1, xUnderMin = hexCell.x - 1 < 0, yOverMax = hexCell.y + 1 > map.GetLength(1) - 1, yUnderMin = hexCell.y - 1 < 0; //stops out of range exceptions
        float retval = 0;
        if (hexCell.y % 2 != 0)
        {
            switch (cell)
            {
                case 1:
                    if (!(yOverMax || xUnderMin)) retval += map[hexCell.x - 1, hexCell.y + 1].height; //up left
                    else { retval += hexCell.height; }
                    break;
                case 2:
                    if (!yOverMax) retval += map[hexCell.x, hexCell.y + 1].height; //up right
                    else { retval += hexCell.height; }
                    break;
                case 3:
                    if (!xOverMax) retval += map[hexCell.x + 1, hexCell.y].height; //centre right
                    else { retval += hexCell.height; }
                    break;
                case 4:
                    if (!yUnderMin) retval += map[hexCell.x, hexCell.y - 1].height; //down right
                    else { retval += hexCell.height; }
                    break;
                case 5:
                    if (!(yUnderMin || xUnderMin)) retval += map[hexCell.x -1, hexCell.y - 1].height;// down left
                    else { retval += hexCell.height; }
                    break;
                default:
                    if (!xUnderMin) retval += map[hexCell.x - 1, hexCell.y].height; //centre left
                    else { retval += hexCell.height; }
                    break;
            }
        }
        else
        {
            switch (cell)
            {
                case 1:
                    if (!yOverMax) retval += map[hexCell.x, hexCell.y + 1].height; //up left
                    else { retval += hexCell.height; }
                    break;
                case 2:
                    if (!(xOverMax || yOverMax)) retval += map[hexCell.x + 1, hexCell.y + 1].height; //up right
                    else { retval += hexCell.height; }
                    break;
                case 3:
                    if (!xOverMax) retval += map[hexCell.x + 1, hexCell.y].height; //centre right
                    else { retval += hexCell.height; }
                    break;
                case 4:
                    if (!(yUnderMin || xOverMax)) retval += map[hexCell.x + 1, hexCell.y - 1].height; //down right
                    else { retval += hexCell.height; }
                    break;
                case 5:
                    if (!(xUnderMin || yUnderMin)) retval += map[hexCell.x, hexCell.y - 1].height;// down left
                    else { retval += hexCell.height; }
                    break;
                default:
                    if (!xUnderMin) retval += map[hexCell.x - 1, hexCell.y].height; //centre left
                    else { retval += hexCell.height; }
                    break;
            }
        }
        return retval;
    }
}
internal class hexCell
{
    internal GameObject obj = null;
    internal float outerRadius = 1;
    internal int x = 0, y = 0;
    internal float innerRadius()
    {
        return outerRadius * 0.866025404f;
    }
    internal int height = 0;   
}