using Microsoft.EntityFrameworkCore.Migrations;

namespace Hass_Alarm.Migrations
{
    public partial class UpdatePin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PinCodes_ActionGroups_ActionGroupId",
                table: "PinCodes");

            migrationBuilder.AlterColumn<string>(
                name: "Pin",
                table: "PinCodes",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "ActionGroupId",
                table: "PinCodes",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "PinCodes",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PinCodes_ActionGroups_ActionGroupId",
                table: "PinCodes",
                column: "ActionGroupId",
                principalTable: "ActionGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PinCodes_ActionGroups_ActionGroupId",
                table: "PinCodes");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PinCodes");

            migrationBuilder.AlterColumn<int>(
                name: "Pin",
                table: "PinCodes",
                type: "int",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<int>(
                name: "ActionGroupId",
                table: "PinCodes",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PinCodes_ActionGroups_ActionGroupId",
                table: "PinCodes",
                column: "ActionGroupId",
                principalTable: "ActionGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
