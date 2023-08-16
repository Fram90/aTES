using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using aTES.TaskTracker.Domain;

#nullable disable

namespace aTES.TaskTracker.Db.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:task_state", "open,closed");

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
                    paymentprice = table.Column<decimal>(name: "payment_price", type: "numeric", nullable: false),
                    status = table.Column<TaskState>(type: "task_state", nullable: false),
                    createdat = table.Column<DateTimeOffset>(name: "created_at", type: "timestamp with time zone", nullable: false),
                    completedat = table.Column<DateTimeOffset>(name: "completed_at", type: "timestamp with time zone", nullable: true)
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
                name: "tasks");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
