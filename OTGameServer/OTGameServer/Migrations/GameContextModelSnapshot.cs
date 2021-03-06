﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TGame;

namespace TGame.Migrations
{
    [DbContext(typeof(GameContext))]
    partial class GameContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("TGame.Entities.Hero", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Exp");

                    b.Property<int>("Gender");

                    b.Property<int>("Level");

                    b.Property<int>("MapId");

                    b.Property<int>("Money");

                    b.Property<int>("TotalPlays");

                    b.Property<int>("TotalWins");

                    b.Property<string>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("PlayerHeroes");
                });

            modelBuilder.Entity("TGame.Entities.ItemStack", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Amount");

                    b.Property<int>("OwnerId");

                    b.Property<int>("TypeId");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("PlayerItems");
                });

            modelBuilder.Entity("TGame.Entities.PlayerStatistics", b =>
                {
                    b.Property<int>("HeroId");

                    b.Property<int>("StatType");

                    b.Property<int>("ClassId");

                    b.Property<int>("Counter");

                    b.HasKey("HeroId", "StatType", "ClassId");

                    b.HasAlternateKey("ClassId", "HeroId", "StatType");

                    b.ToTable("PlayerStatistics");
                });

            modelBuilder.Entity("TGame.Entities.QuestCompletion", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("IsCompleted");

                    b.Property<int>("OwnerId");

                    b.Property<int>("QuestId");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("PlayerQuests");
                });

            modelBuilder.Entity("TGame.Entities.QuestTaskCompletion", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("QuestCompletionId");

                    b.Property<int>("SavedAmount");

                    b.Property<int>("TaskId");

                    b.HasKey("Id");

                    b.HasIndex("QuestCompletionId");

                    b.ToTable("PlayerQuestCompletion");
                });

            modelBuilder.Entity("TGame.Entities.User", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("TGame.Entities.User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("TGame.Entities.User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("TGame.Entities.User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("TGame.Entities.User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TGame.Entities.Hero", b =>
                {
                    b.HasOne("TGame.Entities.User", "Owner")
                        .WithOne("Hero")
                        .HasForeignKey("TGame.Entities.Hero", "UserId");

                    b.OwnsOne("TGame.Entities.Stats", "BaseStats", b1 =>
                        {
                            b1.Property<int>("HeroId");

                            b1.Property<int>("Attack");

                            b1.Property<int>("Counterfury");

                            b1.Property<int>("Defense");

                            b1.Property<int>("Fury");

                            b1.Property<int>("Health");

                            b1.Property<int>("Lifesteal");

                            b1.Property<int>("MaxDamage");

                            b1.Property<int>("MinDamage");

                            b1.Property<int>("Resistance");

                            b1.Property<int>("Speed");

                            b1.HasKey("HeroId");

                            b1.ToTable("PlayerHeroes");

                            b1.HasOne("TGame.Entities.Hero")
                                .WithOne("BaseStats")
                                .HasForeignKey("TGame.Entities.Stats", "HeroId")
                                .OnDelete(DeleteBehavior.Cascade);
                        });

                    b.OwnsOne("TGame.Point", "Location", b1 =>
                        {
                            b1.Property<int>("HeroId");

                            b1.Property<int>("X");

                            b1.Property<int>("Y");

                            b1.HasKey("HeroId");

                            b1.ToTable("PlayerHeroes");

                            b1.HasOne("TGame.Entities.Hero")
                                .WithOne("Location")
                                .HasForeignKey("TGame.Point", "HeroId")
                                .OnDelete(DeleteBehavior.Cascade);
                        });
                });

            modelBuilder.Entity("TGame.Entities.ItemStack", b =>
                {
                    b.HasOne("TGame.Entities.Hero", "Owner")
                        .WithMany("Inventory")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TGame.Entities.PlayerStatistics", b =>
                {
                    b.HasOne("TGame.Entities.Hero")
                        .WithMany("Statistics")
                        .HasForeignKey("HeroId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TGame.Entities.QuestCompletion", b =>
                {
                    b.HasOne("TGame.Entities.Hero", "Owner")
                        .WithMany("Quests")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TGame.Entities.QuestTaskCompletion", b =>
                {
                    b.HasOne("TGame.Entities.QuestCompletion", "QuestCompletion")
                        .WithMany("TaskCompletion")
                        .HasForeignKey("QuestCompletionId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
