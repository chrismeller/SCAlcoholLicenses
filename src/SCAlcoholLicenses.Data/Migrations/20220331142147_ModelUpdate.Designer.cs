﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SCAlcoholLicenses.Data;

#nullable disable

namespace SCAlcoholLicenses.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20220331142147_ModelUpdate")]
    partial class ModelUpdate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("SCAlcoholLicenses.Data.Models.License", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("BusinessName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("City")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CloseOrExtensionDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTimeOffset>("FirstSeen")
                        .HasColumnType("datetimeoffset");

                    b.Property<bool>("FoodProductManufacturer")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset>("LastSeen")
                        .HasColumnType("datetimeoffset");

                    b.Property<bool>("LbdWholesaler")
                        .HasColumnType("bit");

                    b.Property<string>("LegalName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LicenseNumber")
                        .HasMaxLength(25)
                        .HasColumnType("nvarchar(25)");

                    b.Property<string>("LicenseType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LocationAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("OpenDate")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("LicenseNumber", "OpenDate")
                        .IsUnique()
                        .HasFilter("[LicenseNumber] IS NOT NULL");

                    b.ToTable("Licenses");
                });
#pragma warning restore 612, 618
        }
    }
}
