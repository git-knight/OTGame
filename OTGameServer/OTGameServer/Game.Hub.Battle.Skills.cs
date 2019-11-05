using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TGame.Match3;

namespace TGame
{
    class SkillDescription : Attribute
    {
        public int Id { get; }
        public string Name { get; }
        public int[] ColorsNeeded { get; }

        public SkillDescription(int id, string name, int[] colorsNeeded)
        {
            Id = id;
            Name = name;
            ColorsNeeded = colorsNeeded;
        }
    }

    public class Skill
    {
        public int Id { get { return descr.Id; } }

        Func<Board, MoveResult> skillFunc;
        SkillDescription descr;

        public Skill(MethodInfo skillMethod)
        {
            descr = skillMethod.GetCustomAttribute<SkillDescription>();
            skillFunc = (Func<Board, MoveResult>)Delegate.CreateDelegate(typeof(Func<Board, MoveResult>), skillMethod);
        }

        public bool CanUse(Board board) 
            => board.CurrentPlayer.Stones.Zip(descr.ColorsNeeded, (a, b) => a >= b).All(x => x);

        public MoveResult Use(Board board) 
            => skillFunc(board) + MoveResult.ConsumeStones(descr.ColorsNeeded);
    }

    public partial class GameHub
    {
        static private Skill[] skills = typeof(GameHub)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Where(x => x.GetCustomAttribute<SkillDescription>() != null)
            .Select(x => new Skill(x))
            .ToArray();

        static private Skill FindSKill(int id)
            => skills.FirstOrDefault(s => s.Id == id);

        [SkillDescription(1, "Fireball", new int[] { 3, 3, 3 })]
        static private MoveResult Fireball(Board board) 
            => MoveResult.DealDamage(5);
    }
}