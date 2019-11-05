using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using TGame.Entities;
using TGame.Match3;

namespace TGame
{
    public static class ProxyExtension
    {
        public static Task Invoke(this IClientProxy proxy, string MethodPath, object[] args)
        {
            return proxy.SendCoreAsync("InvokeMethod", new object[] { MethodPath }.Concat(args).ToArray());
        }
    }

    public interface IGameClient
    {
        Task OnConnected(string myName);
        Task LoadLevel(object map);
        Task PlayerJoined(object player);
        Task PlayerLeft(string name);
        Task PlayerMoved(string name, Point location);


        Task BattleStarted(Point location, string[] playerNames);
        Task BattleJoined(Point location, object battle);
        Task BattleFinished(object battleResults, object world);
        Task BoardRegenerated(object board);
        Task BattleTurnFinished(Point cell, bool isVertical, object result);

        Task SendChatMessage(object msg);
    }

    [Authorize(Policy = "user")]
    public partial class GameHub : Hub//<IGameClient>
    {
        static public Map[] Maps { get; private set; }
        static public Item[] Items { get; private set; }
        static public MonsterClass[] MonsterClasses { get; private set; }

        static public Monster[] Monsters { get; private set; }
        static public MapObject[] MapObjects { get; private set; }
        static public Quest[] Quests { get; private set; }
        static public QuestTask[] QuestTasks { get; private set; }

        static public Random rand = new Random();

        static public Dictionary<string, Hero> playersOnline = new Dictionary<string, Hero>();
        static public List<Battle> battles = new List<Battle>();

        private readonly GameContext gameContext;
        static private IHubContext<GameHub> hubContext;

        Hero CurrentPlayer
        {
            get { return Context.Items["player"] as Hero; }
            set
            {
                Context.Items["player"] = value;
                value.Connections.Add(Context.ConnectionId);
                Groups.AddToGroupAsync(Context.ConnectionId, value.Name).Wait();
            }
        }
        //string CurrentGroup { get { return CurrentPlayer.CurrentGroup; } set { CurrentPlayer.CurrentGroup = value; } }
        Battle CurrentBattle { get { return CurrentPlayer.CurrentBattle; } set { CurrentPlayer.CurrentBattle = value; } }

        static private void LoadData(string data)
        {
            dynamic allData = JObject.Parse(data);
            
            MonsterClasses = (allData.MonsterClasses as JArray).Select(i => new MonsterClass(i)).ToArray();
            Items = (allData.ItemTypes as JArray).Select(i => new Equipment(i)).ToArray();
            Maps = (allData.Maps as JArray).Select(m => new Map(m)).ToArray();
            Monsters = Maps.SelectMany(m => m.Monsters).ToArray();
            MapObjects = Maps.SelectMany(m => m.MapObjects).ToArray();
            Quests = MapObjects.SelectMany(o => o.Quests).ToArray();
            QuestTasks = Quests.SelectMany(q => q.Tasks).ToArray();
        }

        public GameHub(GameContext gameContext, IHubContext<GameHub> _hubContext)
        {
            this.gameContext = gameContext;
            hubContext = _hubContext;

            if (Maps == null)
                LoadData(File.ReadAllText("Resources/Json.txt"));
        }


        public override async Task OnConnectedAsync()
        {
            if (playersOnline.ContainsKey(Context.UserIdentifier))
            {
                CurrentPlayer = playersOnline[Context.UserIdentifier];
                await Clients.Caller.SendAsync("OnConnected", CurrentPlayer.ToInitialInfo());

                if (CurrentBattle == null)
                    await RejoinWorld();
                else
                    await RejoinBattle();

                await base.OnConnectedAsync();
                return;
            }

            Hero player = gameContext.PlayerHeroes
                .Include(u => u.Inventory)
                .Include(u => u.Owner)
                .Include(u => u.Quests)
                    .ThenInclude(q => q.TaskCompletion)
                .Include(u => u.Statistics)
                .First(u => u.UserId == Context.UserIdentifier);

            await Clients.Caller.SendAsync("OnConnected", player.ToInitialInfo());
            //await Clients.Caller.OnConnected(player.Name);
            playersOnline.Add(Context.UserIdentifier, player);
            CurrentPlayer = player;

            CurrentBattle = battles.Find(b => b.Contains(player));
            if (CurrentBattle != null)
            {
                await RejoinBattle();
                await base.OnConnectedAsync();
                return;
            }

            //await Clients.Group(CurrentPlayer.Map.GroupName).PlayerJoined(player.ToClient());
            await Clients.Group(CurrentPlayer.Map.GroupName).SendAsync("InvokeMethod", "Map.PlayerJoined", player.ToClient());



            await Groups.AddToGroupAsync(Context.ConnectionId, CurrentPlayer.Map.GroupName);
            await Clients.Caller.SendAsync("InvokeMethod", "LoadLevel", GetInitialDataForPlayer(player));
            //await Clients.Caller.LoadLevel(GetInitialDataForPlayer(player));

            await base.OnConnectedAsync();
        }

        static private async Task<bool> CheckPlayerLeft(Hero player)
        {
            if (player.Connections.Count == 0 && player.CurrentBattle == null)
            {
                playersOnline.Remove(player.Owner.Id);
                await hubContext.Clients.Group(player.Map.GroupName).SendAsync("Map.PlayerLeft", player.Name);
                //await Clients.Group(player.Map.GroupName).PlayerLeft(player.UserName);

                return true;
            }

            return false;
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            CurrentPlayer.Connections.Remove(Context.ConnectionId);

            await CheckPlayerLeft(CurrentPlayer);

            await base.OnDisconnectedAsync(exception);
        }

    }
}