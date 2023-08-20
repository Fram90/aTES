using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace aTES.Accounting.Db.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    popugpublicid = table.Column<Guid>(name: "popug_public_id", type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_accounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "billing_cycles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    startdate = table.Column<DateTimeOffset>(name: "start_date", type: "timestamp with time zone", nullable: false),
                    enddate = table.Column<DateTimeOffset>(name: "end_date", type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_billing_cycles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tasks",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    publicid = table.Column<Guid>(name: "public_id", type: "uuid", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    popugpublicid = table.Column<Guid>(name: "popug_public_id", type: "uuid", nullable: false),
                    chargeprice = table.Column<decimal>(name: "charge_price", type: "numeric", nullable: false),
                    paymentprice = table.Column<decimal>(name: "payment_price", type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tasks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    publicid = table.Column<Guid>(name: "public_id", type: "uuid", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_log_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    description = table.Column<string>(type: "text", nullable: false),
                    balancedelta = table.Column<decimal>(name: "balance_delta", type: "numeric", nullable: true),
                    issued = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    accountid = table.Column<int>(name: "account_id", type: "integer", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "popug_transaction",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    accountid = table.Column<int>(name: "account_id", type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    creditvalue = table.Column<decimal>(name: "credit_value", type: "numeric", nullable: false),
                    debitvalue = table.Column<decimal>(name: "debit_value", type: "numeric", nullable: false),
                    billingcycleid = table.Column<int>(name: "billing_cycle_id", type: "integer", nullable: false),
                    issued = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    owneraccountid = table.Column<int>(name: "owner_account_id", type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_popug_transaction", x => x.id);
                    table.ForeignKey(
                        name: "fk_popug_transaction_accounts_account_id",
                        column: x => x.accountid,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_popug_transaction_accounts_owner_account_id",
                        column: x => x.owneraccountid,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_popug_transaction_billing_cycles_billing_cycle_id",
                        column: x => x.billingcycleid,
                        principalTable: "billing_cycles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_items_account_id",
                table: "audit_log_items",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_popug_transaction_account_id",
                table: "popug_transaction",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_popug_transaction_billing_cycle_id",
                table: "popug_transaction",
                column: "billing_cycle_id");

            migrationBuilder.CreateIndex(
                name: "ix_popug_transaction_owner_account_id",
                table: "popug_transaction",
                column: "owner_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_tasks_public_id",
                table: "tasks",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_public_id",
                table: "users",
                column: "public_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_log_items");

            migrationBuilder.DropTable(
                name: "popug_transaction");

            migrationBuilder.DropTable(
                name: "tasks");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "accounts");

            migrationBuilder.DropTable(
                name: "billing_cycles");
        }
    }
}
