using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using TGame.Entities;

namespace TGame.Match3
{
    public class Board
    {
        static private int _id = 0;
        public int Id { get; } = _id++;

        public const int Width = 5;
        public const int Height = 6;
        public const int ColorsCount = 3;

        public Battle Battle { get; }

        public string GroupName { get { return Battle.GroupName + ";board=" + Id; } }

        public WarriorBase PlayerLeft { get; }

        public WarriorBase PlayerRight { get; }

        public WarriorBase CurrentPlayer { get { return Turn % 2 == 0 ? PlayerLeft : PlayerRight; } }

        public int Turn { get; set; } = 0;
        public bool Finished { get; set; } = false;

        public DateTime TurnStartedAt { get; set; }
        public double TurnSecondsLeft { get { return 25 - (DateTime.UtcNow - TurnStartedAt).TotalSeconds; } }
        public Timer Timer { get; set; }

        int[,] cells = new int[Width, Height];

        int this[Point pt] { get { return cells[pt.X, pt.Y]; } set { cells[pt.X, pt.Y] = value; } }

        internal object ToClient()
        {
            return new
            {
                left = PlayerLeft.ToClient_Battle(),
                right = PlayerRight.ToClient_Battle(),
                turn = Turn,
                turnEndsIn = TurnSecondsLeft,
                cells
            };
        }

        public Board(Battle battle, WarriorBase p0, WarriorBase p1)
        {
            Battle = battle;

            PlayerLeft = p0;
            PlayerRight = p1;

            p0.Stones = new int[3];
            p1.Stones = new int[3];

            //p0.HP_Current = p0.MaxHealth;
            //p1.HP_Current = p1.MaxHealth;

            Regenerate();

            TurnStartedAt = DateTime.UtcNow;
        }

        public void Regenerate()
        {
            do
            {
                for (int i = 0; i < Width; i++)
                    for (int j = 0; j < Height; j++)
                        cells[i, j] = RNG.Next(0, ColorsCount + 1);
            }
            while (!IsBoardValid());
        }

        bool IsBoardValid()
        {
            int[,] realCells = cells;
            cells = realCells.Clone() as int[,];

            var res = DoCombinations();

            cells = realCells;
            if (!res.IsEmpty)
                return false;

            return HasValidMoves();
        }

        public bool HasValidMoves() 
        {
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    if (IsMoveValid(new Point(i, j), false) || IsMoveValid(new Point(i, j), true))
                        return true;

            return false;
        }

        public bool IsMoveValid(Point pt, bool isVertical) 
        {
            int[,] realCells = cells;
            cells = realCells.Clone() as int[,];
            
            var res = TryPlayMove(pt, isVertical);

            cells = realCells;
            return res.IsValid && !res.IsEmpty;
        }

        public MoveResult TryPlayMove(Point pt, bool isVertical)
        {
            if (!DoSwap(pt, isVertical))
                return MoveResult.Invalid;

            return DoCombinations();
        }

