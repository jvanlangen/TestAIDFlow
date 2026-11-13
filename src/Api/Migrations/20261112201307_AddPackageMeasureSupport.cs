using Api;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261112201307_AddPackageMeasureSupport")]
public partial class AddPackageMeasureSupport : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Unit",
            table: "PackageSizes",
            type: "character varying(12)",
            maxLength: 12,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(12)",
            oldMaxLength: 12);

        migrationBuilder.AddColumn<Guid>(
            name: "RecipeMeasureId",
            table: "PackageSizes",
            type: "uuid",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_PackageSizes_RecipeMeasureId",
            table: "PackageSizes",
            column: "RecipeMeasureId");

        migrationBuilder.AddForeignKey(
            name: "FK_PackageSizes_RecipeMeasures_RecipeMeasureId",
            table: "PackageSizes",
            column: "RecipeMeasureId",
            principalTable: "RecipeMeasures",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_PackageSizes_RecipeMeasures_RecipeMeasureId",
            table: "PackageSizes");

        migrationBuilder.DropIndex(
            name: "IX_PackageSizes_RecipeMeasureId",
            table: "PackageSizes");

        migrationBuilder.DropColumn(
            name: "RecipeMeasureId",
            table: "PackageSizes");

        migrationBuilder.AlterColumn<string>(
            name: "Unit",
            table: "PackageSizes",
            type: "character varying(12)",
            maxLength: 12,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(12)",
            oldMaxLength: 12,
            oldNullable: true);
    }
}
