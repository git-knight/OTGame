using System;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json.Linq;
using TGame.Match3;

namespace TGame.Entities
{
    public class Monster : WarriorBase
    {
        private static int _idCounter = 0;

        public int MonsterId { get; set; } = _idCounter++;

        public MonsterClass Type { get; set; }

        [NotMapped] public override int RespawnTime => Type.RespawnTime;

        [NotMapped] public override string Name => Type.Name;
        [NotMapped] public override string BattleId => Name + "-" + MonsterId;
        
        [NotMapped] public override int Level { get => Type.Level; set { Type.Level = value; } }
        [NotMapped] public Stats BaseStats => Type.BaseStats;
        public override int GetBaseStat(StatName stat) => BaseStats[stat];

        public Monster(JToken data)
        {
            Type = GameHub.MonsterClasses[(int)data["TypeId"]];
            Location = new Point(data["Location"]);
        }

        public override void OnBattleJoined(Battle battle)
        {
            HP_Current = BaseStats.Health;
        }

        public override object ToClient()
        {
            return new
            {
                id = MonsterId,
                IsAlive
            };
        }

        public override object ToClient_Battle()
        {
            return new
            {
                id = MonsterId,
                Name,
                Type.Level,
                hp = HP_Current,
                hpMax = Stats.Health,
                Stones
            };
        }

        public override string ToString()
        {
            return (Type.Name + " " + Type.Id) + " " + Location;
        }
    }
}
