using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using TGame.Match3;

namespace TGame.Entities
{
    public class Hero : WarriorBase
    {
        public int Id { get; set; }
        [NotMapped] public override string Name => Owner.UserName;
        [NotMapped] public override string BattleId => "p-" + Name;

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public User Owner { get; set; }

        public int Gender { get; set; }
        public override int Level { get; set; }
        public int Money { get; set; }
        public int Exp { get; set; }

        [NotMapped] public override int RespawnTime => Stats.Health / 4;

        public Stats BaseStats { get; set; }
        public override int GetBaseStat(StatName stat) => BaseStats[stat];

        public virtual IEnumerable<ItemStack> Inventory { get; set; }
        public virtual IEnumerable<QuestCompletion> Quests { get; set; }
        public virtual IEnumerable<PlayerStatistics> Statistics { get; set; }


        public int TotalWins { get; set; }
        public int TotalPlays { get; set; }
        

        const int expStep = 100;
        [NotMapped] public int ExpToNextLevel => (2 * expStep + (Level - 1) * expStep) * Level / 2;
        
        [NotMapped] public Battle CurrentBattle { get; set; }
        [NotMapped] public HashSet<string> Connections { get; } = new HashSet<string>();

        public Hero() { }

        public override int CalculateStatIncrease(StatName stat) 
            => Inventory.Select(x => !x.IsEquipped ? 0 : (x.Type as Equipment)?.Stats[stat] ?? 0).Sum();


        public int GetKillCounter(int monsterType) 
            => Statistics.FirstOrDefault(s => s.StatType == StatisticsType.MonsterKillCounter && s.ClassId == monsterType)?.Counter ?? 0;

        public int GetItemCounter(int itemType) 
            => Inventory.FirstOrDefault(s => s.TypeId == itemType)?.Amount ?? 0;



        public bool TryMoveTo(int direction)
        {
            PointHex[] directions =
            {
                new PointHex(-1, 1, 0), new PointHex(-1, 0, 1), new PointHex(0, -1, 1),
                new PointHex(1, -1, 0), new PointHex(1, 0, -1), new PointHex(0, 1, -1)
            };

            var newLoc = LocationHex + directions[direction];
            if (Map[newLoc.ToCoord()] == 0) 
                return false;

            LocationHex = newLoc;
            return true;
        }

        public override void OnBattleJoined(Battle battle)
        {
            CurrentBattle = battle;
        }
        public override void OnBattleFinished()
        {
            TotalPlays++;
            CurrentBattle = null;
        }

        public override object ToClient()
        {
            return new
            {
                Name,
                Gender,
                Level,
                Location,
                //hp = HP_Current,
                //hpMax = HP_Full
            };
        }

        public object ToOwner()
        {
            return new
            {
                Name,
                Level,
                Gender,
                Location,
                hp = HP_Current,
                hpMax = Stats.Health,
                Exp,
                ExpToNextLevel,
                Money
            };
        }

        public override object ToClient_Battle()
        {
            return new
            {
                Name,
                Gender,
                Level,
                hp = HP_Current,
                hpMax = Stats.Health,
                Stones
            };
        }

        internal object ToInitialInfo()
        {
            return new
            {
                Name,
                Gender,
                Level,
                Money,
                hp = HP_Current,
                hpMax = Stats.Health,
                Exp,
                ExpToNextLevel
            };
        }

    }
}
