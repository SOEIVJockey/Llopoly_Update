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
            if (cell.type == "X")//impassable terrain settings
            {
                cell.obj.GetComponent<CapsuleCollider>().height = 100;
                cell.obj.GetComponent<CapsuleCollider>().radius = cell.outerRadius();
                cell.obj.GetComponent<MeshFilter>().mesh = addMesh(cell, flat);
            }
            else
            {
                cell.obj.GetComponent<MeshFilter>().mesh = addMesh(cell, flat);
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
            if (flat)
            {
                addWalls(cell);
            }
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
    private static Mesh addMesh(hexCell hexCell, bool flat)
    {
        Mesh mesh = new Mesh();
        float innerRadius = hexCell.innerRadius, outerRadius = hexCell.outerRadius();
        Vector3[] corners = new Vector3[] //hexagon coordinates from centre + maxheight of point neighbours + self
        {
            new Vector3(0f, flat ? hexCell.height : Math.Max(Math.Max(hexCell.height, getHeight(hexCell, 1)), getHeight(hexCell, 2)), outerRadius),
            new Vector3(innerRadius, flat ? hexCell.height : Math.Max(Math.Max(hexCell.height, getHeight(hexCell, 2)), getHeight(hexCell, 3)), 0.5f * outerRadius),
            new Vector3(innerRadius, flat ? hexCell.height : Math.Max(Math.Max(hexCell.height, getHeight(hexCell, 3)), getHeight(hexCell,4)), -0.5f * outerRadius),
            new Vector3(0f, flat ? hexCell.height : Math.Max(Math.Max(hexCell.height, getHeight(hexCell, 4)), getHeight(hexCell, 5)), -outerRadius),
            new Vector3(-innerRadius, flat ? hexCell.height : Math.Max(Math.Max(hexCell.height, getHeight(hexCell, 5)), getHeight(hexCell, 6)), -0.5f * outerRadius),
            new Vector3(-innerRadius, flat ? hexCell.height : Math.Max(Math.Max(hexCell.height, getHeight(hexCell, 6)), getHeight(hexCell, 1)), 0.5f * outerRadius),
            new Vector3(0f, flat ? hexCell.height : Math.Max(Math.Max(hexCell.height, getHeight(hexCell, 1)), getHeight(hexCell, 2)), outerRadius)
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
                float x = flat ? hexCell.height : (getHeight(hexCell, 1) + getHeight(hexCell, 2) + getHeight(hexCell, 3) + getHeight(hexCell, 4) + getHeight(hexCell, 5) + getHeight(hexCell, 6) + hexCell.height) / 7;
                vertices[i] = new Vector3(vertices[i].x, Math.Max(x, hexCell.height), vertices[i].y);
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
        List<Vector3> wallVertices = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        List<int> wallTriangles = new List<int>();
        Mesh wallMesh = new Mesh();
        Vector3[] corners = new Vector3[] //hexagon coordinates from centre + maxheight of point neighbours + self
        {
            new Vector3(0f, Math.Max(Math.Max(cell.height, getHeight(cell, 1)), getHeight(cell, 2)), cell.outerRadius()),
            new Vector3(cell.innerRadius, Math.Max(Math.Max(cell.height, getHeight(cell, 2)), getHeight(cell, 3)), 0.5f * cell.outerRadius()),
            new Vector3(cell.innerRadius, Math.Max(Math.Max(cell.height, getHeight(cell, 3)), getHeight(cell,4)), -0.5f * cell.outerRadius()),
            new Vector3(0f, Math.Max(Math.Max(cell.height, getHeight(cell, 4)), getHeight(cell, 5)), -cell.outerRadius()),
            new Vector3(-cell.innerRadius, Math.Max(Math.Max(cell.height, getHeight(cell, 5)), getHeight(cell, 6)), -0.5f * cell.outerRadius()),
            new Vector3(-cell.innerRadius, Math.Max(Math.Max(cell.height, getHeight(cell, 6)), getHeight(cell, 1)), 0.5f * cell.outerRadius())
        };
        for (int i = 0; i < 6; i++)
        {
            int startIndex = wallVertices.Count;
            wallVertices.Add(corners[i]); // left
            wallVertices.Add(corners[i] + ((cell.height >= corners[i].y) ? -(Vector3.up * cell.height) : (Vector3.up * (cell.height - corners[i].y)))); // bottom left
            uv.Add(corners[i]); // left
            uv.Add(corners[i] + ((cell.height > corners[i].y) ? -(Vector3.up * cell.height) : (Vector3.up * (cell.height - corners[i].y)))); // bottom left
            if (i != 5)
            {
                wallVertices.Add(corners[1 + i]);// right            
                wallVertices.Add(corners[1 + i] + ((cell.height >= corners[1 + i].y) ? -(Vector3.up * cell.height) : (Vector3.up * (cell.height - corners[1 + i].y)))); // bottom right
                uv.Add(corners[1 + i]);
                uv.Add(corners[1 + i] + ((cell.height > corners[1 + i].y) ? -(Vector3.up * cell.height) : (Vector3.up * (cell.height - corners[1 + i].y)))); // bottom right
            }
            else
            {
                wallVertices.Add(corners[0]);// right
                wallVertices.Add(corners[0] + ((cell.height >= corners[0].y) ? -(Vector3.up * cell.height) : (Vector3.up * (cell.height - corners[0].y)))); // bottom right
                uv.Add(corners[0]);// right
                uv.Add(corners[0] + ((cell.height >= corners[0].y) ? -(Vector3.up * cell.height) : (Vector3.up * (cell.height - corners[0].y)))); // bottom right
            }
            if (cell.height >= corners[0].y)
            {
                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 0);

                wallTriangles.Add(startIndex + 1);
                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 0);
            }
            else
            {
                wallTriangles.Add(startIndex + 0);
                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 1);

                wallTriangles.Add(startIndex + 0);
                wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 3);
            }
        }
        wallMesh.vertices = wallVertices.ToArray();
        wallMesh.triangles = wallTriangles.ToArray(); //add gamebject child with mesh
        wallMesh.uv = uv.ToArray();
        wallMesh.RecalculateNormals();

        GameObject x = new GameObject("Wall", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
        x.GetComponent<MeshFilter>().mesh = wallMesh;
        x.GetComponent<MeshCollider>().sharedMesh = wallMesh;
        x.GetComponent<MeshRenderer>().material = cell.obj.GetComponent<MeshRenderer>().material;
        x.GetComponent<Renderer>().material.shader = Shader.Find(shader);
        x.transform.SetParent(cell.obj.transform);
        x.transform.localPosition = Vector3.zero;
        x.tag = cell.type == "F" ? "Wall" : "noClimbWall";

    }
    private static float getHeight(hexCell cell, int c = 0)
    {
        bool xOverMax = cell.x + 1 > map.GetLength(0) - 1, xUnderMin = cell.x - 1 < 0, yOverMax = cell.y + 1 > map.GetLength(1) - 1, yUnderMin = cell.y - 1 < 0; //stops out of range exceptions=
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
    private static UnityEngine.Object assignObject(string obj, UnityEngine.Object[] objs)
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