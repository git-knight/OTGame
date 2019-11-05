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
                if (pl is Hero player)
                {
                    //await Clients.Group(CurrentPlayer.Map.GroupName).BattleStarted(CurrentPlayer.Location, CurrentBattle.PlayerNames);
                    await hubContext.Clients.Group(player.Map.GroupName).SendAsync("InvokeMethod", "Map.BattleStarted", player.Location, battle.PlayerNames);

                    await hubContext.Groups.RemoveFromGroupAsync(player.Connections.First(), player.Map.GroupName);

                    await hubContext.Groups.AddToGroupAsync(player.Connections.First(), battle.GroupName);
                    await hubContext.Groups.AddToGroupAsync(player.Connections.First(), battle.Board.GroupName);

                    //await Clients.Group(CurrentBattle.Board.GroupName).BattleJoined(CurrentPlayer.Location, CurrentBattle.ToClient(CurrentPlayer.UserName));
                    await hubContext.Clients.Group(battle.Board.GroupName).SendAsync("InvokeMethod", "BattleJoined", battle.ToClient(player.Name));
                }

            Board board = battle.Board;
            board.Timer = new System.Timers.Timer(2500 * 1000);
            board.Timer.Elapsed += async (a, b) => await LoseMove(board);
            StartTurnTimer(board);
        }

        private async Task RejoinBattle()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, CurrentBattle.GroupName);
            await Groups.AddToGroupAsync(Context.ConnectionId, CurrentBattle.Board.GroupName);

            //await Clients.Group(CurrentPlayer.UserName).BattleJoined(CurrentPlayer.Location, CurrentBattle.ToClient(CurrentPlayer.UserName));
            await Clients.Group(CurrentPlayer.Name).SendAsync("InvokeMethod", "BattleJoined", CurrentBattle.ToClient(CurrentPlayer.Name));
        }

        static private void StartTurnTimer(Board board)
        {
            //board.Timer.Start();
            board.TurnStartedAt = DateTime.UtcNow;
        }

        static public async Task LoseMove(Board board)
        {
            MoveResult res = MoveResult.Invalid;
            ApplyMoveResult(board, ref res);
            await hubContext.Clients.Group(board.GroupName).SendAsync("InvokeMethod", "Battle.Board.OnActionPerformed", new Point(-1, -1), true, res.ToPlayer(board.Turn));

            while (!board.Finished && board.CurrentPlayer is Monster)
                await DoAIMove(board);

            if (!board.Finished)
                StartTurnTimer(board);
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

            while (CurrentBattle != null && CurrentBoard.CurrentPlayer is Monster)
                await DoAIMove(CurrentBoard);

            if (CurrentBattle != null)
                StartTurnTimer(CurrentBoard);
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

            while (CurrentBattle != null && CurrentBoard.CurrentPlayer is Monster)
                await DoAIMove(CurrentBoard);

            if (CurrentBattle != null)
                StartTurnTimer(CurrentBoard);
        }

        static private async Task FinishTurn(Board board)
        {
            //await Clients.Group(board.GroupName).BattleTurnFinished(point, isVertical, new { dmgSelf = damage[0], dmgEnemy = damage[1] });


            WarriorBase[] players = { board.PlayerLeft, board.PlayerRight };
            if (players.Any(p => p.HP_Current <= 0)) 
            {
                using (var gameContext = new GameContext())
                {
                    board.Finished = true;

                    await hubContext.Clients
                        .Group(players[0].Map.GroupName)
                        .SendAsync("InvokeMethod", "Map.BattleFinished", players[0].Location);


                    foreach (var pl in players)
                    {
                        if (pl is Hero winner)
                        {
                            gameContext.Attach(winner);

                            var enemy = players[0] == pl ? players[1] : players[0];

                            int exp = -1;
                            int money = 0;

                            if (winner.HP_Current > 0)
                            {
                                exp = 10;
                                money = RNG.Next(7, 15);

                                winner.Money += money;
                                winner.Exp += exp;
                                winner.TotalWins++;
                                if (enemy is Monster monster)
                                {
                                    var stats = winner.Statistics.FirstOrDefault(s => s.StatType == StatisticsType.MonsterKillCounter && s.ClassId == monster.Type.Id);
                                    if (stats == null)
                                    {
                                        stats = new PlayerStatistics
                                        {
                                            ClassId = monster.Type.Id,
                                            HeroId = winner.Id,
                                            StatType = StatisticsType.MonsterKillCounter,
                                            Counter = 0
                                        };
                                        gameContext.PlayerStatistics.Add(stats);
                                    }
                                    stats.Counter++;
                                }

                                await PlayerReceivedExperience(winner);
                            }

                            winner.OnBattleFinished();

                            if (await CheckPlayerLeft(winner))
                                continue;

                            foreach (var conn in winner.Connections)
                                await hubContext.Groups.AddToGroupAsync(conn, winner.Map.GroupName);

                            await hubContext.Clients.Group(winner.Name).SendAsync("InvokeMethod", "BattleFinished",
                                new
                                {
                                    exp,
                                    money,
                                    health = winner.HP_Current
                                },
                                GetInitialDataForPlayer(winner)
                            );
                            /*
                            await Clients.Group(winner.Name).BattleFinished(
                                new
                                {
                                    exp,
                                    money
                                }, 
                                GetInitialDataForPlayer(winner)
                            );//*/


                        }
                    }

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
                res.Damage[0] = 8;

            else if (res.Skulls > 0)
                res.Damage[1] += Math.Max(3, res.Skulls + me.Stats.Attack - opp.Stats.Defense + rand.Next(me.Stats.MinDamage, me.Stats.MaxDamage + 1));

            me.HP_Current -= res.Damage[0];
            opp.HP_Current -= res.Damage[1];

            me.Stones = me.Stones.Zip(res.Colors, (a, b) => a + b).ToArray();

            board.Turn += res.Has4 ? 2 : 1;
        }

        static private async Task RegenerateBoard(Board board)
        {
            board.Regenerate();
            //await Clients.Group(board.GroupName).BoardRegenerated(board.ToClient());
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