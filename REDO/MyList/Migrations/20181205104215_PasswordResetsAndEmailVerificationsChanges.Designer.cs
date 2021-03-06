﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MyList.Models.Data;

namespace MyList.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20181205104215_PasswordResetsAndEmailVerificationsChanges")]
    partial class PasswordResetsAndEmailVerificationsChanges
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.2-rtm-30932")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("MyList.Models.ApplicationUser", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Email");

                    b.Property<bool>("EmailVerified");

                    b.Property<string>("FirstName");

                    b.Property<string>("LastName");

                    b.Property<string>("PasswordHash");

                    b.Property<DateTime>("RegistrationDatetime");

                    b.HasKey("ID");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("MyList.Models.EmailVerification", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("RandomString");

                    b.Property<DateTime>("Sent");

                    b.Property<int>("UserID");

                    b.Property<bool>("Verified");

                    b.Property<DateTime>("VerifiedOn");

                    b.HasKey("ID");

                    b.ToTable("EmailVerifications");
                });

            modelBuilder.Entity("MyList.Models.PasswordReset", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("RandomString");

                    b.Property<DateTime>("Sent");

                    b.Property<int>("UserID");

                    b.HasKey("ID");

                    b.ToTable("PasswordResets");
                });
#pragma warning restore 612, 618
        }
    }
}
