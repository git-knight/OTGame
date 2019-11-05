using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    GameObject Username;
    GameObject Level;
    Text GoldAmount;
    StatusBar Health;
    StatusBar Exp;

    // Start is called before the first frame update
    void Start()
    {
        Username = transform.Find("UserInfo/Username").gameObject;
        Level = transform.Find("UserInfo/Level").gameObject;
        GoldAmount = transform.Find("GoldAmountText").GetComponent<Text>();
        Health = transform.Find("UserInfo/Health").gameObject.GetComponent<StatusBar>();
        Exp = transform.Find("UserInfo/Exp").gameObject.GetComponent<StatusBar>();

        Username.GetComponent<Text>().text = GameWorld.PlayerData.Name;

        Health.Initialize(GameWorld.PlayerData.Health, GameWorld.PlayerData.HealthMax);
        Exp.Initialize(GameWorld.PlayerData.Exp, GameWorld.PlayerData.ExpToNextLevel);
    }

    void Update()
    {
        GoldAmount.text = GameWorld.PlayerData.Gold + " gold";

        Health.Max = GameWorld.PlayerData.HealthMax;
        Health.Current = GameWorld.PlayerData.Health;
        Exp.Max = GameWorld.PlayerData.ExpToNextLevel;
        Exp.Current = GameWorld.PlayerData.Exp;

        Level.GetComponent<Text>().text = "[" + GameWorld.PlayerData.Level + "]";
    }
}
