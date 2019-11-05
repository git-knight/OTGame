using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TGame.Entities
{
    public class MapObject
    {
        private static int _idCounter = 0;

        public int Id { get; set; } = _idCounter++;
        public string Name { get; set; }

        public Map Map { get; set; }
        public Point Location { get; set; }

        public Quest[] Quests { get; set; }

        public MapObject(Map map, JToken data)
        {
            Map = map;
            Name = (string)data["Name"];
            Location = new Point(data["Location"]);
            Quests = ((JArray)data["Quests"]).Select(m => new Quest(this, m)).ToArray();
        }

        internal object ToClient(Hero player)
        {
            return new
            {
                id = Id,
                questStatus = Quests.Max(q => q.GetStatusFor(player))
            };
        }
    }
}
