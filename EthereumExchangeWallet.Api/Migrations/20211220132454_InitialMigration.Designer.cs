// <auto-generated />
using System;
using EthereumExchangeWallet.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace EthereumExchangeWallet.Api.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20211220132454_InitialMigration")]
    partial class InitialMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.12")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("EthereumExchangeWallet.Data.Entities.Asset", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Assets");
                });

            modelBuilder.Entity("EthereumExchangeWallet.Data.Entities.DepositAddress", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Address")
                        .HasColumnType("text");

                    b.Property<int?>("AssetId")
                        .HasColumnType("integer");

                    b.Property<int?>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("AssetId");

                    b.HasIndex("UserId");

                    b.ToTable("DepositAddresses");
                });

            modelBuilder.Entity("EthereumExchangeWallet.Data.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Username")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("EthereumExchangeWallet.Data.Entities.DepositAddress", b =>
                {
                    b.HasOne("EthereumExchangeWallet.Data.Entities.Asset", "Asset")
                        .WithMany()
                        .HasForeignKey("AssetId");

                    b.HasOne("EthereumExchangeWallet.Data.Entities.User", null)
                        .WithMany("DepositAddresses")
                        .HasForeignKey("UserId");

                    b.Navigation("Asset");
                });

            modelBuilder.Entity("EthereumExchangeWallet.Data.Entities.User", b =>
                {
                    b.Navigation("DepositAddresses");
                });
#pragma warning restore 612, 618
        }
    }
}
