using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace TGame.Entities
{
    public abstract class QuestAvailabilityCondition
    {
        public int Id { get; set; }
        public Quest Quest { get; set; }
        public int RequiredAmount { get; set; }

        [NotMapped]
        public abstract bool IsStatic { get; }

        public abstract bool IsTrueFor(Hero player);
    }

    public class MinimumLevelCondition : QuestAvailabilityCondition
    {
        public MinimumLevelCondition(JToken data)
        {
            RequiredAmount = (int)data["RequiredAmount"];
        }

        public override bool IsStatic => true;
        public override bool IsTrueFor(Hero player) 
            => player.Level >= RequiredAmount;
    }

    public abstract class QuestTask
    {
        private static int _idCounter = 0;

        public int Id { get; set; } = _idCounter++;
        public Quest Quest { get; set; }
        public int RequiredAmount { get; set; }

        [NotMapped] public abstract bool IsStatic { get; }

        public abstract int GetCounterFor(Hero player, int savedAmount);
        public abstract QuestTaskCompletion ToCompletionInfo(Hero player);
        public abstract string GetStatusString();


        public int ActualAmount(QuestCompletion qc) => GetCounterFor(qc.Owner, qc.TaskCompletion.FirstOrDefault(c => c.Task == this)?.SavedAmount ?? 0);
        public bool IsCompleted(QuestCompletion qc) => ActualAmount(qc) >= RequiredAmount;
        public bool IsCompleted(Hero hero) => IsCompleted(hero.Quests.First(q => q.Quest == Quest));


        internal object ToPlayer(QuestCompletion qc)
        {
            return new
            {
                statusText = GetStatusString()
                        .Replace("{current}", Math.Min(RequiredAmount, ActualAmount(qc)) + "")
                        .Replace("{max}", RequiredAmount + ""),
                isCompleted = IsCompleted(qc)
            };
        }
    }

    public class MonsterKillingQuest : QuestTask
    {
        public MonsterKillingQuest(JToken data)
        {
            MonsterType = GameHub.MonsterClasses[(int)data["MonsterType"]];
            RequiredAmount = (int)data["RequiredAmount"];
        }

        public MonsterClass MonsterType { get; set; }
        public override bool IsStatic => false;

        public override int GetCounterFor(Hero player, int savedAmount)
        {
            return player.GetKillCounter(MonsterType.Id) - savedAmount;
        }

        public override string GetStatusString()
            => "{current}/{max} " + MonsterType.Name + "s killed";

        public override QuestTaskCompletion ToCompletionInfo(Hero player)
        {
            return new QuestTaskCompletion
            {
                Task = this,
                SavedAmount = player.GetKillCounter(MonsterType.Id)
            };
        }
    }

    public class ItemQuest : QuestTask
    {
        public int ItemId { get; set; }
        public Item Item { get; set; }

        public override bool IsStatic => true;

        public override int GetCounterFor(Hero player, int savedAmount) 
            => player.GetItemCounter(ItemId);

        public override string GetStatusString()
        {
            if (RequiredAmount > 0)
                return "{current}/{max} " + Item.Name + " acquired";
            else return "{current}/{max} " + Item.Name + " used";
        }

        public override QuestTaskCompletion ToCompletionInfo(Hero player)
        {
            throw new NotImplementedException();
        }
    }

    public class QuestReward
    {
        public QuestReward(JToken data)
        {
            Reward = GameHub.Items[(int)data["ItemType"]];
            Amount = (int)data["Amount"];
        }

        public int Id { get; set; }
        public Quest Quest { get; set; }
        public Item Reward { get; set; }
        public int Amount { get; set; }

        internal object ToPlayer()
        {
            if (!Reward.IsStackable)
            {
                return new
                {
                    name = Reward.Name
                };
            }
            else
            {
                return new
                {
                    name = Reward.Name,
                    Amount
                };
            }
        }
    }

    public enum QuestStatus
    {
        None,
        Available,
        Completed
    }

    public class Quest
    {
        private static int _idCounter = 0;

        public int Id { get; set; } = _idCounter++;
        public string Title { get; set; }
        public string Description { get; set; }
        public string Dialogues { get; set; }

        public int MapObjectId { get; set; }

        public QuestAvailabilityCondition[] AvailabilityConditions { get; set; }
        public QuestTask[] Tasks { get; set; }

        public int RewardExp { get; set; }
        public int RewardGold { get; set; }
        public QuestReward[] RewardItems { get; set; }

        public Quest(MapObject mapObject, JToken data)
        {
            MapObjectId = mapObject.Id;

            Title = (string)data["Title"];
            Description = (string)data["Description"];
            Dialogues = (string)data["Dialogues"];
            RewardExp = (int)data["RewardExp"];
            RewardGold = (int)data["RewardGold"];
            AvailabilityConditions = ((JArray)data["Conditions"]).Select(m => new MinimumLevelCondition(m) { Quest = this }).ToArray();
            Tasks = ((JArray)data["Tasks"]).Select(m => new MonsterKillingQuest(m) { Quest = this }).ToArray();
            RewardItems = ((JArray)data["RewardItems"]).Select(m => new QuestReward(m) { Quest = this }).ToArray();
        }

        internal QuestStatus GetStatusFor(Hero player)
        {
            var completion = player.Quests.FirstOrDefault(q => q.QuestId == Id);
            if (completion == null)
            {
                if (AvailabilityConditions.All(c => c.IsTrueFor(player)))
                    return QuestStatus.Available;

                return QuestStatus.None;
            }

            if (!completion.IsCompleted && completion.CanTurnIn())
                return QuestStatus.Completed;

            return QuestStatus.None;
        }

        internal object ToPlayer(Hero receiver)
        {
            return new
            {
                Id,
                Title,
                Description,
                Dialogues,
                status = GetStatusFor(receiver)
            };
        }
    }
}
