using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TGame.Entities;
using TGame.Match3;

namespace TGame
{
    public partial class GameHub
    {
        public Board CurrentBoard { get { return CurrentBattle.Board; } }
        public bool IsPlayerRight { get { return CurrentPlayer == CurrentBoard.PlayerRight; } }

        public WarriorBase Opponent { get { return CurrentPlayer == CurrentBoard.PlayerRight ? CurrentBoard.PlayerLeft : CurrentBoard.PlayerRight; } }

        static private async Task StartBattle(WarriorBase[] opps, WarriorBase inv)
        {
            var battle = new Battle(inv, opps);

            battles.Add(battle);
            foreach (var pl in battle.Players) 
            {
                pl.StopHealthRegen();

                if (pl is Hero player)
                {
                    await hubContext.Clients.Group(player.Map.GroupName).SendAsync("InvokeMethod", "Map.BattleStarted", player.Location, battle.PlayerNames);

                    await hubContext.Groups.RemoveFromGroupAsync(player.Connections.First(), player.Map.GroupName);

                    await hubContext.Groups.AddToGroupAsync(player.Connections.First(), battle.GroupName);
                    await hubContext.Groups.AddToGroupAsync(player.Connections.First(), battle.Board.GroupName);

                    await hubContext.Clients.Group(battle.Board.GroupName).SendAsync("InvokeMethod", "BattleJoined", battle.ToClient(player.Name));
                }
            }

            Board board = battle.Board;
            board.Timer = new System.Timers.Timer(2500 * 1000);
            board.Timer.Elapsed += async (a, b) => await LoseMove(board);
            
            await OnTurnStarted(board);
        }

        private async Task RejoinBattle()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, CurrentBattle.GroupName);
            await Groups.AddToGroupAsync(Context.ConnectionId, CurrentBattle.Board.GroupName);

