using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Monster : Unit
{
    public int Level { get; set; }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal void Init(int id, MonsterType monster)
    {
        Id = id;
        Name = monster.name;
        Level = monster.Level;
        transform.Find("Canvas/name").GetComponent<Text>().text += " [" + Level + "]";
    }
}
