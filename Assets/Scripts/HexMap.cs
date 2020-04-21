using System;
using System.Collections.Generic;
using UnityEngine;

public class HexMap : MonoBehaviour
{
    private static char delimeter = ',';
    private static hexCell[,] map;
    private static UnityEngine.Object[] mats = new UnityEngine.Object[0], pMats = mats, prefabs = mats;
    private static string shader = "Standard";
    private static Transform parentMap;

    internal static void LoadMap(Transform t, string sysPath, string texturesAndPhysicsPath)//call this to load map
    {
        parentMap = t;
        readMap(sysPath); //setup tiles      
        loadTexturesAndPhysicsPrefabs(texturesAndPhysicsPath);
        foreach (hexCell cell in map) //apply mesh's
        {
            bool flat = cell.type == "F" || cell.type == "U";
            if (cell.type == "X")//impassable component settings
            {
                cell.obj.GetComponent<CapsuleCollider>().height = 100;
                cell.obj.GetComponent<CapsuleCollider>().radius = cell.outerRadius();
                cell.obj.GetComponent<MeshFilter>().mesh = addHexMesh(cell, flat);
            }
            else // all other tile component settings
            {
                cell.obj.GetComponent<MeshFilter>().mesh = addHexMesh(cell, flat);
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
            if (flat) //forceflat's need walls
            {
                addWalls(cell);
            }
            if (cell.tObject != "N") //cell object spawn
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
                var line = lines[(lines.Length - 1) - j];
                map[i, j] = new hexCell()
                {
                    type = line.Split(delimeter)[i][0].ToString().ToUpper(),
                    landType = (line.Split(delimeter)[i][1] + "" + line.Split(delimeter)[i][2]).ToString(),
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

    private static Mesh addHexMesh(hexCell cell, bool flat)
    {
        Mesh mesh = new Mesh();
        float innerRadius = cell.innerRadius, outerRadius = cell.outerRadius();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        List<int> triangles = new List<int>();
        Vector3 center = cell.obj.transform.localPosition;
        Vector3[] corners = getCorners(cell);

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
            if (vertices[i] == cell.obj.transform.localPosition) //set centre point height
            {
                float x = flat ? cell.height : (getHeight(cell, 1) + getHeight(cell, 2) + getHeight(cell, 3) + getHeight(cell, 4) + getHeight(cell, 5) + getHeight(cell, 6) + cell.height) / 7;
                vertices[i] = new Vector3(vertices[i].x, Math.Max(x, cell.height), vertices[i].y);
            }
        }
		
        mesh.vertices = vertices.ToArray(); //set mesh properties to mesh
        mesh.triangles = triangles.ToArray();
        mesh.uv = uv.ToArray();
        mesh.RecalculateNormals(); //redraw
        return mesh;
    }
    private static void addWalls(hexCell cell)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        List<int> triangles = new List<int>();
        Mesh wallMesh = new Mesh();
        Vector3[] corners = getCorners(cell);
        for (int i = 0; i < 6; i++) //loop through hexagon sides
        {
            int startIndex = vertices.Count;
            if (corners[i] != corners[i] + (heightOverPoint(cell, corners[i]) ? -(Vector3.up * cell.height) : (Vector3.up * (cell.height - corners[i].y)))) //if i am highest i am equal which is fine unless i am actually equal and my height is 0
            {
                vertices.Add(corners[i]); //left
                vertices.Add(corners[i] + (heightOverPoint(cell, corners[i]) ? -(Vector3.up * cell.height) : (Vector3.up * (cell.height - corners[i].y)))); //bottom left
                vertices.Add(corners[1 + i]);//right            
                vertices.Add(corners[1 + i] + (heightOverPoint(cell, corners[i + 1]) ? -(Vector3.up * cell.height) : (Vector3.up * (cell.height - corners[i + 1].y)))); //bottom right

                uv.Add(corners[i]); //left
                uv.Add(corners[i] + (heightOverPoint(cell, corners[i]) ? -(Vector3.up * cell.height) : (Vector3.up * (cell.height - corners[i].y)))); //bottom left
                uv.Add(corners[i + 1]); //right
                uv.Add(corners[i + 1] + (heightOverPoint(cell, corners[i + 1]) ? -(Vector3.up * cell.height) : (Vector3.up * (cell.height - corners[i + 1].y)))); //bottom right

                if (heightOverPoint(cell, corners[0]))
                {
                    triangles.Add(startIndex + 3);
                    triangles.Add(startIndex + 2);
                    triangles.Add(startIndex + 0);
					
                    triangles.Add(startIndex + 1);
                    triangles.Add(startIndex + 3);
                    triangles.Add(startIndex + 0);
                }
                else
                {
                    triangles.Add(startIndex + 0);
                    triangles.Add(startIndex + 3);
                    triangles.Add(startIndex + 1);
					
                    triangles.Add(startIndex + 0);
                    triangles.Add(startIndex + 2);
                    triangles.Add(startIndex + 3);
                }
            }
        }
        wallMesh.vertices = vertices.ToArray();
        wallMesh.triangles = triangles.ToArray(); //add gamebject child with mesh
        wallMesh.uv = uv.ToArray();
        wallMesh.RecalculateNormals();

        GameObject x = new GameObject("Wall", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider)); //child object setup
        x.GetComponent<MeshFilter>().mesh = wallMesh;
        x.GetComponent<MeshCollider>().sharedMesh = wallMesh;
        x.GetComponent<MeshRenderer>().material = cell.obj.GetComponent<MeshRenderer>().material;
        x.GetComponent<Renderer>().material.shader = Shader.Find(shader);
        x.transform.SetParent(cell.obj.transform);
        x.transform.localPosition = Vector3.zero;
        x.tag = cell.type == "F" ? "Wall" : "noClimbWall";
    }
    private static Vector3[] getCorners(hexCell cell) {
        return new Vector3[] //hexagon coordinates from centre + maxheight of point neighbours + self
        {
            new Vector3(0f, Math.Max(Math.Max(cell.height, getHeight(cell, 1)), getHeight(cell, 2)), cell.outerRadius()),
            new Vector3(cell.innerRadius, Math.Max(Math.Max(cell.height, getHeight(cell, 2)), getHeight(cell, 3)), 0.5f * cell.outerRadius()),
            new Vector3(cell.innerRadius, Math.Max(Math.Max(cell.height, getHeight(cell, 3)), getHeight(cell,4)), -0.5f * cell.outerRadius()),
            new Vector3(0f, Math.Max(Math.Max(cell.height, getHeight(cell, 4)), getHeight(cell, 5)), -cell.outerRadius()),
            new Vector3(-cell.innerRadius, Math.Max(Math.Max(cell.height, getHeight(cell, 5)), getHeight(cell, 6)), -0.5f * cell.outerRadius()),
            new Vector3(-cell.innerRadius, Math.Max(Math.Max(cell.height, getHeight(cell, 6)), getHeight(cell, 1)), 0.5f * cell.outerRadius()),
            new Vector3(0f, Math.Max(Math.Max(cell.height, getHeight(cell, 1)), getHeight(cell, 2)), cell.outerRadius())
        };
    }
    private static bool heightOverPoint(hexCell cell, Vector3 v)
    {
        return cell.height >= v.y;
    }
    private static float getHeight(hexCell cell, int c = 0)
    {
        bool xOverMax = cell.x + 1 > map.GetLength(0) - 1, xUnderMin = cell.x - 1 < 0, yOverMax = cell.y + 1 > map.GetLength(1) - 1, yUnderMin = cell.y - 1 < 0; //stops out of range exceptions
        if (cell.y % 2 != 0)
        {
            switch (c)
            {
                case 1://up left
                    return (!(yOverMax || xUnderMin) && !(map[cell.x - 1, cell.y + 1].type == "F" || map[cell.x - 1, cell.y + 1].type == "U")) ? map[cell.x - 1, cell.y + 1].height : cell.height;
                case 2://up right
                    return (!yOverMax && !(map[cell.x, cell.y + 1].type == "F" || map[cell.x, cell.y + 1].type == "U")) ? map[cell.x, cell.y + 1].height : cell.height;
                case 3://centre right
                    return (!xOverMax && !(map[cell.x + 1, cell.y].type == "F" || map[cell.x + 1, cell.y].type == "U")) ? map[cell.x + 1, cell.y].height : cell.height;
                case 4://down right
                    return (!yUnderMin && !(map[cell.x, cell.y - 1].type == "F" || map[cell.x, cell.y - 1].type == "U")) ? map[cell.x, cell.y - 1].height : cell.height;
                case 5:// down left
                    return (!(yUnderMin || xUnderMin) && !(map[cell.x - 1, cell.y - 1].type == "F" || map[cell.x - 1, cell.y - 1].type == "U")) ? map[cell.x - 1, cell.y - 1].height : cell.height;
                default://centre left
                    return (!xUnderMin && !(map[cell.x - 1, cell.y].type == "F" || map[cell.x - 1, cell.y].type == "U")) ? map[cell.x - 1, cell.y].height : cell.height;
            }
        }
        else
        {
            switch (c)
            {
                case 1://up left
                    return (!yOverMax && !(map[cell.x, cell.y + 1].type == "F" || map[cell.x, cell.y + 1].type == "U")) ? map[cell.x, cell.y + 1].height : cell.height;
                case 2://up right
                    return (!(xOverMax || yOverMax) && !(map[cell.x + 1, cell.y + 1].type == "F" || map[cell.x + 1, cell.y + 1].type == "U")) ? map[cell.x + 1, cell.y + 1].height : cell.height;
                case 3://centre right
                    return (!xOverMax && !(map[cell.x + 1, cell.y].type == "F" || map[cell.x + 1, cell.y].type == "U")) ? map[cell.x + 1, cell.y].height : cell.height;
                case 4://down right
                    return (!(yUnderMin || xOverMax) && !(map[cell.x + 1, cell.y - 1].type == "F" || map[cell.x + 1, cell.y - 1].type == "U")) ? map[cell.x + 1, cell.y - 1].height : cell.height;
                case 5:// down left
                    return (!(xUnderMin || yUnderMin) && !(map[cell.x, cell.y - 1].type == "F" || map[cell.x, cell.y - 1].type == "U")) ? map[cell.x, cell.y - 1].height : cell.height;
                default://centre left
                    return (!xUnderMin  && !(map[cell.x - 1, cell.y].type == "F" || map[cell.x - 1, cell.y].type == "U")) ? map[cell.x - 1, cell.y].height : cell.height;
            }
        }
    }

    private static UnityEngine.Object assignObject(string obj, UnityEngine.Object[] objs) //return associated object to assign
    {
        int i = 0;
        bool ps = false;
        if (objs.Length > 0)
        {
            foreach (var item in objs)
            {
                if (obj == objs[i].name)
                {
                    if (obj != "S")
                    {
                        return objs[i];
                    }
                    else if(!ps)
                    {
                        return objs[i];
                    }
                    else
                    {
                        Debug.Log("Player already spawned.");
                    }
                }
                i++;
            }
        }
        return null;
    }
}

internal class hexCell
{
    internal string type = string.Empty, tObject = string.Empty, landType = string.Empty;
    internal GameObject obj = null;
    internal int x = 0, y = 0;
    internal float innerRadius = 1;
    internal float outerRadius()
    {
        return innerRadius + (innerRadius* 0.1547f);
    }
    internal int height = 0;
}