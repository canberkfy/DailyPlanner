using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyPlanner.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyTaskFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DailyTasks_DailyPlans_DailyPlanId",
                table: "DailyTasks");

            migrationBuilder.DropColumn(
                name: "EstimatedDuration",
                table: "DailyTasks");

            migrationBuilder.DropColumn(
                name: "SuggestedStartTime",
                table: "DailyTasks");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "DailyTasks",
                newName: "UserId");

            migrationBuilder.AlterColumn<int>(
                name: "DailyPlanId",
                table: "DailyTasks",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "DailyTasks",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "DailyTasks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_DailyTasks_DailyPlans_DailyPlanId",
                table: "DailyTasks",
                column: "DailyPlanId",
                principalTable: "DailyPlans",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DailyTasks_DailyPlans_DailyPlanId",
                table: "DailyTasks");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "DailyTasks");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "DailyTasks");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "DailyTasks",
                newName: "Name");

            migrationBuilder.AlterColumn<int>(
                name: "DailyPlanId",
                table: "DailyTasks",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EstimatedDuration",
                table: "DailyTasks",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "SuggestedStartTime",
                table: "DailyTasks",
                type: "time",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DailyTasks_DailyPlans_DailyPlanId",
                table: "DailyTasks",
                column: "DailyPlanId",
                principalTable: "DailyPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
