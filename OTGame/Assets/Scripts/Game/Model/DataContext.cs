#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NotSoSimpleJSON;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class DataContext : MonoBehaviour
{
    public GameObject ClientData;

    private MonsterType[] monsterTypes;

    IEnumerable<T> ChildrenOf<T>(Transform obj)
    {
        foreach (Transform child in obj)
        {
            if (child.TryGetComponent<T>(out var component))
                yield return component;
        }
    }

    IEnumerable<T> ChildrenOf<T>(string path)
    {
        foreach (Transform child in transform.Find(path))
        {
            if (child.TryGetComponent<T>(out var component))
                yield return component;
        }
    }

    IEnumerable<GameObject> ChildrenOf(string path)
    {
        foreach (Transform child in transform.Find(path))
            yield return child.gameObject;
    }

    int monsterId;

    public void Export()
    {
        monsterTypes = Resources.LoadAll<GameObject>("Editor/MonsterTypes/")
            .Select(x => (x as GameObject).GetComponent<MonsterType>())
            .ToArray();

        int monsterCounter = 0;
        foreach (var type in monsterTypes)
            type.Id = monsterCounter++;

        int itemCounter = 0;
        foreach (var item in ChildrenOf<ItemType>("ItemTypes"))
            item.Id = itemCounter++;


        monsterId = 0;

        File.WriteAllText("../OTGameServer/OTGameServer/Resources/Json.txt", JSON.FromData(new
        {
            MonsterClasses = monsterTypes.Select(m => m.ToModel()),
            ItemTypes = ChildrenOf<ItemType>("ItemTypes").Select(m => m.ToModel()),
            Maps = ChildrenOf("Maps").Select(m => ToModel(m))
        }).Serialize());
    }
        
    public object ToModel(GameObject map)
    {
        var clientMap = new GameObject(map.name);

        var clientMapModel = clientMap.AddComponent<MapModel>();
        clientMapModel.Init(map.GetComponent<MapModel>());

        if (map.TryGetComponent<SpriteRenderer>(out var origMapSprite))
        {
            var mapSprite = clientMap.AddComponent<SpriteRenderer>();
            mapSprite.sprite = origMapSprite.sprite;
        }

        var grid = new GameObject("grid");
        grid.transform.SetParent(clientMap.transform);
        var gridSprite = grid.gameObject.AddComponent<SpriteRenderer>();
        gridSprite.sprite = Resources.Load<Sprite>("Sprites/grid");

        var mapSpr = map.GetComponent<SpriteRenderer>().sprite;
        var mapSize = new Vector2(mapSpr.texture.width, mapSpr.texture.height) / mapSpr.pixelsPerUnit;

        gridSprite.drawMode = SpriteDrawMode.Tiled;
        gridSprite.transform.position = Vector3.zero;
        gridSprite.size = mapSize;
        gridSprite.sortingOrder = 1;

        int monsterCounter = 0;
        foreach (var monster in ChildrenOf<MonsterType>(map.transform))
        {
            var monsterObj = Instantiate(monster, clientMap.transform);
            monsterObj.name = "m-" + (monsterCounter++);

            var monsterData = monsterObj.gameObject.AddComponent<Monster>();
            monsterData.Init(monsterId++, monster);

            monsterData.CoordHex = PointHex.FromScreenCoord(monster.transform.position);
            monsterObj.transform.position = monsterData.CoordHex.ToScreenPoint();

            DestroyImmediate(monsterObj.GetComponent<MonsterType>());
        }

        foreach (var questUnit in ChildrenOf<QuestUnit>(map.transform))
        {
            var unitObj = Instantiate(questUnit.transform.Find("View"), clientMap.transform);
            unitObj.name = "u-" + questUnit.name;

            var unit = unitObj.gameObject.AddComponent<Unit>();
            unit.CoordHex = PointHex.FromScreenCoord(questUnit.transform.position);
            unitObj.transform.position = unit.CoordHex.ToScreenPoint();
        }
        
        PrefabUtility.SaveAsPrefabAsset(clientMap, "Assets/Resources/Maps/" + map.name + ".prefab");
        DestroyImmediate(clientMap);

        return new
        {
            Name = map.name,
            Passability = new string(map.GetComponent<MapModel>().Passability.Select(x => (char)('0' + x)).ToArray()),
            Monsters = ChildrenOf<MonsterType>(map.transform).Select(m => new
            {
                TypeId = m.Id,
                Location = PointHex.FromScreenCoord(m.transform.position).ToPoint()
            }),
            QuestUnits = ChildrenOf<QuestUnit>(map.transform).Select(u => new
            {
                Name = u.name,
                Location = PointHex.FromScreenCoord(u.transform.position).ToPoint(),
                Quests = ChildrenOf<Quest>(u.transform).Select(q => new {
                    Title = q.name,
                    q.Description,
                    q.Dialogues,
                    q.RewardExp,
                    q.RewardGold,

                    Conditions = ChildrenOf<QuestAvailabilityCondition>(q.transform).Select(c => c.ToModel()).ToArray(),
                    Tasks = ChildrenOf<QuestTask>(q.transform).Select(t => t.ToModel()).ToArray(),
                    RewardItems = q.RewardItems.Select(r => new {
                        ItemType = r.Item.Id,
                        r.Amount
                    })
                })
            })
        };
    }

    void Start()
    {
        gameObject.SetActive(false);
    }
}
#endif