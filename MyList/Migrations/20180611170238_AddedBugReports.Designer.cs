﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using MyList.Models.Data;
using System;

namespace MyList.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20180611170238_AddedBugReports")]
    partial class AddedBugReports
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("MyList.Models.ApplicationUser", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("FullName")
                        .IsRequired();

                    b.Property<string>("PasswordHash")
                        .IsRequired();

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.HasKey("ID");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("MyList.Models.Bug", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Date");

                    b.Property<string>("Text")
                        .IsRequired();

                    b.Property<string>("Title")
                        .IsRequired();

                    b.Property<int>("UserID");

                    b.HasKey("ID");

                    b.ToTable("BugReports");
                });
#pragma warning restore 612, 618
        }
    }
}
