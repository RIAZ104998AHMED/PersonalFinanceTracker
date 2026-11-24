using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalFinanceTracker.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddProject2Domain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RecurringOperationId",
                table: "Operations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    StorageKey = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attachments_Operations_OperationId",
                        column: x => x.OperationId,
                        principalTable: "Operations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExchangeRates",
                columns: table => new
                {
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeRates", x => new { x.Date, x.Currency });
                });

            migrationBuilder.CreateTable(
                name: "RecurringOperations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Frequency = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    NextRunDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurringOperations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecurringOperations_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecurringOperations_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Operations_RecurringOperationId",
                table: "Operations",
                column: "RecurringOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_RecurringOperationId_Date",
                table: "Operations",
                columns: new[] { "RecurringOperationId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_OperationId",
                table: "Attachments",
                column: "OperationId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_Date_Currency",
                table: "ExchangeRates",
                columns: new[] { "Date", "Currency" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecurringOperations_AccountId",
                table: "RecurringOperations",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringOperations_CategoryId",
                table: "RecurringOperations",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Operations_RecurringOperations_RecurringOperationId",
                table: "Operations",
                column: "RecurringOperationId",
                principalTable: "RecurringOperations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Operations_RecurringOperations_RecurringOperationId",
                table: "Operations");

            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropTable(
                name: "ExchangeRates");

            migrationBuilder.DropTable(
                name: "RecurringOperations");

            migrationBuilder.DropIndex(
                name: "IX_Operations_RecurringOperationId",
                table: "Operations");

            migrationBuilder.DropIndex(
                name: "IX_Operations_RecurringOperationId_Date",
                table: "Operations");

            migrationBuilder.DropColumn(
                name: "RecurringOperationId",
                table: "Operations");
        }
    }
}
