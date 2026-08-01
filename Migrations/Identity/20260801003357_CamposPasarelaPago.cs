using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirportApp.Migrations.Identity
{
    /// <inheritdoc />
    public partial class CamposPasarelaPago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalUrl",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CaptureId",
                table: "Payments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalUrl",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CaptureId",
                table: "Payments");
        }
    }
}
