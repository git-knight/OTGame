using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TGame.Entities;

namespace TGame
{
    public partial class GameHub
    {
        static private Stats LevelupStats = new Stats {
            Health = 20,
            Attack = 5,
            Defense = 5,
            MinDamage = 2,
            MaxDamage = 2
        };

        private Task PlayerReceivedExperience() => PlayerReceivedExperience(CurrentPlayer);
        static private async Task PlayerReceivedExperience(Hero player)
        {
            if (player.Exp >= player.ExpToNextLevel)
            {
                player.Exp -= player.ExpToNextLevel;
                player.Level++;

                player.BaseStats += LevelupStats;
                player.HP_Current += LevelupStats.Health;

                await hubContext.Clients.Group(player.Name).SendAsync("InvokeMethod", "PlayerLeveledUp", new {
                    player.Level,
                    player.Exp,
                    player.ExpToNextLevel,
                    hp = player.HP_Current,
                    hpMax = player.Stats.Health
                });
            }
        }

        private void PlayerReceiveItem(QuestReward item, Hero player = null)
        {
            player = player ?? CurrentPlayer;

            if (item.Reward.IsStackable)
            {
                var stack = player.Inventory.FirstOrDefault(i => i.TypeId == item.Reward.Id);
                if (stack != null)
                {
                    stack.Amount += item.Amount;
                    return;
                }
            }

            gameContext.PlayerItems.Add(new ItemStack { Owner = player, TypeId = item.Reward.Id, Amount = item.Amount });
        }

        private int GetMaxEquipsCount(EquipmentSlot slot)
        {
            switch (slot)
            {
            case EquipmentSlot.None:
                throw new Exception("cannot get MaxEquipsCount for EquipmentSlot.None!");
            case EquipmentSlot.Weapon:
                return 2;
            default:
                throw new Exception("unknown EquipmentSlot." + slot + "!");
            }
        }

        public async Task EquipItem(int id)
        {
            var item = CurrentPlayer.Inventory.FirstOrDefault(i => i.Id == id);
            if (item == null || !(item.Type is Equipment))
                return;

            if (!item.IsEquipped)
            {
                var itemsToReplace = CurrentPlayer.Inventory.Where(i => i.IsEquipped && i.Type.Id == item.Type.Id).ToArray();
                if (itemsToReplace.Length >= GetMaxEquipsCount((item.Type as Equipment).Slot))
                {
                    var itemToReplace = itemsToReplace[rand.Next(itemsToReplace.Length)];
                    gameContext.Attach(itemToReplace);
                    itemToReplace.IsEquipped = false;
                }

                gameContext.Attach(item);
                item.IsEquipped = true;

                await gameContext.SaveChangesAsync();

                await Clients.Caller.SendAsync("InvokeMethod", "ActiveViewScript.OnItemEquipped", id);
            }
            else
            {
                gameContext.Attach(item);
                item.IsEquipped = false;
                await gameContext.SaveChangesAsync();
                await Clients.Caller.SendAsync("InvokeMethod", "ActiveViewScript.OnItemUnequipped", id);
            }
        }

        public async Task OpenInventory() 
        {
            await Clients.Caller.SendAsync("InvokeMethod", "PlayerOpenedInventory", CurrentPlayer.Inventory.Select(i => i.ToPlayer()).ToArray());
        }

        public async Task ShowActiveQuests()
        {
            await Clients.Caller.SendAsync("InvokeMethod", "ShowActiveQuests", CurrentPlayer.Quests.Select(q => q.ToPlayer()).ToArray());
        }

        public async Task ShowHeroStats() 
        {
            var player = CurrentPlayer;

            await Clients.Caller.SendAsync("InvokeMethod", "ShowHeroStats",
                new
                {
                    player.Level,
                    player.BaseStats,
                    player.Stats,
                    player.Exp,
                    EquippedItems = player.Inventory.Where(i => i.IsEquipped).Select(i => i.ToPlayer()).ToArray(),
                    player.TotalWins,
                    player.TotalPlays
                });
        }

        public async Task ShowFullPlayerInfo(string playerNick)
        {
            Hero player = playerNick == CurrentPlayer.Name ? CurrentPlayer : (playersOnline.GetValueOrDefault(playerNick) ?? gameContext.PlayerHeroes.Include(p=>p.Owner).FirstOrDefault(p => p.Name == playerNick));
            if (player == null)
                return;

            await Clients.Caller.SendAsync("InvokeMethod", "ShowHeroInfo",
                new
                {
                    nick = player.Name,
                    player.Level,
                    health = player.Stats.Health,
                    equippedItems = player.Inventory.Where(x => x.IsEquipped).Select(x => x.ToPlayer()).ToArray()
                });
        }

        public async Task ProceedQuest(int id)
        {
            if (id < 0 || id >= Quests.Count())
                return;

            var quest = GameHub.Quests[id];

            var questCompletion = CurrentPlayer.Quests.FirstOrDefault(q => q.QuestId == id);
            if (questCompletion == null)
            {
                if (quest.AvailabilityConditions.All(c => c.IsTrueFor(CurrentPlayer)))
                {
                    questCompletion = new QuestCompletion
                    {
                        Owner = CurrentPlayer,
                        QuestId = id,
                        Quest = quest,
                        TaskCompletion = quest.Tasks.Where(t => !t.IsStatic).Select(t => t.ToCompletionInfo(CurrentPlayer)).Where(t => t != null).ToList()
                    };
                    gameContext.Attach(CurrentPlayer);
                    gameContext.PlayerQuests.Add(questCompletion);
                    await gameContext.SaveChangesAsync();
                }
                await ShowUnitQuests(quest.MapObjectId);
                return;
            }

            if (questCompletion.IsCompleted)
                return;

            if (quest.Tasks.All(c => c.IsTrueFor(CurrentPlayer)))
            {
                await Clients.Caller.SendAsync("InvokeMethod", "PlayerRewarded", new
                {
                    money = quest.RewardGold,
                    exp = quest.RewardExp,
                    items = quest.RewardItems.Select(i => i.ToPlayer()).ToArray()
                });

                gameContext.Attach(CurrentPlayer);
                questCompletion.IsCompleted = true;
                CurrentPlayer.Money += quest.RewardGold;
                CurrentPlayer.Exp += quest.RewardExp;
                foreach(var item in quest.RewardItems)
                    PlayerReceiveItem(item);
                await PlayerReceivedExperience();
                await gameContext.SaveChangesAsync();
            }

            await ShowUnitQuests(quest.MapObjectId);
        }
    }
}