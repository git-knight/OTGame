using System;
using System.Collections;
using System.Collections.Generic;
using NotSoSimpleJSON;
using UnityEngine;

public class Battle : MonoBehaviour
{
    public GameObject BattleFinishedPrefab;

    public Board Board { get; private set; }

    static public bool IsExited { get; private set; }

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal void Initialize(JSONNode battleData)
    {
        IsExited = false;
        Board = FindObjectOfType<Board>();
        Board.Initialize(battleData["board"]);
    }

    public void ExitBattle()
    {
        IsExited = true;
    }
}
