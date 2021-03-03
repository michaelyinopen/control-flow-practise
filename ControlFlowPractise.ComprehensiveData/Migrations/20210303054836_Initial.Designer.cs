﻿// <auto-generated />
using System;
using ControlFlowPractise.ComprehensiveData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ControlFlowPractise.ComprehensiveData.Migrations
{
    [DbContext(typeof(ComprehensiveDataDbContext))]
    [Migration("20210303054836_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
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
                        .HasColumnType("datetime2");

                    b.Property<string>("FailureMessage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FailureType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Operation")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OrderId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("RequestId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool?>("ResponseHasNoError")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.ToTable("WarrantyCaseVerification");
                });
#pragma warning restore 612, 618
        }
    }
}
