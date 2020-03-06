using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hass_Alarm.Migrations
{
    public partial class ActionsAndPin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActionGroups",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Actions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ActionGroupId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Input_Boolean_EntityId = table.Column<string>(nullable: true),
                    Input_Boolean_SetState = table.Column<bool>(nullable: false),
                    Service_Domain = table.Column<string>(nullable: true),
                    Service_Name = table.Column<string>(nullable: true),
                    Service_Fields = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Actions_ActionGroups_ActionGroupId",
                        column: x => x.ActionGroupId,
                        principalTable: "ActionGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PinCodes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Pin = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false),
                    ActionGroupId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PinCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PinCodes_ActionGroups_ActionGroupId",
                        column: x => x.ActionGroupId,
                        principalTable: "ActionGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Actions_ActionGroupId",
                table: "Actions",
                column: "ActionGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PinCodes_ActionGroupId",
                table: "PinCodes",
                column: "ActionGroupId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Actions");

            migrationBuilder.DropTable(
                name: "PinCodes");

            migrationBuilder.DropTable(
                name: "ActionGroups");
        }
    }
}
