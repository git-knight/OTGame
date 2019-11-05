#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapModel))]
public class MapGen : Editor
{
    private MapModel map;

    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();

        if (GUILayout.Button("Update View"))
        {
            if (testObj == null)
                testObj = new GameObject("hidden");

            map = target as MapModel;

            var grid = map.transform.Find("grid");
            SpriteRenderer spr;
            if (grid == null)
            {
                grid = new GameObject("grid").transform;
                grid.transform.SetParent(map.transform);

                spr = grid.gameObject.AddComponent<SpriteRenderer>();
                spr.sprite = Resources.Load<Sprite>("Sprites/grid");
            }
            else spr = grid.GetComponent<SpriteRenderer>();

            var mapSpr = map.GetComponent<SpriteRenderer>().sprite;
            var mapSize = new Vector2(mapSpr.texture.width, mapSpr.texture.height) / mapSpr.pixelsPerUnit;

            spr.drawMode = SpriteDrawMode.Tiled;
            spr.transform.position = Vector3.zero;
            spr.size = mapSize;//new Vector2(mapTopLeft.x * -2, mapTopLeft.y * 2);
            spr.sortingOrder = 1;


            map.Passability = new byte[map.Width * map.Height];
            for (int i = 0; i < map.Width; i++)
                for (int j = 0; j < map.Height; j++)
                {
                    var cell = map.transform.Find("grid/cell_" + i + "_" + j) ?? new GameObject("cell_" + i + "_" + j).transform;
                    cell.SetParent(grid);
                    cell.transform.position = ((PointHex)new Vector2Int(i, j)).ToScreenPointCentered();

                    map.Passability[j * map.Width + i] = (byte)(collides(cell.transform.position) ? 0 : 1);
                    var cellSpr = GetComponent<SpriteRenderer>(cell) ?? cell.gameObject.AddComponent<SpriteRenderer>();
                    cellSpr.sprite = Resources.Load<Sprite>("Sprites/map_cursor" + (map.Passability[j * map.Width + i] == 0 ? "_red" : ""));
                    cellSpr.sortingOrder = 2;
                    cellSpr.color = new Color(1, 1, 1, 0.3f);
                    
                    
                }

            DestroyImmediate(testObj);
            testObj = null;
        }

        // if (GUILayout.Button("Fix Edges"))
        // {
        //     map = target as MapModel;

        //     foreach(var c in ChildrenOf<EdgeCollider2D>(map.transform))
        //     {
        //         var newPoints = new Vector2[c.pointCount];
        //         for(int i=0;i<c.pointCount-1;i++)
        //         {
        //             var v0 = c.points[i];
        //             var v1 = c.points[(i+1)%(c.pointCount-1)]-v0;
        //             var v2 = c.points[(i-1+c.pointCount-1)%(c.pointCount-1)]-v0;
        //             var vm = (v1.normalized + v2.normalized).normalized;
        //             newPoints[i] = c.points[i]+vm*0.034f;
        //         }

        //         newPoints[c.pointCount-1] = newPoints[0];

        //         c.points = newPoints;
        //     }
        // }
    }

    private T GetComponent<T>(Transform obj)
    {
        obj.TryGetComponent<T>(out var res);
        return res;
    }

    static GameObject testObj;

    private bool collides(Vector3 coord)
    {
        var cellCollider = GetComponent<CircleCollider2D>(testObj.transform) ?? testObj.gameObject.AddComponent<CircleCollider2D>();
        cellCollider.offset = coord;
        // cellCollider.offset = ((PointHex)new Vector2Int(i, j)).ToScreenPoint();
        cellCollider.radius = 29.5f;
        foreach(var edgeCollider in ChildrenOf<EdgeCollider2D>(map.transform))
        {
            var coll = edgeCollider.Distance(cellCollider);
            //if (Physics2D.IsTouching(edgeCollider, cellCollider))
            if (coll.isOverlapped)
                // DestroyImmediate(cellCollider);
                return true;
        }

        // DestroyImmediate(cellCollider);
        return false;
    }



    IEnumerable<T> ChildrenOf<T>(Transform obj)
    {
        foreach (Transform child in obj)
        {
            if (child.TryGetComponent<T>(out var component))
                yield return component;
        }
    }
}
#endif