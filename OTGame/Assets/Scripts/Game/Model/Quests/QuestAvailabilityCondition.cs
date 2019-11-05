using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class QuestAvailabilityCondition : MonoBehaviour
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
