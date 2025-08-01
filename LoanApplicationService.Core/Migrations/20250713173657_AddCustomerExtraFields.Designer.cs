﻿// <auto-generated />
using System;
using LoanApplicationService.Core.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace LoanApplicationService.Core.Migrations
{
    [DbContext(typeof(LoanApplicationServiceDbContext))]
    [Migration("20250713173657_AddCustomerExtraFields")]
    partial class AddCustomerExtraFields
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("LoanApplicationService.Core.Models.Account", b =>
                {
                    b.Property<int>("AccountId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("AccountId"));

                    b.Property<string>("AccountNumber")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("AccountType")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<int>("ApplicationId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("CustomerId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("DisbursementDate")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("InterestRate")
                        .HasColumnType("decimal(5,4)");

                    b.Property<DateTime?>("MaturityDate")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("MonthlyPayment")
                        .HasColumnType("decimal(10,2)");

                    b.Property<DateTime>("NextPaymentDate")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("OutstandingBalance")
                        .HasColumnType("decimal(15,2)");

                    b.Property<decimal>("PrincipalAmount")
                        .HasColumnType("decimal(15,2)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<int>("TermMonths")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("AccountId");

                    b.HasIndex("ApplicationId")
                        .IsUnique();

                    b.HasIndex("CustomerId");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("LoanApplicationService.Core.Models.AuditTrail", b =>
                {
                    b.Property<int>("AuditId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("AuditId"));

                    b.Property<int?>("AccountId")
                        .HasColumnType("int");

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int?>("ApplicationId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("CustomerId")
                        .HasColumnType("int");

                    b.Property<int>("EntityId")
                        .HasColumnType("int");

                    b.Property<string>("EntityType")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("IpAddress")
                        .HasMaxLength(45)
                        .HasColumnType("nvarchar(45)");

                    b.Property<string>("NewValues")
                        .HasMaxLength(2000)
                        .HasColumnType("nvarchar(2000)");

                    b.Property<string>("OldValues")
                        .HasMaxLength(2000)
                        .HasColumnType("nvarchar(2000)");

                    b.Property<string>("UserAgent")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<Guid?>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("AuditId");

                    b.HasIndex("AccountId");

                    b.HasIndex("ApplicationId");

                    b.HasIndex("CustomerId");

                    b.HasIndex("UserId");

                    b.ToTable("AuditTrail");
                });

            modelBuilder.Entity("LoanApplicationService.Core.Models.Customer", b =>
                {
                    b.Property<int>("CustomerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CustomerId"));

                    b.Property<string>("Address")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal?>("AnnualIncome")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateOfBirth")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EmploymentStatus")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsDeleted")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(false);

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NationalId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("CustomerId");

                    b.HasIndex("UserId");

                    b.ToTable("Customers");
                });

            modelBuilder.Entity("LoanApplicationService.Core.Models.LoanApplication", b =>
                {
                    b.Property<int>("ApplicationId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ApplicationId"));

                    b.Property<DateTime>("ApplicationDate")
                        .HasColumnType("datetime2");

                    b.Property<decimal?>("ApprovedAmount")
                        .HasColumnType("decimal(15,2)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("CustomerId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("DecisionDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("DecisionNotes")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<decimal?>("InterestRate")
                        .HasColumnType("decimal(5,4)");

                    b.Property<Guid?>("ProcessedBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.Property<string>("Purpose")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<decimal>("RequestedAmount")
                        .HasColumnType("decimal(15,2)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<int>("TermMonths")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("ApplicationId");

                    b.HasIndex("CustomerId");

                    b.HasIndex("ProcessedBy");

                    b.HasIndex("ProductId");

                    b.ToTable("LoanApplications");
                });

            modelBuilder.Entity("LoanApplicationService.Core.Models.LoanCharge", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsDeleted")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(false);

                    b.Property<bool>("IsPenalty")
                        .HasColumnType("bit");

                    b.Property<bool>("IsUpfront")
                        .HasColumnType("bit");

                    b.Property<int?>("LoanProductProductId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("LoanProductProductId");

                    b.ToTable("LoanCharges");
                });

            modelBuilder.Entity("LoanApplicationService.Core.Models.LoanChargeMapper", b =>
                {
                    b.Property<int>("LoanChargeId")
                        .HasColumnType("int");

                    b.Property<int>("LoanProductId")
                        .HasColumnType("int");

                    b.HasKey("LoanChargeId", "LoanProductId");

                    b.HasIndex("LoanProductId");

                    b.ToTable("LoanChargeMapper");
                });

            modelBuilder.Entity("LoanApplicationService.Core.Models.LoanProduct", b =>
                {
                    b.Property<int>("ProductId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ProductId"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DeletedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("EligibilityCriteria")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<decimal>("InterestRate")
                        .HasColumnType("decimal(5,4)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<bool>("IsDeleted")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(false);

                    b.Property<int>("LoanProductType")
                        .HasColumnType("int");

                    b.Property<decimal>("MaxAmount")
                        .HasColumnType("decimal(15,2)");

                    b.Property<int>("MaxTermMonths")
                        .HasColumnType("int");

                    b.Property<decimal>("MinAmount")
                        .HasColumnType("decimal(15,2)");

                    b.Property<int>("MinTermMonths")
                        .HasColumnType("int");

                    b.Property<int>("PaymentFrequency")
                        .HasColumnType("int");

                    b.Property<decimal>("ProcessingFee")
                        .HasColumnType("decimal(10,2)");

                    b.Property<string>("ProductName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("ProductId");

                    b.ToTable("LoanProducts");
                });

            modelBuilder.Entity("LoanApplicationService.Core.Models.Notification", b =>
                {
                    b.Property<int>("NotificationId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("NotificationId"));

                    b.Property<int?>("AccountId")
                        .HasColumnType("int");

                    b.Property<string>("Channel")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("CustomerId")
                        .HasColumnType("int");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsRead")
                        .HasColumnType("bit");

                    b.Property<int?>("LoanApplicationApplicationId")
                        .HasColumnType("int");

                    b.Property<string>("Message")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NotificationHeader")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ReadAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Recipient")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("SentAt")
                        .HasColumnType("datetime2");

                    b.Property<bool>("Success")
                        .HasColumnType("bit");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("NotificationId");

                    b.HasIndex("AccountId");

                    b.HasIndex("LoanApplicationApplicationId");

                    b.ToTable("Notifications");
                });

            modelBuilder.Entity("LoanApplicationService.Core.Models.NotificationTemplate", b =>
                {
                    b.Property<int>("TemplateId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("TemplateId"));

                    b.Property<string>("BodyText")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Channel")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("NotificationHeader")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Subject")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("TemplateId");

                    b.ToTable("NotificationTemplates");
                });

            modelBuilder.Entity("LoanApplicationService.Core.Models.Users", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("LoanApplicationService.Core.Models.Account", b =>
                {
                    b.HasOne("LoanApplicationService.Core.Models.LoanApplication", "LoanApplication")
                        .WithOne("Account")
                        .HasForeignKey("LoanApplicationService.Core.Models.Account", "ApplicationId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("LoanApplicationService.Core.Models.Customer", "Customer")
                        .WithMany("Accounts")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Customer");

                    b.Navigation("LoanApplication");
                });

            modelBuilder.Entity("LoanApplicationService.Core.Models.AuditTrail", b =>
                {
                    b.HasOne("LoanApplicationService.Core.Models.Account", "Account")
                        .WithMany("AuditTrails")
                        .HasForeignKey("AccountId");

                    b.HasOne("LoanApplicationService.Core.Models.LoanApplication", "LoanApplication")
                        .WithMany("AuditTrails")
                        .HasForeignKey("ApplicationId");

                    b.HasOne("LoanApplicationService.Core.Models.Customer", "Customer")
                        .WithMany()
                        .HasForeignKey("CustomerId");

                    b.HasOne("LoanApplicationService.Core.Models.Users", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("Account");

                    b.Navigation("Customer");

                    b.Navigation("LoanApplication");

                    b.Navigation("User");
                });

            modelBuilder.Entity("LoanApplicationService.Core.Models.Customer", b =>
                {
                    b.HasOne("LoanApplicationService.Core.Models.Users", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("LoanApplicationService.Core.Models.LoanApplication", b =>
                {
                    b.HasOne("LoanApplicationService.Core.Models.Customer", "Customer")
                        .WithMany("LoanApplications")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("LoanApplicationService.Core.Models.Users", "ProcessedByUser")
                        .WithMany()
                        .HasForeignKey("ProcessedBy");

                    b.HasOne("LoanApplicationService.Core.Models.LoanProduct", "LoanProduct")
                        .WithMany("LoanApplications")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Customer");

                    b.Navigation("LoanProduct");

                    b.Navigation("ProcessedByUser");
                });

            modelBuilder.Entity("LoanApplicationService.Core.Models.LoanCharge", b =>
                {
                    b.HasOne("LoanApplicationService.Core.Models.LoanProduct", null)
                        .WithMany("LoanCharges")
                        .HasForeignKey("LoanProductProductId");
                });

            modelBuilder.Entity("LoanApplicationService.Core.Models.LoanChargeMapper", b =>
                {
                    b.HasOne("LoanApplicationService.Core.Models.LoanCharge", "LoanCharge")
                        .WithMany("LoanChargeMap")
                        .HasForeignKey("LoanChargeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("LoanApplicationService.Core.Models.LoanProduct", "LoanProduct")
                        .WithMany("LoanChargeMap")
                        .HasForeignKey("LoanProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("LoanCharge");

                    b.Navigation("LoanProduct");
                });

            modelBuilder.Entity("LoanApplicationService.Core.Models.Notification", b =>
                {
                    b.HasOne("LoanApplicationService.Core.Models.Account", null)
                        .WithMany("Notifications")
                        .HasForeignKey("AccountId");

                    b.HasOne("LoanApplicationService.Core.Models.LoanApplication", null)
                        .WithMany("Notifications")
                        .HasForeignKey("LoanApplicationApplicationId");
                });

            modelBuilder.Entity("LoanApplicationService.Core.Models.Account", b =>
                {
                    b.Navigation("AuditTrails");

                    b.Navigation("Notifications");
                });

            modelBuilder.Entity("LoanApplicationService.Core.Models.Customer", b =>
                {
                    b.Navigation("Accounts");

                    b.Navigation("LoanApplications");
                });

            modelBuilder.Entity("LoanApplicationService.Core.Models.LoanApplication", b =>
                {
                    b.Navigation("Account");

                    b.Navigation("AuditTrails");

                    b.Navigation("Notifications");
                });

            modelBuilder.Entity("LoanApplicationService.Core.Models.LoanCharge", b =>
                {
                    b.Navigation("LoanChargeMap");
                });

            modelBuilder.Entity("LoanApplicationService.Core.Models.LoanProduct", b =>
                {
                    b.Navigation("LoanApplications");

                    b.Navigation("LoanChargeMap");

                    b.Navigation("LoanCharges");
                });
#pragma warning restore 612, 618
        }
    }
}
