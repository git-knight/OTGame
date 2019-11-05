﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace TGame.Entities
{
    public class QuestTaskCompletion
    {
        public int Id { get; set; }

        public int QuestCompletionId { get; set; }
        public QuestCompletion QuestCompletion { get; set; }

        public int TaskId { get; set; }
        [NotMapped] public QuestTask Task { get { return GameHub.QuestTasks[TaskId]; } set { TaskId = value.Id; } }

        public int SavedAmount { get; set; }

        [NotMapped]
        public int ActualAmount { get { return Math.Min(Task.RequiredAmount, Task.GetCounterFor(QuestCompletion.Owner, SavedAmount)); } }

        public bool IsCompleted()
        {
            return ActualAmount >= Task.RequiredAmount;
        }

        internal object ToPlayer()
        {
            return new
            {
                statusText = Task.GetStatusString()
                        .Replace("{current}", ActualAmount + "")
                        .Replace("{max}", Task.RequiredAmount + ""),
                isCompleted = IsCompleted()
            };
        }
    }

    public class QuestCompletion
    {
        public int Id { get; set; }

        public int OwnerId { get; set; }
        public virtual Hero Owner { get; set; }

        public int QuestId { get; set; }
        [NotMapped] public Quest Quest { get { return GameHub.Quests[QuestId]; } set { QuestId = value.Id; } }

        public virtual IEnumerable<QuestTaskCompletion> TaskCompletion { get; set; }

        public bool IsCompleted { get; set; }

        internal object ToPlayer()
        {
            return new
            {
                Quest.Title,
                Quest.Description,
                completionInfo = TaskCompletion.Select(t => t.ToPlayer()).ToArray(),
                IsCompleted
            };
        }
    }
}
