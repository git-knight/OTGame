using NotSoSimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerData
{
    public const int healthPerSecond = 4;

    public string Name { get; set; }
    public int Gender { get; set; }
    public int Level { get; set; }
    public int Gold { get; set; }

    public int Health { get { return HealingStartedAt.HasValue ? Math.Min(HealthBeforeHealing.Value + (int)((DateTime.UtcNow - HealingStartedAt.Value).TotalSeconds * healthPerSecond), HealthMax) : HealthMax; } }
    public int HealthMax { get; set; }

    public int? HealthBeforeHealing { get; set; }
    public DateTime? HealingStartedAt { get; set; }

    public int Exp { get; set; }
    public int ExpToNextLevel { get; set; }
}

public class GameWorld : MonoBehaviour
{
    public GameObject BattlePrefab;
    public GameObject UnitQuestListPrefab;
    public GameObject QuestCompletedPrefab;
    public GameObject PlayerQuestListPrefab;
    public GameObject InventoryViewPrefab;
    public GameObject HeroStatsPrefab;
    public GameObject PlayerLevelupPrefab;

    public Map Map { get; private set; }
    public Chat Chat { get; private set; }
    static public Battle Battle { get; private set; }
    static public GameObject ActiveView { get; set; }

    static public PlayerData PlayerData { get; private set; }

    public void OnConnected(JSONObject playerData)
    {
        PlayerData = new PlayerData
        {
            Name = playerData["name"].AsString,
            Gender = playerData["gender"].AsInt.Value,
            Level = playerData["level"].AsInt.Value,
            Gold = playerData["money"].AsInt.Value,
            HealthBeforeHealing = playerData["hp"].AsInt.Value,
            HealthMax = playerData["hpMax"].AsInt.Value,
            Exp = playerData["exp"].AsInt.Value,
            ExpToNextLevel = playerData["expToNextLevel"].AsInt.Value,
        };

        Debug.Log("Connected as " + PlayerData.Name);
    }

    public void ShowUnitQuests(JSONObject quests)
    {
        if (Battle != null)
            return;

        if (ActiveView == null)
        {
            ActiveView = Instantiate(UnitQuestListPrefab, transform.Find("MainUI"));
            ActiveView.GetComponent<QuestViewer>().Initialize(quests["quests"]);

            ActiveView.transform.Find("background/Close").GetComponent<Button>().onClick.AddListener(() =>
            {
                UICommon.FadeOut(ActiveView.gameObject);
                ActiveView = null;
            });
        }
        else ActiveView.GetComponent<QuestViewer>().Initialize(quests["quests"]);
    }

    public void ShowActiveQuests(JSONArray quests)
    {
        if (Battle != null)
            return;

        ActiveQuestsView q;
        if (ActiveView != null)
            if ((q = ActiveView.GetComponent<ActiveQuestsView>()) == null)
            {
                Destroy(ActiveView);
                ActiveView = null;
            }
            else q.Initialize(quests);

        if (ActiveView == null)
        {
            ActiveView = Instantiate(PlayerQuestListPrefab, transform.Find("MainUI"));
            ActiveView.GetComponent<ActiveQuestsView>().Initialize(quests);

            ActiveView.transform.Find("background/Close").GetComponent<Button>().onClick.AddListener(() =>
            {
                UICommon.FadeOut(ActiveView.gameObject);
                ActiveView = null;
            });
        }
    }

    GameObject ActivePopup;
    public MonoBehaviour ActiveViewScript { get; set; }

    public IEnumerator PlayerLeveledUp(JSONObject playerData)
    {
        while (Battle != null || ActivePopup != null)
            yield return null;

        PlayerData = new PlayerData
        {
            Name = PlayerData.Name,
            Gender = PlayerData.Gender,
            Level = playerData["level"].AsInt.Value,
            HealthBeforeHealing = PlayerData.HealthBeforeHealing,
            HealingStartedAt = PlayerData.HealingStartedAt,
            HealthMax = playerData["hpMax"].AsInt.Value,
            Exp = playerData["exp"].AsInt.Value,
            ExpToNextLevel = playerData["expToNextLevel"].AsInt.Value,
        };

        var levelupPopup = ActivePopup = Instantiate(PlayerLevelupPrefab, transform.Find("MainUI"));

        var playerLevelLabel = levelupPopup.transform.Find("MainPanel/3level").gameObject;
        playerLevelLabel.GetComponent<Text>().text = playerLevelLabel.GetComponent<Text>().text.Replace("X", PlayerData.Level + "");

        var btnContinue = levelupPopup.transform.Find("MainPanel/Continue").gameObject;
        btnContinue.GetComponent<Button>().onClick.AddListener(() =>
        {
            Destroy(levelupPopup);
            ActivePopup = null;
        });
    }

    public IEnumerator PlayerOpenedInventory(JSONArray items)
    {
        while (Battle != null || ActivePopup != null)
            yield return null;

        ActivePopup = Instantiate(InventoryViewPrefab, transform.Find("MainUI"));
        var inventory = ActivePopup.GetComponent<InventoryView>();
        inventory.Initialize(items);
        ActiveViewScript = inventory;

        var btnContinue = inventory.transform.Find("background/Close").GetComponent<Button>();
        btnContinue.onClick.AddListener(() =>
        {
            UICommon.FadeOut(inventory.gameObject);
            ActiveView = null;
            ActivePopup = null;
        });
    }

