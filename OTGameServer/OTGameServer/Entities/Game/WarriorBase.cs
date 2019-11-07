using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using TGame.Match3;

namespace TGame.Entities
{
    public abstract class WarriorBase
    {
        const int healthPerSecond = 4;

        public abstract string Name { get; }
        public abstract string BattleId { get; }

        public abstract int GetBaseStat(StatName stat);

        public virtual int Level { get; set; }
        [NotMapped] public ActualStats Stats { get; }

        public abstract int RespawnTime { get; }

        [NotMapped] public int? HealingStartedWith { get; set; }
        [NotMapped] public DateTime? HealingStartedAt { get; set; }
        [NotMapped] public int HP_Current { get { return HealingStartedAt.HasValue ? Math.Min(HealingStartedWith.Value + (int)((DateTime.UtcNow - HealingStartedAt.Value).TotalSeconds * healthPerSecond), Stats.Health) : HealingStartedWith ?? Stats.Health; } set { HealingStartedWith = value; } }
        [NotMapped] public bool IsAlive => HealingStartedAt.HasValue ? (DateTime.UtcNow > HealingStartedAt + TimeSpan.FromSeconds(RespawnTime) || HealingStartedWith > 0) : true;

        [NotMapped] public int[] Stones { get; set; }

        public int MapId { get; set; }
        public Point Location { get; set; }

        [NotMapped] 
        public PointHex LocationHex { get => Location.ToHex(); set { Location = value.ToCoord(); } }

        [NotMapped] public Map Map { get { return GameHub.Maps[MapId]; } set { MapId = value.MapId; } }

        public WarriorBase()
        {
            Stats = new ActualStats(this);
        }

        //internal bool Hits(WarriorBase b) => Vector2.Distance(Location, b.Location) <= 0.2f;

        public virtual int CalculateStatIncrease(StatName stat) => 0;

        public void StartHealthRegen()
        {
            HealingStartedWith = Math.Max(0, HP_Current);
            HealingStartedAt = DateTime.UtcNow;
        }
        public void StopHealthRegen()
        {
            HealingStartedWith = HP_Current;
            HealingStartedAt = null;
        }

        public virtual void OnBattleJoined(Battle battle) { }
        public virtual void OnBattleFinished() { }

        public abstract object ToClient();
        public abstract object ToClient_Battle();
    }
}
