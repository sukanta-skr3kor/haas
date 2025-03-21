﻿// <auto-generated />

#nullable disable

#nullable disable

namespace HaasConnectorAPIs.Migrations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.Migrations;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
    using System;

    /// <summary>
    /// Defines the <see cref="InitialCreate" />
    /// </summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20250318154019_InitialCreate")]
    internal partial class InitialCreate
    {
        /// <inheritdoc />

        /// <summary>
        /// The BuildTargetModel
        /// </summary>
        /// <param name="modelBuilder">The modelBuilder<see cref="ModelBuilder"/></param>
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.3");

            modelBuilder.Entity("HaasConnectorService.Models.HaasDB", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ParamaterName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("time")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("HaasData");
                });
        }
    }
}