            await Clients.Group(CurrentPlayer.Name).SendAsync("InvokeMethod", "BattleJoined", CurrentBattle.ToClient(CurrentPlayer.Name));
        }

        static private void StartTurnTimer(Board board)
        {
            //board.Timer.Start();
            board.TurnStartedAt = DateTime.UtcNow;
        }

        static private async Task OnTurnStarted(Board board)
        {
            while (!board.Finished && board.CurrentPlayer is Monster)
                await DoAIMove(board);

            if (!board.Finished)
                StartTurnTimer(board);
        }

        static public async Task LoseMove(Board board)
        {
            MoveResult res = MoveResult.Invalid;
            ApplyMoveResult(board, ref res);
            await hubContext.Clients.Group(board.GroupName).SendAsync("InvokeMethod", "Battle.Board.OnActionPerformed", new Point(-1, -1), true, res.ToPlayer(board.Turn));

            await OnTurnStarted(board);
        }

        public async Task DoBoardMove(Point point, bool isVertical)
        {
            if (CurrentBattle == null || CurrentBoard == null)
                return;

            if (CurrentBoard.CurrentPlayer.Name != CurrentPlayer.Name)
                return;

            MoveResult res = CurrentBoard.TryPlayMove(point, isVertical);
            if (!res.IsValid)
                return;

            ApplyMoveResult(CurrentBoard, ref res);
            await Clients.Group(CurrentBoard.GroupName).SendAsync("InvokeMethod", "Battle.Board.OnActionPerformed", point, isVertical, res.ToPlayer(CurrentBoard.Turn));
            await FinishTurn(CurrentBoard);

            if (CurrentBattle != null)
                await OnTurnStarted(CurrentBoard);
        }

        public async Task UseSkill(int id)
        {
            if (CurrentBoard.CurrentPlayer.Name != CurrentPlayer.Name)
                return;

            var skill = FindSKill(id);
            if (skill == null || !skill.CanUse(CurrentBoard))
                return;

            var moveResult = skill.Use(CurrentBoard);
            ApplyMoveResult(CurrentBoard, ref moveResult);
            await Clients.Group(CurrentBoard.GroupName).SendAsync("InvokeMethod", "Battle.Board.OnSkillUsed", id, moveResult.ToPlayer(CurrentBoard.Turn));
            await FinishTurn(CurrentBoard);

            await OnTurnStarted(CurrentBoard);
        }

        static private async Task FinishTurn(Board board)
        {
            //await Clients.Group(board.GroupName).BattleTurnFinished(point, isVertical, new { dmgSelf = damage[0], dmgEnemy = damage[1] });


            WarriorBase[] battlers = { board.PlayerLeft, board.PlayerRight };
            if (battlers.Any(p => p.HP_Current <= 0)) 
            {
                using (var gameContext = new GameContext())
                {
                    board.Finished = true;

                    await hubContext.Clients
                        .Group(battlers[0].Map.GroupName)
                        .SendAsync("InvokeMethod", "Map.BattleFinished", battlers[0].Location);

                    var deadPlayers = battlers.Where(x => x.HP_Current <= 0).ToArray();

                    foreach (var battler in battlers)
                        battler.StartHealthRegen();

                    foreach (Hero player in battlers.OfType<Hero>())
                    {
                        gameContext.Attach(player);

                        var enemy = battlers[0] == player ? battlers[1] : battlers[0];

                        int exp = -1;
                        int money = 0;

                        if (!deadPlayers.Contains(player))
                        {
                            exp = 10;
                            money = RNG.Next(7, 15);

                            player.Money += money;
                            player.Exp += exp;
                            player.TotalWins++;
                            if (enemy is Monster monster)
                            {
                                var stats = player.Statistics.FirstOrDefault(s => s.StatType == StatisticsType.MonsterKillCounter && s.ClassId == monster.Type.Id);
                                if (stats == null)
                                {
                                    stats = new PlayerStatistics
                                    {
                                        ClassId = monster.Type.Id,
                                        HeroId = player.Id,
                                        StatType = StatisticsType.MonsterKillCounter,
                                        Counter = 0
                                    };
                                    gameContext.PlayerStatistics.Add(stats);
                                }
                                stats.Counter++;
                            }

                            await PlayerReceivedExperience(player);
                        }

                        if (await CheckPlayerLeft(player))
                            continue;

                        foreach (var conn in player.Connections)
                            await hubContext.Groups.AddToGroupAsync(conn, player.Map.GroupName);

                        await hubContext.Clients.Group(player.Name).SendAsync("InvokeMethod", "BattleFinished",
                            new
                            {
                                exp,
                                money,
                                health = player.HP_Current
                            },
                            GetInitialDataForPlayer(player)
                        );
                    }

                    foreach (var battler in battlers)
                        battler.OnBattleFinished();

                    battles.Remove(board.Battle);
                    await gameContext.SaveChangesAsync();
                }

                return;
            }

            board.TurnStartedAt = DateTime.UtcNow;

            if (!board.HasValidMoves())
                await RegenerateBoard(board);

            return;
        }

        static private void ApplyMoveResult(Board board, ref MoveResult res)
        {
            board.Timer.Stop();

            var me = (board.Turn % 2 == 1) ? board.PlayerRight : board.PlayerLeft;
            var opp = (board.Turn % 2 == 0) ? board.PlayerRight : board.PlayerLeft;

            if (res.IsEmpty)
                res.Damage[0] = 4;
            else if (res.Skulls > 0)
                res.Damage[1] +=  res.Skulls + me.Stats.Attack + rand.Next(me.Stats.MinDamage, me.Stats.MaxDamage + 1);

            if (res.Damage[1] > 0)
            {
                if (me.Stats.Fury > opp.Stats.Counterfury && rand.NextDouble() < ((-20) / (me.Stats.Fury - opp.Stats.Counterfury + 19.5) + 1))
                {
                    res.IsCritical = true;
                    res.Damage[1] += (res.Damage[1] + 1) / 2;
                }

                if (me.Stats.Lifesteal > opp.Stats.Resistance)
                    res.Damage[0] -= (int)Math.Round(res.Damage[1] * ((-20) / (me.Stats.Lifesteal - opp.Stats.Resistance + 19.5) + 1));
            }

            if (res.Damage[0] > 0)
                res.Damage[0] = Math.Max(3, res.Damage[0] - me.Stats.Defense);

            if (res.Damage[1] > 0)
                res.Damage[1] = Math.Max(3, res.Damage[1] - opp.Stats.Defense);

            me.HP_Current -= res.Damage[0];
            opp.HP_Current -= res.Damage[1];

            me.Stones = me.Stones.Zip(res.Colors, (a, b) => a + b).ToArray();

            board.Turn += res.Has4 ? 2 : 1;
        }

        static private async Task RegenerateBoard(Board board)
        {
            board.Regenerate();
            await hubContext.Clients.Group(board.GroupName).SendAsync("InvokeMethod", "Battle.Board.OnRegenerated", board.ToClient());
        }

        static private async Task DoAIMove(Board board)
        {
            await Task.Delay(3000);

            List<Point>[] availableMoves = { new List<Point>(), new List<Point>() };

            for (int i = 0; i < Board.Width; i++)
                for (int j = 0; j < Board.Height; j++)
                    for (int t = 0; t < 2; t++)
                        if (board.IsMoveValid(new Point(i, j), t == 1))
                            availableMoves[t].Add(new Point(i, j));
            
            var m = RNG.Next(0, availableMoves[0].Count + availableMoves[1].Count);

            var isVertical = m >= availableMoves[0].Count;
            var pt = isVertical ? availableMoves[1][m-availableMoves[0].Count] : availableMoves[0][m];

            MoveResult res = board.TryPlayMove(pt, isVertical);

            ApplyMoveResult(board, ref res);
            await hubContext.Clients.Group(board.GroupName).SendAsync("InvokeMethod", "Battle.Board.OnActionPerformed", pt, isVertical, res.ToPlayer(board.Turn));
            await FinishTurn(board);

        }
    }
}