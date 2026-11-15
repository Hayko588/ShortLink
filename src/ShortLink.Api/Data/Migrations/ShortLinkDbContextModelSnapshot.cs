using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShortLink.Api.Data;

namespace ShortLink.Api.Data.Migrations;

[DbContext(typeof(ShortLinkDbContext))]
public sealed class ShortLinkDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0");
        modelBuilder.HasAnnotation("Relational:MaxIdentifierLength", 63);
        modelBuilder.HasAnnotation(
            "Npgsql:ValueGenerationStrategy",
            Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

        modelBuilder.Entity("ShortLink.Api.Models.Link", entity =>
        {
            entity.Property<long>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("bigint")
                .HasAnnotation(
                    "Npgsql:ValueGenerationStrategy",
                    Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
            entity.Property<string>("Code").IsRequired().HasMaxLength(7).HasColumnType("character varying(7)");
            entity.Property<DateTimeOffset>("CreatedAt").HasColumnType("timestamp with time zone");
            entity.Property<string>("LongUrl").IsRequired().HasMaxLength(2048).HasColumnType("character varying(2048)");
            entity.HasKey("Id");
            entity.HasIndex("Code").IsUnique();
            entity.ToTable("Links");
        });
    }
}
