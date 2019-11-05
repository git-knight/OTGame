using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class QuestTask : MonoBehaviour
{
    public int RequiredAmount;

    public virtual object ToModel()
    {
        return new
        {
            Type = GetType().Name,
            RequiredAmount
        };
    }
}
