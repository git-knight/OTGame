using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EquipmentSlot
{
    None = 0,
    Weapon
}

public class Equipment : ItemType
{
    public EquipmentSlot Slot;
    public Stats Stats;

    public override bool IsStackable => false;

    public override object ToModel()
    {
        return new
        {
            Type = nameof(Equipment),
            Name = name,
            Price,
            Slot,
            Stats
        };
    }
}
