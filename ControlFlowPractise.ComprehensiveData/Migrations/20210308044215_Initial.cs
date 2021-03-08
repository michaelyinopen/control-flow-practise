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
                    DateTime = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    OrderId = table.Column<string>(nullable: false),
                    WarrantyCaseId = table.Column<string>(nullable: true),
                    Operation = table.Column<string>(nullable: false),
                    WarrantyCaseStatus = table.Column<string>(nullable: true),
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

            migrationBuilder.CreateTable(
                name: "WarrantyProof",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateTime = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    OrderId = table.Column<string>(nullable: false),
                    WarrantyCaseId = table.Column<string>(nullable: false),
                    RequestId = table.Column<Guid>(nullable: false),
                    Proof = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarrantyProof", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyCaseVerification_OrderId_ResponseHasNoError_FailureType_DateTime",
                table: "WarrantyCaseVerification",
                columns: new[] { "OrderId", "ResponseHasNoError", "FailureType", "DateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyCaseVerification_OrderId_ResponseHasNoError_FailureType_Operation_WarrantyCaseStatus_DateTime",
                table: "WarrantyCaseVerification",
                columns: new[] { "OrderId", "ResponseHasNoError", "FailureType", "Operation", "WarrantyCaseStatus", "DateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyProof_RequestId",
                table: "WarrantyProof",
                column: "RequestId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WarrantyCaseVerification");

            migrationBuilder.DropTable(
                name: "WarrantyProof");
        }
    }
}
