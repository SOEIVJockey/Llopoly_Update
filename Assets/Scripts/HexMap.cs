using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HexMap : MonoBehaviour
{
    private static char delimeter = ',';
    private static hexCell[,] map;
    private static UnityEngine.Object[] mats = new UnityEngine.Object[0], pMats = mats, prefabs = mats;
    private static string shader = "Standard";
    private static Transform parentMap;

    internal static void LoadMap(string sysPath, string texturesAndPhysicsPath)//call this to load map
    {
        parentMap = new GameObject().transform; 
        readMap(sysPath); //setup tiles      
        loadTexturesAndPhysicsPrefabs(texturesAndPhysicsPath);
        foreach (hexCell cell in map) //apply mesh's
        {
            if (cell.type == "X")//impassable terrain settings
            {
                cell.obj.GetComponent<CapsuleCollider>().height = 100;
                cell.obj.GetComponent<CapsuleCollider>().radius = cell.outerRadius();
            }
            else
            {
                cell.obj.GetComponent<MeshFilter>().mesh = addMesh(cell);
                cell.obj.GetComponent<MeshCollider>().sharedMesh = cell.obj.GetComponent<MeshFilter>().mesh;
                if (mats.Length > int.Parse(cell.landType))
                {
                    cell.obj.GetComponent<MeshRenderer>().material = (Material)assignObject(cell.landType, mats);
                }
                if (pMats.Length > int.Parse(cell.landType))
                {
                    cell.obj.GetComponent<MeshCollider>().material = (PhysicMaterial)assignObject(cell.landType, pMats);
                }
            }
            cell.obj.transform.position = new Vector3(cell.x * cell.innerRadius * 2 + (cell.y % 2 == 0 ? cell.innerRadius : 0), 0, cell.y * 3 / 2f); //must be set after addmesh
            if (cell.tObject != "N")
            {
                var g = (GameObject)assignObject(cell.tObject, prefabs);
                if (g != null)
                {
                    var go = Instantiate(g);
                    go.transform.position = cell.obj.transform.position + (Vector3.up * go.GetComponent<Collider>().bounds.extents.y / 2);
                    go.transform.SetParent(cell.obj.transform);
                }
            }
            cell.obj.transform.SetParent(parentMap);
        }
    }
    private static void loadTexturesAndPhysicsPrefabs(string path)
    {
        mats = Resources.LoadAll(path + "Materials");
        pMats = Resources.LoadAll(path + "Physics");
        prefabs = Resources.LoadAll(path + "Objects");
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
        for (int i = 0; i < map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++) //create tile
            {
                var line = lines[(lines.Length-1) - j]
;                map[i, j] = new hexCell()//X00N00
                {
                    type = line.Split(delimeter)[i][0].ToString().ToUpper(),
                    landType = (line.Split(delimeter)[i][1] +""+ line.Split(delimeter)[i][2]).ToString(),
                    tObject = (line.Split(delimeter)[i][3]).ToString().ToUpper(),
                    height = int.Parse((line.Split(delimeter)[i][4] + "" + line.Split(delimeter)[i][5]).ToString()),
                    x = i,
                    y = j
                };
                map[i, j].obj = (map[i, j].type == "X") ? new GameObject("HexCell:" + i + delimeter + j, typeof(MeshFilter), typeof(MeshRenderer), typeof(CapsuleCollider))
                                                 : new GameObject("HexCell:" + i + delimeter + j + delimeter, typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
                map[i, j].obj.GetComponent<Renderer>().material.shader = Shader.Find(shader);
            }
        }
    }
    private static Mesh addMesh(hexCell hexCell)
    {
        Mesh mesh = new Mesh();
        float innerRadius = hexCell.innerRadius, outerRadius = hexCell.outerRadius();
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
                    if (!(yOverMax || xUnderMin) && (map[hexCell.x - 1, hexCell.y + 1].type != "F" || map[hexCell.x - 1, hexCell.y + 1].type != "U"))
                        retval += map[hexCell.x - 1, hexCell.y + 1].height; //up left
                    else
                        retval += hexCell.height;
                    break;
                case 2:
                    if (!yOverMax && (map[hexCell.x, hexCell.y + 1].type != "F" || map[hexCell.x, hexCell.y + 1].type != "U"))
                        retval += map[hexCell.x, hexCell.y + 1].height; //up right
                    else
                        retval += hexCell.height;
                    break;
                case 3:
                    if (!xOverMax && (map[hexCell.x + 1, hexCell.y].type != "F" || map[hexCell.x + 1, hexCell.y].type != "U"))
                        retval += map[hexCell.x + 1, hexCell.y].height; //centre right
                    else
                        retval += hexCell.height;
                    break;
                case 4:
                    if (!yUnderMin && (map[hexCell.x, hexCell.y - 1].type != "F" || map[hexCell.x, hexCell.y - 1].type != "U"))
                        retval += map[hexCell.x, hexCell.y - 1].height; //down right
                    else
                        retval += hexCell.height;
                    break;
                case 5:
                    if (!(yUnderMin || xUnderMin) && (map[hexCell.x - 1, hexCell.y - 1].type != "F" || map[hexCell.x - 1, hexCell.y - 1].type != "U"))
                        retval += map[hexCell.x - 1, hexCell.y - 1].height;// down left
                    else
                        retval += hexCell.height;
                    break;
                default:
                    if (!xUnderMin && (map[hexCell.x - 1, hexCell.y].type != "F" || map[hexCell.x - 1, hexCell.y].type != "U"))
                        retval += map[hexCell.x - 1, hexCell.y].height; //centre left
                    else
                        retval += hexCell.height;
                    break;
            }
        }
        else
        {
            switch (cell)
            {
                case 1:
                    if (!yOverMax && (map[hexCell.x, hexCell.y + 1].type != "F" || map[hexCell.x, hexCell.y + 1].type != "U"))
                        retval += map[hexCell.x, hexCell.y + 1].height; //up left
                    else
                        retval += hexCell.height;
                    break;
                case 2:
                    if (!(xOverMax || yOverMax) && (map[hexCell.x + 1, hexCell.y + 1].type != "F" || map[hexCell.x + 1, hexCell.y + 1].type != "U"))
                        retval += map[hexCell.x + 1, hexCell.y + 1].height; //up right
                    else
                        retval += hexCell.height;
                    break;
                case 3:
                    if (!xOverMax && (map[hexCell.x + 1, hexCell.y].type != "F" || map[hexCell.x + 1, hexCell.y].type != "U"))
                        retval += map[hexCell.x + 1, hexCell.y].height; //centre right
                    else
                        retval += hexCell.height;
                    break;
                case 4:
                    if (!(yUnderMin || xOverMax) && (map[hexCell.x + 1, hexCell.y - 1].type != "F" || map[hexCell.x + 1, hexCell.y - 1].type != "U"))
                        retval += map[hexCell.x + 1, hexCell.y - 1].height; //down right
                    else
                        retval += hexCell.height;
                    break;
                case 5:
                    if (!(xUnderMin || yUnderMin) && (map[hexCell.x, hexCell.y - 1].type != "F" || map[hexCell.x, hexCell.y - 1].type != "U"))
                        retval += map[hexCell.x, hexCell.y - 1].height;// down left
                    else
                        retval += hexCell.height;
                    break;
                default:
                    if (!xUnderMin && (map[hexCell.x - 1, hexCell.y].type != "F" || map[hexCell.x - 1, hexCell.y].type != "U"))
                        retval += map[hexCell.x - 1, hexCell.y].height; //centre left
                    else
                        retval += hexCell.height;
                    break;
            }
        }
        return retval;
    }
    private static UnityEngine.Object assignObject(string obj, UnityEngine.Object[] objs)
    {
        int i = 0;
        if (objs.Length > 0)
        {
            foreach (var item in objs)
            {
                if (obj == objs[i].name)
                {
                    return objs[i];
                }
                i++;
            }
        }
        return null;
    }
}

internal class hexCell
{
    [SerializeField]
    internal string type = string.Empty, tObject = string.Empty, landType = string.Empty;
    internal GameObject obj = null;
    [SerializeField]
    internal int x = 0, y = 0;
    internal float innerRadius = 1;
    internal float outerRadius()
    {
        return innerRadius + (1547 / 10000);
    }
    internal int height = 0;
}