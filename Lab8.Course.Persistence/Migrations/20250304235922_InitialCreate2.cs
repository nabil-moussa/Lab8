using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lab8.Course.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.RenameTable(
                name: "Courses",
                newName: "Courses",
                newSchema: "public");

            migrationBuilder.AddColumn<string>(
                name: "SchemaName",
                schema: "public",
                table: "Courses",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SchemaName",
                schema: "public",
                table: "Courses");

            migrationBuilder.RenameTable(
                name: "Courses",
                schema: "public",
                newName: "Courses");
        }
    }
}
