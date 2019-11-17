using Microsoft.EntityFrameworkCore.Migrations;

namespace TGame.Migrations
{
    public partial class MoreStats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BaseStats_Counterfury",
                table: "PlayerHeroes",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BaseStats_Fury",
                table: "PlayerHeroes",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BaseStats_Lifesteal",
                table: "PlayerHeroes",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BaseStats_Resistance",
                table: "PlayerHeroes",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BaseStats_Speed",
                table: "PlayerHeroes",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseStats_Counterfury",
                table: "PlayerHeroes");

            migrationBuilder.DropColumn(
                name: "BaseStats_Fury",
                table: "PlayerHeroes");

            migrationBuilder.DropColumn(
                name: "BaseStats_Lifesteal",
                table: "PlayerHeroes");

            migrationBuilder.DropColumn(
                name: "BaseStats_Resistance",
                table: "PlayerHeroes");

            migrationBuilder.DropColumn(
                name: "BaseStats_Speed",
                table: "PlayerHeroes");
        }
    }
}
