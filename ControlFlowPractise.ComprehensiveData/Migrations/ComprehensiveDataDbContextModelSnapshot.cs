﻿// <auto-generated />
using System;
using ControlFlowPractise.ComprehensiveData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ControlFlowPractise.ComprehensiveData.Migrations
{
    [DbContext(typeof(ComprehensiveDataDbContext))]
    partial class ComprehensiveDataDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.12")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ControlFlowPractise.ComprehensiveData.Models.WarrantyCaseVerification", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("CalledExternalParty")
                        .HasColumnType("bit");

                    b.Property<bool?>("CalledWithResponse")
                        .HasColumnType("bit");

                    b.Property<string>("ConvertedResponse")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("DateTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.Property<string>("FailureMessage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FailureType")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Operation")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("OrderId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<Guid>("RequestId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool?>("ResponseHasNoError")
                        .HasColumnType("bit");

                    b.Property<string>("WarrantyCaseId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("WarrantyCaseStatus")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("OrderId", "ResponseHasNoError", "FailureType", "DateTime");

                    b.HasIndex("OrderId", "ResponseHasNoError", "FailureType", "Operation", "WarrantyCaseStatus", "DateTime");

                    b.ToTable("WarrantyCaseVerification");
                });

            modelBuilder.Entity("ControlFlowPractise.ComprehensiveData.Models.WarrantyProof", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("DateTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.Property<string>("OrderId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Proof")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("RequestId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("WarrantyCaseId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("RequestId")
                        .IsUnique();

                    b.ToTable("WarrantyProof");
                });
#pragma warning restore 612, 618
        }
    }
}
