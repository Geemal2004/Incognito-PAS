using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IncognitoPAS.Migrations
{
    /// <inheritdoc />
    public partial class AddProposalDocumentUrlToProposals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProposalDocumentUrl",
                table: "Proposals",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProposalDocumentUrl",
                table: "Proposals");
        }
    }
}
