using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore;
using ShortLink.Api.Data;

namespace ShortLink.Api.Data.Migrations;

[DbContext(typeof(ShortLinkDbContext))]
[Migration("20261115100000_InitialCreate")]
public sealed class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Links",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy",
                        Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Code = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                LongUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Links", column => column.Id));

        migrationBuilder.CreateIndex(
            name: "IX_Links_Code",
            table: "Links",
            column: "Code",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "Links");

    protected override void BuildTargetModel(ModelBuilder modelBuilder)
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
