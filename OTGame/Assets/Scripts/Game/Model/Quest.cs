using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RewardStack
{
    public ItemType Item;
    public int Amount;
}

public class Quest : MonoBehaviour
{
    public string Description;
    public string Dialogues;
    public int RewardExp;
    public int RewardGold;

    public RewardStack[] RewardItems;

    void Start()
    {
        
    }
}
