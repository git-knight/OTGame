using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TGame.Match3;

namespace TGame.Entities
{
    public class Map
    {
        private static int _idCounter = 0;

        public int MapId { get; set; } = _idCounter++;
        public string Title { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }
        
        public byte[] PassabilityData { get; set; }

        public MapObject[] MapObjects { get; set; }
        public Monster[] Monsters { get; set; }

        public List<Battle> Battles { get; } = new List<Battle>();
        public string GroupName { get { return "map=" + Title; } }

        public byte this[Point pt] { get { return PassabilityData[pt.Y * Width + pt.X]; } }

        public Map() : this(30, 30) { }

        public Map(int w = 30, int h = 30)
        {
            Width = w;
            Height = h;
        }

        public Map(JToken data)
        {
            Title = (string)data["Name"];
            PassabilityData = ((string)data["Passability"]).Select(x => (byte)(x - '0')).ToArray();
            Monsters = ((JArray)data["Monsters"]).Select(m => new Monster(m)).ToArray();
            MapObjects = ((JArray)data["QuestUnits"]).Select(m => new MapObject(this, m)).ToArray();
        }

       public Monster GetMonsterAt(Point pt)
       {
           return Monsters.FirstOrDefault(b => b.Location == pt);
       }

        internal object ToClient(Hero player)
        {
            return new
            {
                title = Title,
                objects = MapObjects.Select(x => x.ToClient(player)).ToArray(),
                monsters = Monsters.Select(m => m.ToClient()).ToArray(),
                battles = Battles.Select(b => b.ToMap()).ToArray()
            };
        }

        internal bool TryGetUnit(PointHex newLoc, out MapObject unit)
        {
            unit = MapObjects.FirstOrDefault(u => u.Location == newLoc.ToCoord());
            return unit != null;
        }
    }
}
