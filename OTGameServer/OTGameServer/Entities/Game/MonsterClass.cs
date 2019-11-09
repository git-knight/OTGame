using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;

namespace TGame.Entities
{
    public class MonsterClass
    {
        private static int _idCounter = 0;

        public int Id { get; set; } = _idCounter++;

        public string Name { get; set; }   

        public int Level { get; set; }
        public int RespawnTime { get; set; }

        public Stats BaseStats { get; set; }

        public MonsterClass(JToken data)
        {
            Name = (string)data["Name"];
            Level = (int)data["Level"];
            RespawnTime = (int)data["RespawnTime"];
            BaseStats = data["BaseStats"].ToObject<Stats>();
        }
    }
}