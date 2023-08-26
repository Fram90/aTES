using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace aTES.Accounting.Db.Migrations
{
    /// <inheritdoc />
    public partial class Outbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_popug_transaction_accounts_account_id",
                table: "popug_transaction");

            migrationBuilder.DropForeignKey(
                name: "fk_popug_transaction_accounts_owner_account_id",
                table: "popug_transaction");

            migrationBuilder.DropForeignKey(
                name: "fk_popug_transaction_billing_cycles_billing_cycle_id",
                table: "popug_transaction");

            migrationBuilder.DropTable(
                name: "audit_log_items");

            migrationBuilder.DropPrimaryKey(
                name: "pk_popug_transaction",
                table: "popug_transaction");

            migrationBuilder.DropIndex(
                name: "ix_popug_transaction_owner_account_id",
                table: "popug_transaction");

            migrationBuilder.DropColumn(
                name: "owner_account_id",
                table: "popug_transaction");

            migrationBuilder.RenameTable(
                name: "popug_transaction",
                newName: "transactions");

            migrationBuilder.RenameIndex(
                name: "ix_popug_transaction_billing_cycle_id",
                table: "transactions",
                newName: "ix_transactions_billing_cycle_id");

            migrationBuilder.RenameIndex(
                name: "ix_popug_transaction_account_id",
                table: "transactions",
                newName: "ix_transactions_account_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_transactions",
                table: "transactions",
                column: "id");

            migrationBuilder.CreateTable(
                name: "outbox_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    topic = table.Column<string>(type: "text", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    added = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_items", x => x.id);
                });

            migrationBuilder.AddForeignKey(
                name: "fk_transactions_accounts_account_id",
                table: "transactions",
                column: "account_id",
                principalTable: "accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_transactions_billing_cycles_billing_cycle_id",
                table: "transactions",
                column: "billing_cycle_id",
                principalTable: "billing_cycles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_transactions_accounts_account_id",
                table: "transactions");

            migrationBuilder.DropForeignKey(
                name: "fk_transactions_billing_cycles_billing_cycle_id",
                table: "transactions");

            migrationBuilder.DropTable(
                name: "outbox_items");

            migrationBuilder.DropPrimaryKey(
                name: "pk_transactions",
                table: "transactions");

            migrationBuilder.RenameTable(
                name: "transactions",
                newName: "popug_transaction");

            migrationBuilder.RenameIndex(
                name: "ix_transactions_billing_cycle_id",
                table: "popug_transaction",
                newName: "ix_popug_transaction_billing_cycle_id");

            migrationBuilder.RenameIndex(
                name: "ix_transactions_account_id",
                table: "popug_transaction",
                newName: "ix_popug_transaction_account_id");

            migrationBuilder.AddColumn<int>(
                name: "owner_account_id",
                table: "popug_transaction",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "pk_popug_transaction",
                table: "popug_transaction",
                column: "id");

            migrationBuilder.CreateTable(
                name: "audit_log_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    accountid = table.Column<int>(name: "account_id", type: "integer", nullable: false),
                    balancedelta = table.Column<decimal>(name: "balance_delta", type: "numeric", nullable: true),
                    description = table.Column<string>(type: "text", nullable: false),
                    issued = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_log_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_audit_log_items_accounts_account_id",
                        column: x => x.accountid,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_popug_transaction_owner_account_id",
                table: "popug_transaction",
                column: "owner_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_items_account_id",
                table: "audit_log_items",
                column: "account_id");

            migrationBuilder.AddForeignKey(
                name: "fk_popug_transaction_accounts_account_id",
                table: "popug_transaction",
                column: "account_id",
                principalTable: "accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_popug_transaction_accounts_owner_account_id",
                table: "popug_transaction",
                column: "owner_account_id",
                principalTable: "accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_popug_transaction_billing_cycles_billing_cycle_id",
                table: "popug_transaction",
                column: "billing_cycle_id",
                principalTable: "billing_cycles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
