using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ControlFlowPractise.ComprehensiveData.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WarrantyCaseVerification",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateTime = table.Column<DateTime>(nullable: false),
                    OrderId = table.Column<string>(nullable: false),
                    Operation = table.Column<string>(nullable: false),
                    RequestId = table.Column<Guid>(nullable: false),
                    CalledExternalParty = table.Column<bool>(nullable: false),
                    CalledWithResponse = table.Column<bool>(nullable: true),
                    ResponseHasNoError = table.Column<bool>(nullable: true),
                    FailureType = table.Column<string>(nullable: true),
                    FailureMessage = table.Column<string>(nullable: true),
                    ConvertedResponse = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarrantyCaseVerification", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WarrantyCaseVerification");
        }
    }
}
