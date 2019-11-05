using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TGame.Entities
{
    public enum StatisticsType
    {
        None = -1,
        MonsterKillCounter,
    }

    public class PlayerStatistics
    {
        [Key, Column(Order = 0)]
        public int HeroId { get; set; }
        [Key, Column(Order = 1)]
        public StatisticsType StatType { get; set; }
        [Key, Column(Order = 2)]
        public int ClassId { get; set; }

        public int Counter { get; set; }

    }
}