using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterType : MonoBehaviour
{
    public int Id;
    public int Level;
    public int RespawnTime;
    public Stats BaseStats;

    public object ToModel()
    {
        return new {
            Name = name,
            Level,
            RespawnTime,
            BaseStats
        };
    }
}
