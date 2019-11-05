using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterKillingQuest : QuestTask
{
    public MonsterType Monster;

    public override object ToModel()
    {
        return new
        {
            Type = GetType().Name,
            MonsterType = Monster.Id,
            RequiredAmount
        };
    }
}
