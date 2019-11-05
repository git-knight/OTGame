using System.Collections.Generic;
using System.Linq;
using TGame.Entities;

namespace TGame.Match3
{
    public class Battle
    {
        static private int _id = 0;
        public int Id { get; } = _id++;
        public Point Location { get; set; }
        public Board Board { get; set; }

        public List<WarriorBase>[] Teams { get; set; }

        public IEnumerable<WarriorBase> Players { get{ return Teams[0].Concat(Teams[1]); } }
        public string[] PlayerNames { get { return Players.OfType<Hero>().Select(p => p.Name).ToArray(); } }

        public string GroupName { get; }

        public Battle(WarriorBase inv, WarriorBase[] opps)
        {
            Teams = new List<WarriorBase>[] { new WarriorBase[] { inv }.ToList(), opps.Take(1).ToList() };
            Location = inv.Location;
            Board = new Board(this, inv, opps[0]);
            GroupName = "board=" + Id;

            inv.OnBattleJoined(this);

            opps[0].OnBattleJoined(this);
            //foreach (var opp in opps)
            //  opp.OnBattleJoined(this);
        }

        public bool Contains(Hero player)
        {
            return Players.Any(p => (p as Hero)?.Id == player.Id);
            //return (Board.PlayerLeft as Player)?.Id == player.Id || (Board.PlayerRight as Player)?.Id == player.Id;
        }

        public object ToMap()
        {
            return new
            {
                location = Location,
                playersList = PlayerNames
            };
        }

        internal object ToClient(string player)
        {
            //var playersList = Players.Select(p => p.ToClient_Battle()).ToArray();

            return new {
                //playersList,
                board = Board.ToClient()
            };
        }
    }
}