    public IEnumerator ShowHeroStats(JSONNode playerInfo) 
    {
        while (Battle != null || ActivePopup != null)
            yield return null;

        ActivePopup = Instantiate(HeroStatsPrefab, transform.Find("MainUI"));
        ActivePopup.GetComponent<HeroStatsView>().Initialize(playerInfo);

        var btnContinue = ActivePopup.transform.Find("background/Close").GetComponent<Button>();
        btnContinue.onClick.AddListener(() =>
        {
            UICommon.FadeOut(ActivePopup);
            ActiveView = null;
            ActivePopup = null;
        });
    }

    public IEnumerator PlayerRewarded(JSONObject reward)
    {
        while (Battle != null || ActivePopup != null)
            yield return null;

        PlayerData.Exp += reward["exp"].AsInt ?? 0;
        PlayerData.Gold += reward["money"].AsInt ?? 0;

        var questCompleted = ActivePopup = Instantiate(QuestCompletedPrefab, transform.Find("MainUI"));

        var recvText = questCompleted.transform.Find("MainPanel/TopPanel/MainText").GetComponent<Text>();
        recvText.text = "You receive: \n";
        if ((reward["exp"].AsInt ?? 0) > 0)
            recvText.text += "<color=green>+" + reward["exp"].AsInt + "</color> experience\n";
        if ((reward["money"].AsInt ?? 0) > 0)
            recvText.text += "<color=green>+" + reward["money"].AsInt + "</color> gold\n";

        var itemsList = questCompleted.transform.Find("MainPanel/ItemsList");
        var itemPrefab = itemsList.GetChild(0).gameObject;
        foreach(var item in reward["items"].AsArray)
        {
            var itemView = Instantiate(itemPrefab, itemsList, false);

            var itemImage = itemView.transform.Find("ItemImage").GetComponent<RawImage>();
            itemImage.texture = Resources.Load<Texture>("Sprites/Items/" + item["name"].AsString);

            var itemName = itemView.transform.Find("ItemName").GetComponent<Text>();
            itemName.text = item["name"].AsString;

            itemView.SetActive(true);
        }

        var btnContinue = questCompleted.transform.Find("MainPanel/BottomPanel/Continue").GetComponent<Button>();
        btnContinue.onClick.AddListener(() =>
        {
            Destroy(questCompleted);
            ActivePopup = null;
        });
    }

    public void OnShowQuestsClicked()
    {
        GameHub.Invoke("ShowActiveQuests");
    }

    public void OnShowHeroStatsClicked()
    {
        GameHub.Invoke("ShowHeroStats");
    }

    public void OnShowInventoryClicked()
    {
        GameHub.Invoke("OpenInventory");
    }

    //public void ShowHeroInfo(string nick)
    //{
    //    GameHub.Invoke("ShowHeroInfo", nick);
    //}

    public void LoadLevel(JSONObject world)
    {
        if (Map == null)
        {
            // Map = Instantiate(MapPrefab).GetComponent<Map>();
            // Map.gameObject.transform.SetParent(this.transform);
            Map = transform.Find("MapView").GetComponent<Map>();
        }

        Map.LoadLevel(world);

        if (Chat == null)
            Chat = transform.Find("MainUI/Chat").GetComponent<Chat>();
        Chat.gameObject.SetActive(true);
    }

    public bool BattleJoined(JSONNode battleData)
    {
        PlayerData.HealthBeforeHealing = PlayerData.Health;
        PlayerData.HealingStartedAt = null;

        if (Map != null && Map.Players.Values.Any(p => p.IsMoving && ("p-"+battleData["board"]["left"]["name"].AsString == p.name || "p-"+battleData["board"]["right"]["name"].AsString == p.name)))
            return false;

        Battle = Instantiate(BattlePrefab).GetComponent<Battle>();
        Battle.Initialize(battleData);
        return true;
    }

    public IEnumerator BattleFinished(JSONNode battleData, JSONObject world)
    {
        PlayerData.HealthBeforeHealing = battleData["health"].AsInt.Value;
        PlayerData.HealingStartedAt = DateTime.UtcNow;

        while (Battle.Board.IsAnimating)
            yield return null;

        var finishBattlePopup = Instantiate(Battle.BattleFinishedPrefab);

        var youWin = finishBattlePopup.transform.Find("MainPanel/YouWin").gameObject;
        var recExp = finishBattlePopup.transform.Find("MainPanel/10exp").gameObject;
        var recMoney = finishBattlePopup.transform.Find("MainPanel/10money").gameObject;

        if (battleData["exp"].AsInt == -1)
        {
            youWin.GetComponent<Text>().text = "You lose!";
            recExp.GetComponent<Text>().enabled = false;
            recMoney.GetComponent<Text>().enabled = false;
        }
        else
        {
            recExp.GetComponent<Text>().text = recExp.GetComponent<Text>().text.Replace("+10", battleData["exp"].AsInt + "");
            recMoney.GetComponent<Text>().text = recMoney.GetComponent<Text>().text.Replace("+20", battleData["money"].AsInt + "");

            PlayerData.Exp += battleData["exp"].AsInt ?? 0;
            PlayerData.Gold += battleData["money"].AsInt ?? 0;
        }

        while (!Battle.IsExited)
            yield return null;

        Destroy(Battle.gameObject);
        Destroy(finishBattlePopup);
        Battle = null;

        LoadLevel(world);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
