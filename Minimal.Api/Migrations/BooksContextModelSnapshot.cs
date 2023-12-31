﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Minimal.Api.DbContext;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Minimal.Api.Migrations
{
    [DbContext(typeof(BooksContext))]
    partial class BooksContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Minimal.Api.Entities.Book", b =>
                {
                    b.Property<string>("Isbn")
                        .HasColumnType("text");

                    b.Property<string>("Author")
                        .HasColumnType("text");

                    b.Property<int?>("PageCount")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("ReleaseDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ShortDescription")
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Isbn");

                    b.ToTable("Books");
                });
#pragma warning restore 612, 618
        }
    }
}
