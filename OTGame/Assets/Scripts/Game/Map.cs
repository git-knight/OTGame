using System;
using System.Collections.Generic;
using System.Linq;
using NotSoSimpleJSON;
using UnityEngine;

public class Map : MonoBehaviour
{
    public GameObject UnitPrefab;

    private PlayerController playerController;
    private GameObject mapSprite;
    private SpriteRenderer mapCursor;

    public Dictionary<string, Unit> Players { get; set; }
    
    private MapModel MapModel => mapSprite.GetComponent<MapModel>();
    private IEnumerable<Monster> Monsters => Magic.ChildrenOf<Monster>(mapSprite.transform);
    private IEnumerable<Unit> QuestUnits => GetQuestUnits();

    void Start()
    {
        mapCursor = transform.Find("mapCursor").GetComponent<SpriteRenderer>();
    }

    public void UpdateMapCursor(Sprite sprite, PointHex coord)
    {
        mapCursor.sprite = sprite;
        mapCursor.transform.position = coord.ToScreenPointCentered();
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

        playerController.Player = Players["p-" + GameWorld.PlayerData.Name];
        playerController.Player.GetComponent<SpriteRenderer>().sortingOrder = 7;
    }

    public void MonsterRespawned(int id)
    {
        foreach(var m in Monsters)
            m.gameObject.SetActive(true);
    }


    public bool IsBlocked(PointHex pt, bool checkMonsters = false) => IsBlocked(pt.ToPoint(), checkMonsters);

    public bool IsBlocked(Vector2Int pt, bool checkMonsters = false)
    {
        return pt.x < 0 || pt.y < 0 || pt.x >= MapModel.Width || pt.y >= MapModel.Height || MapModel.Passability[pt.y * MapModel.Width + pt.x] == 0 || checkMonsters && (Monsters.Any(m => m.Coord == pt) || Players.Values.Any(p => p.Coord == pt) || QuestUnits.Any(q => q.Coord == pt));
    }


    public Monster TryGetMonster(PointHex pt)
    {
        return Monsters.FirstOrDefault(m => m.CoordHex == pt);
    }

    private IEnumerable<Unit> GetQuestUnits()
    {
        foreach(var el in Magic.ChildrenOf<Unit>(mapSprite.transform))
            if (el.name.StartsWith("u-"))
                yield return el;
    }
}
