﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using aTES.Accounting.Db;

#nullable disable

namespace aTES.Accounting.Db.Migrations
{
    [DbContext(typeof(AccountingDbContext))]
    [Migration("20230826102046_Outbox")]
    partial class Outbox
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("aTES.Accounting.Db.StreamedTask", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("ChargePrice")
                        .HasColumnType("numeric")
                        .HasColumnName("charge_price");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<decimal>("PaymentPrice")
                        .HasColumnType("numeric")
                        .HasColumnName("payment_price");

                    b.Property<Guid>("PopugPublicId")
                        .HasColumnType("uuid")
                        .HasColumnName("popug_public_id");

                    b.Property<Guid>("PublicId")
                        .HasColumnType("uuid")
                        .HasColumnName("public_id");

                    b.HasKey("Id")
                        .HasName("pk_tasks");

                    b.HasIndex("PublicId")
                        .IsUnique()
                        .HasDatabaseName("ix_tasks_public_id");

                    b.ToTable("tasks", (string)null);
                });

            modelBuilder.Entity("aTES.Accounting.Db.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("email");

                    b.Property<Guid>("PublicId")
                        .HasColumnType("uuid")
                        .HasColumnName("public_id");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("role");

                    b.HasKey("Id")
                        .HasName("pk_users");

                    b.HasIndex("PublicId")
                        .IsUnique()
                        .HasDatabaseName("ix_users_public_id");

                    b.ToTable("users", (string)null);
                });

            modelBuilder.Entity("aTES.Accounting.Domain.Account", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<Guid>("PopugPublicId")
                        .HasColumnType("uuid")
                        .HasColumnName("popug_public_id");

                    b.HasKey("Id")
                        .HasName("pk_accounts");

                    b.ToTable("accounts", (string)null);
                });

            modelBuilder.Entity("aTES.Accounting.Domain.BillingCycle", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset>("EndDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("end_date");

                    b.Property<DateTimeOffset>("StartDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("start_date");

                    b.Property<int>("Status")
                        .HasColumnType("integer")
                        .HasColumnName("status");

                    b.HasKey("Id")
                        .HasName("pk_billing_cycles");

                    b.ToTable("billing_cycles", (string)null);
                });

            modelBuilder.Entity("aTES.Accounting.Domain.PopugTransaction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("AccountId")
                        .HasColumnType("integer")
                        .HasColumnName("account_id");

                    b.Property<int>("BillingCycleId")
                        .HasColumnType("integer")
                        .HasColumnName("billing_cycle_id");

                    b.Property<decimal>("CreditValue")
                        .HasColumnType("numeric")
                        .HasColumnName("credit_value");

                    b.Property<decimal>("DebitValue")
                        .HasColumnType("numeric")
                        .HasColumnName("debit_value");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<DateTimeOffset>("Issued")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("issued");

                    b.Property<int>("Type")
                        .HasColumnType("integer")
                        .HasColumnName("type");

                    b.HasKey("Id")
                        .HasName("pk_transactions");

                    b.HasIndex("AccountId")
                        .HasDatabaseName("ix_transactions_account_id");

                    b.HasIndex("BillingCycleId")
                        .HasDatabaseName("ix_transactions_billing_cycle_id");

                    b.ToTable("transactions", (string)null);
                });

            modelBuilder.Entity("aTES.Common.Shared.Db.OutboxItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset>("Added")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("added");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("message");

                    b.Property<string>("Topic")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("topic");

                    b.HasKey("Id")
                        .HasName("pk_outbox_items");

                    b.ToTable("outbox_items", (string)null);
                });

            modelBuilder.Entity("aTES.Accounting.Domain.PopugTransaction", b =>
                {
                    b.HasOne("aTES.Accounting.Domain.Account", "Account")
                        .WithMany("Transactions")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_transactions_accounts_account_id");

                    b.HasOne("aTES.Accounting.Domain.BillingCycle", "BillingCycle")
                        .WithMany("Transactions")
                        .HasForeignKey("BillingCycleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_transactions_billing_cycles_billing_cycle_id");

                    b.Navigation("Account");

                    b.Navigation("BillingCycle");
                });

            modelBuilder.Entity("aTES.Accounting.Domain.Account", b =>
                {
                    b.Navigation("Transactions");
                });

            modelBuilder.Entity("aTES.Accounting.Domain.BillingCycle", b =>
                {
                    b.Navigation("Transactions");
                });
#pragma warning restore 612, 618
        }
    }
}