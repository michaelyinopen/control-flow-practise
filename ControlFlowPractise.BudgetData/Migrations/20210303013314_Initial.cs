using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ControlFlowPractise.BudgetData.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExternalPartyRequest",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateTime = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    OrderId = table.Column<string>(nullable: false),
                    Operation = table.Column<string>(nullable: false),
                    RequestId = table.Column<Guid>(nullable: false),
                    Request = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalPartyRequest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExternalPartyResponse",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateTime = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    OrderId = table.Column<string>(nullable: false),
                    Operation = table.Column<string>(nullable: false),
                    RequestId = table.Column<Guid>(nullable: false),
                    Response = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalPartyResponse", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalPartyRequest_OrderId_RequestId",
                table: "ExternalPartyRequest",
                columns: new[] { "OrderId", "RequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalPartyResponse_OrderId_RequestId",
                table: "ExternalPartyResponse",
                columns: new[] { "OrderId", "RequestId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalPartyRequest");

            migrationBuilder.DropTable(
                name: "ExternalPartyResponse");
        }
    }
}
