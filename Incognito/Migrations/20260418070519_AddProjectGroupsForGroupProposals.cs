using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IncognitoPAS.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectGroupsForGroupProposals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectGroupId",
                table: "Proposals",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProjectGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LeaderId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectGroups_AspNetUsers_LeaderId",
                        column: x => x.LeaderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectGroupMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectGroupId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectGroupMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectGroupMembers_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectGroupMembers_ProjectGroups_ProjectGroupId",
                        column: x => x.ProjectGroupId,
                        principalTable: "ProjectGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_ProjectGroupId",
                table: "Proposals",
                column: "ProjectGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectGroupMembers_ProjectGroupId_StudentId",
                table: "ProjectGroupMembers",
                columns: new[] { "ProjectGroupId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectGroupMembers_StudentId",
                table: "ProjectGroupMembers",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectGroups_LeaderId",
                table: "ProjectGroups",
                column: "LeaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Proposals_ProjectGroups_ProjectGroupId",
                table: "Proposals",
                column: "ProjectGroupId",
                principalTable: "ProjectGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Proposals_ProjectGroups_ProjectGroupId",
                table: "Proposals");

            migrationBuilder.DropTable(
                name: "ProjectGroupMembers");

            migrationBuilder.DropTable(
                name: "ProjectGroups");

            migrationBuilder.DropIndex(
                name: "IX_Proposals_ProjectGroupId",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "ProjectGroupId",
                table: "Proposals");
        }
    }
}
