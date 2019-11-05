using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NotSoSimpleJSON;
using UnityEngine;

public class Map : MonoBehaviour
{
    public GameObject UnitPrefab;

    private PlayerController playerController;
    private GameObject mapSprite;

    public Dictionary<string, Unit> Players { get; set; }

    void Start()
    {
    }

    void UnloadLevel()
    {
        if (mapSprite == null)
            return;

        Destroy(mapSprite);
        Players = null;
    }

    public void LoadLevel(JSONObject world) 
    {
        UnloadLevel();
        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        JSONObject map = world["map"].AsObject;
        string name = map["title"].AsString;

        mapSprite = Instantiate(Resources.Load<GameObject>("Maps/" + name), transform);
        mapSprite.SetActive(true);

        foreach(var monster in map["monsters"].AsArray)
        {
            var id = monster["id"].AsInt;
            if (monster["isAlive"].AsBool == false)
                mapSprite.transform.Find("m-" + id).gameObject.SetActive(false);
        }


        Players = world["players"].AsArray.Select(u =>
        {
            var unitObj = Instantiate(UnitPrefab, mapSprite.transform, false);

            var unit = unitObj.AddComponent<Unit>();
            unit.Key = "p-" + u["name"].AsString;
            unit.CoordHex = new PointHex(u["location"]);
            unit.ServerCoord = unit.Coord;
            unitObj.name = unit.Name = unit.Key;

            return unit;
        }).ToDictionary(u => u.Key);
        //  mapSprite = Instantiate(transform.Find("MapSprite").gameObject, transform);
        //mapSprite.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Maps/" + name);
        /*
        var edges = map["edges"].AsArray.Select(e => {
            var edgeObj = new GameObject("edge", typeof(EdgeCollider2D), typeof(Rigidbody2D));
            edgeObj.transform.SetParent(transform);

            var rb = edgeObj.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;

            var edge = edgeObj.GetComponent<EdgeCollider2D>();
            edge.transform.position = e["position"].ToVecF();
            edge.points = e["points"].AsArray.Select(p=>p.ToVecF()).ToArray();

            return edgeObj;
        }).ToArray();
        units = world["players"].AsArray.Concat(map["monsters"].AsArray)
            .Select(u => {
                var unitObj = Instantiate(UnitPrefab, transform, false);
                unitObj.transform.position = new Vector3((float)u["location"]["x"].AsDouble.Value, (float)u["location"]["y"].AsDouble.Value, 0); // .AsInt.Value/64f*40-20
                
                Unit unit;
                if (u["id"].IsEmpty) {
                    unit = unitObj.AddComponent<Unit>();
                    unit.Key = "p-" + u["name"].AsString;
                }
                else 
                {
                    unit = unitObj.AddComponent<Monster>();
                    unit.Key = "m-" + u["id"].AsInt;
                }
                unitObj.name = unit.Name = unit.Key;

                return unit;
            }).ToDictionary(u => u.Key);

        */
        playerController.Player = Players["p-" + GameWorld.PlayerData.Name];

        // GameHub.Invoke("UpdatePlayerCoord", 
        //     playerController.Player.transform.position.x, 
        //     playerController.Player.transform.position.y);
    }

    // public void ServerTick(JSONArray players)
    // {
    //     var myCoord = playerController.Player.transform.position;

    //     foreach(var plData in players)
    //     {
    //         Unit player;
    //         if (!units.TryGetValue("p-" + plData["name"].AsString, out player))
    //             return;

    //         if (player == playerController.Player)
    //             continue;
                
    //         player.transform.position = plData["location"].ToVecF();
    //     }

    //     GameHub.Invoke("UpdatePlayerCoord", 
    //         myCoord.x, 
    //         myCoord.y);
    // }
}
