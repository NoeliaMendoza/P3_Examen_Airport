using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirportApp.Data.MigrationsIdentity
{
    /// <inheritdoc />
    public partial class IndiceUnicoTransaccionPago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Payments_Gateway_ExternalTransactionId",
                table: "Payments",
                columns: new[] { "Gateway", "ExternalTransactionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_Gateway_ExternalTransactionId",
                table: "Payments");
        }
    }
}
