using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TGame.Entities;

namespace TGame
{
    public partial class GameHub
    {
        static readonly Timer monsterTicker = new Timer(async (o) => await TickMonsters(), null, 5000, 1500);

        static private async Task TickMonsters()
        {
            if (hubContext == null || Monsters == null)
                return;

            var respawnedMonsters = Monsters.Where(m => m.IsAlive && m.HealingStartedAt.HasValue);
            foreach (var monster in respawnedMonsters)
            {
                monster.HealingStartedWith = monster.HP_Current;
                monster.HealingStartedAt = null;

                var playersInThatCell = playersOnline.Values.Where(p => p.Location == monster.Location && p.MapId == monster.MapId).ToArray();
                if (playersInThatCell.Length > 0)
                    await StartBattle(playersInThatCell, monster);

                await hubContext.Clients.Group(monster.Map.GroupName).SendAsync("InvokeMethod", "Map.MonsterRespawned", monster.MonsterId);
            }
        }

        public async Task RejoinWorld()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, CurrentPlayer.Map.GroupName);
            await Clients.Caller.SendAsync("InvokeMethod", "LoadLevel", GetInitialDataForPlayer(CurrentPlayer));
        }

        public async Task DoPlayerMove(int direction)
        {
            if (CurrentBattle != null)
                return;

            gameContext.Attach(CurrentPlayer);
            if (!CurrentPlayer.TryMoveTo(direction))
                return;

            await gameContext.SaveChangesAsync();

            WarriorBase[] hitOpp = ((IEnumerable<WarriorBase>)CurrentPlayer.Map.Monsters.Where(b => b.IsAlive && b.Location == CurrentPlayer.Location)).Concat(playersOnline.Values.Where(x => x.Id != CurrentPlayer.Id && x.MapId == CurrentPlayer.MapId && x.Location == CurrentPlayer.Location)).ToArray();
            if (hitOpp.Length > 0)
                await StartBattle(hitOpp, CurrentPlayer);
            else await Clients.Group(CurrentPlayer.Map.GroupName).SendAsync("InvokeMethod", "Map.Players#p-" + CurrentPlayer.Name + ".Moved", CurrentPlayer.Location);
        }

        static internal object GetInitialDataForPlayer(Hero player)
        {
            return new
            {
                map = player.Map.ToClient(player),
                players = playersOnline.Where(p => p.Value.MapId == player.MapId).Select(p => p.Value.ToClient()).ToArray()
            };
        }

        public async Task ShowUnitQuests(int unitId)
        {
            if (unitId < 0 || unitId >= MapObjects.Count())
                return;
                
            var unit = MapObjects[unitId];

            var unitQuests = unit.Quests.Where(q => q.GetStatusFor(CurrentPlayer) != QuestStatus.None);

            await Clients.Caller.SendAsync("InvokeMethod", "ShowUnitQuests", new
            {
                unitId,
                quests = Quests.Select(q => q.ToPlayer(CurrentPlayer)).ToArray()
            });
        }
    }
}