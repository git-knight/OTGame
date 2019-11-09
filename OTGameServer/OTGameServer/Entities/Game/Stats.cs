using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TGame.Entities
{
    public enum StatName
    {
        Health=0,
        Attack,
        Defense,
        MinDamage,
        MaxDamage,

        Count
    }

    [Owned]
    public class Stats
    {
        private int[] AllStats = new int[(int)StatName.Count];

        public int Health { get => this[StatName.Health]; set { this[StatName.Health] = value; } }
        public int Attack { get => this[StatName.Attack]; set { this[StatName.Attack] = value; } }
        public int Defense { get => this[StatName.Defense]; set { this[StatName.Defense] = value; } }
        public int MinDamage { get => this[StatName.MinDamage]; set { this[StatName.MinDamage] = value; } }
        public int MaxDamage { get => this[StatName.MaxDamage]; set { this[StatName.MaxDamage] = value; } }

        public virtual int this[StatName stat] { get => AllStats[(int)stat]; private set { AllStats[(int)stat] = value; } }

        static public Stats operator+(Stats a, Stats b)
        {
            return new Stats{
                AllStats = a.AllStats.Zip(b.AllStats, (x, y) => x + y).ToArray()
            };
        }
    }

    public class ActualStats : Stats
    {
        private WarriorBase owner;

        private int?[] cachedStats = new int?[(int)StatName.Count];

        public override int this[StatName stat] 
            => (cachedStats[(int)stat] = cachedStats[(int)stat] ?? owner.GetBaseStat(stat) + owner.CalculateStatIncrease(stat)).Value;

        public ActualStats(WarriorBase owner)
        {
            this.owner = owner;
        }

        public void Invalidate()
        {
            cachedStats = new int?[(int)StatName.Count];
        }
    }
}
