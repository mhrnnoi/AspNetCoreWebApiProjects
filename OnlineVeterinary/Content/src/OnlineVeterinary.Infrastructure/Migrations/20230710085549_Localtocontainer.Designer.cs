﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using OnlineVeterinary.Infrastructure.Persistence.DataContext;

#nullable disable

namespace OnlineVeterinary.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20230710085549_Localtocontainer")]
    partial class Localtocontainer
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("OnlineVeterinary.Domain.Pet.Entities.Pet", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("Id");

                    b.Property<Guid>("CareGiverId")
                        .HasColumnType("uuid")
                        .HasColumnName("CareGiverId");

                    b.Property<string>("CareGiverLastName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("CareGiverLastName");

                    b.Property<DateTime>("DateOfBirth")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("DateOfBirth");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("Name");

                    b.Property<string>("PetType")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("PetType");

                    b.HasKey("Id");

                    b.ToTable("Pets");
                });

            modelBuilder.Entity("OnlineVeterinary.Domain.Reservation.Entities.Reservation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("Id");

                    b.Property<Guid>("CareGiverId")
                        .HasColumnType("uuid")
                        .HasColumnName("CareGiverId");

                    b.Property<string>("CareGiverLastName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("CareGiverLastName");

                    b.Property<DateTime>("DateOfReservation")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("DateOfReservation");

                    b.Property<Guid>("DoctorId")
                        .HasColumnType("uuid")
                        .HasColumnName("DoctorId");

                    b.Property<string>("DrLastName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("DrLastName");

                    b.Property<Guid>("PetId")
                        .HasColumnType("uuid")
                        .HasColumnName("PetId");

                    b.Property<string>("PetName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("PetName");

                    b.HasKey("Id");

                    b.ToTable("Reservations");
                });

            modelBuilder.Entity("OnlineVeterinary.Domain.Users.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("Id");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("Email");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("FirstName");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("LastName");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("Password");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("Role");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });
#pragma warning restore 612, 618
        }
    }
}