        MoveResult DoCombinations()
        {
            var res = new MoveResult(true);
            while (true)
            {
                var step = new MoveResult(true);

                Point right = new Point(1, 0);
                Point bottom = new Point(0, 1);
                int W = Width;
                int H = Height;

                var toRemove = new List<Point>(64);

                for (int dir = 0; dir < 2; dir++)
                {
                    for (int i = 0; i < W; i++)
                    {
                        var repeats = new List<Point>(7);
                        int repColor = -1;

                        for (int j = 0; j < H; j++)
                        {
                            Point coord = right*i + bottom*j;
                            int color = this[coord];
                            if (color != repColor)
                            {
                                if (repColor != -1 && repeats.Count >= 3)
                                {
                                    if (repeats.Count >= 4)
                                        step.Has4 = true;

                                    int num = repeats.Count * (repeats.Count - 2);

                                    if (repColor == 0)
                                        step.Skulls += num;
                                    else step.Colors[repColor - 1] += num;
                                    toRemove.AddRange(repeats);
                                }

                                repeats.Clear();
                                repColor = color;
                            }

                            repeats.Add(coord);
                        }

                        if (repColor != -1 && repeats.Count >= 3)
                        {
                            if (repeats.Count >= 4)
                                step.Has4 = true;

                            int num = repeats.Count * (repeats.Count - 2);

                            if (repColor == 0)
                                step.Skulls += num;
                            else step.Colors[repColor - 1] += num;
                            toRemove.AddRange(repeats);
                            //repeats.Clear();
                        }
                    }

                    W = Height;
                    H = Width;
                    right = bottom;
                    bottom = new Point(bottom.Y, bottom.X);
                }

                if (step.IsEmpty)
                    break;

                foreach (var coord in toRemove)
                    this[coord] = -1;

                PlayGravity();
                
                res += step;
            }

            return res;
        }

        void PlayGravity()
        {
            Point right = new Point(1, 0);
            Point bottom = new Point(0, 1);
            int W = Width;
            int H = Height;

            for (int dir = 0; dir < 2; dir++)
            {
                for (int i = 0; i < W; i++)
                {
                    var y = H - 1;
                    for (int j = H - 1; j >= 0; j--)
                    {
                        var coord = right*i + bottom*j;
                        if (this[coord] != -1)
                        {
                            var t = this[coord];
                            this[coord] = -1;
                            this[right*i + bottom*(y--)] = t;
                        }
                    }
                }

                W = Height;
                H = Width;
                right = bottom;
                bottom = new Point(bottom.Y, bottom.X);

            }
        }

        bool DoSwap(Point pt, bool isVertical)
        {
            var pt2 = new Point(pt.X + (isVertical ? 0 : 1), pt.Y + (isVertical ? 1 : 0));
            if (pt2.X >= Width || pt2.Y >= Height || this[pt2] == -1) 
                return false;
            

            int cell = this[pt];
            this[pt] = this[pt2];
            this[pt2] = cell;

            return true;
        }
    }

    public struct MoveResult 
    {
        public int[] Damage { get; set; }
        public int[] Colors { get; set; }
        public int Skulls { get; set; }
        public bool Has4 { get; set; }
        public bool IsCritical { get; set; }
        public bool IsValid { get; set; }

        public bool IsEmpty { get { return Skulls == 0 && !(Colors?.Any(x => x != 0) ?? false) && !Damage.Any(x => x != 0); } }

        public MoveResult(bool isValid)
        {
            Damage = new int[2];
            IsValid = isValid;
            Colors = new int[Board.ColorsCount];
            Skulls = 0;
            Has4 = false;
            IsCritical = false;
        }

        static public readonly MoveResult Invalid = new MoveResult { IsValid = false };

        static public MoveResult operator+(MoveResult a, MoveResult b)
        {
            return new MoveResult {
                IsValid = true,
                Colors = a.Colors.Zip(b.Colors, (x, y) => x + y).ToArray(),
                Damage = a.Damage.Zip(b.Damage, (x, y) => x + y).ToArray(),
                Skulls = a.Skulls + b.Skulls,
                Has4 = a.Has4 || b.Has4,
                IsCritical = a.IsCritical || b.IsCritical
            };
        }

        public object ToPlayer(int serverTurn)
        {
            return new
            {
                turn = serverTurn,
                colors = Colors ?? new int[3],
                dmgSelf = Damage[0],
                dmgEnemy = Damage[1],
                IsCritical,
                IsValid,
                IsEmpty
            };
        }

        internal static MoveResult DealDamage(int damage) 
            => new MoveResult(true) { Skulls = damage };

        internal static MoveResult ConsumeStones(int[] colorsNeeded)
            => new MoveResult(true) { Colors = colorsNeeded.Select(x => -x).ToArray() };
    }
}
