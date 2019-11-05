using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TGame.Entities;

namespace TGame
{
    public class GameContext : IdentityDbContext<User>
    {
        public DbSet<Hero> PlayerHeroes { get; set; }
        public DbSet<ItemStack> PlayerItems { get; set; }
        public DbSet<QuestTaskCompletion> PlayerQuestCompletion { get; set; }
        public DbSet<QuestCompletion> PlayerQuests { get; set; }
        public DbSet<PlayerStatistics> PlayerStatistics { get; set; } 

        // public DbSet<Map> Maps { get; set; }
        // public DbSet<MapObject> MapObjects { get; set; }
        // public DbSet<Monster> Monsters { get; set; }
        // public DbSet<MonsterClass> MonsterTypes { get; set; }
        // public DbSet<ItemClass> ItemClasses { get; set; }
        // public DbSet<Item> ItemTypes { get; set; }
        // public DbSet<Quest> Quests { get; set; }
        // public DbSet<QuestAvailabilityCondition> QuestAvailabilityConditions { get; set; } 
        // public DbSet<QuestTask> QuestTasks { get; set; }
        // public DbSet<QuestReward> QuestRewards { get; set; }
        /*
        public GameContext(DbContextOptions options)
            : base(options)
        {

        }*/

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseLazyLoadingProxies();
            var connection = @"host=localhost;port=5432;database=TGameDatabase;username=postgres;password=qwe";
            optionsBuilder.UseNpgsql(connection);

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // builder.Entity<Hero>().ToTable("Players");

            builder.Entity<PlayerStatistics>()
                    .HasKey(s => new { s.HeroId, s.StatType, s.ClassId });

            // builder.Entity<QuestAvailabilityCondition>()
            //         .HasDiscriminator<int>("ConditionType")
            //         .HasValue<MinimumLevelCondition>(1);

            // builder.Entity<QuestTask>()
            //         .HasDiscriminator<int>("TaskType")
            //         .HasValue<MonsterKillingQuest>(1)
            //         .HasValue<ItemQuest>(2);

            // builder.Entity<Item>()
            //         .HasDiscriminator<int>("ItemType")
            //         .HasValue<Equipment>(1);

            /*
            builder.Entity<Player>()
                .OwnsOne(u => u.Hero, hp =>
                  {
                      hp.OwnsOne(h => h.Stats, sp =>
                      {
                          sp.OwnsOne(s => s.Location).OwnsOne(l => l.Point);
                      })
                      .HasOne(h => h.Map);
                  });

            builder.Entity<Monster>()
                .OwnsOne(m => m.Stats, sp =>
                {
                    sp.OwnsOne(s => s.Location).OwnsOne(l => l.Point);
                });
            // builder.Entity<Stats>().OwnsOne(h => h.Location);
            builder.Entity<User>()
                .OwnsOne(h => h.Hero)
                .OwnsOne(h => h.Stats)
                .OwnsOne(s => s.Location);
            // builder.Entity<Hero>().OwnsOne(h => h.Stats);
            // builder.Entity<Hero>().HasOne(h => h.Map);
            builder.Entity<Monster>()
                .OwnsOne(h => h.Stats)
                .OwnsOne(s => s.Location);
            // builder.Entity<Stats>().OwnsOne(h => h.Location);
            builder.Entity<Monster>().HasOne(h => h.Map).WithMany(m => m.Monsters);

            builder.Entity<User>().ToTable("Players");
            */
            //builder.Entity<Hero>().HasOne(h => h.Map);
            //builder.Entity<Hero>().HasOne(h => h.Stats);
            //builder.Entity<Stats>().HasOne(s => s.Location);
            //builder.Entity<Location>().HasOne(s => s.Point);
            //builder.Entity<Hero>().ToTable("PlayerCharacters");
            //builder.Entity<Monster>().ToTable("Monsters");

            /* builder.Entity<Monster>()
                .HasOne(b => b.Location.Map)
                .WithMany(m => m.Monsters); */
        }
    }
}
