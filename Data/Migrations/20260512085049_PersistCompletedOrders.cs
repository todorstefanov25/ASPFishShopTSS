using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FishShopASP.Data.Migrations
{
    /// <inheritdoc />
    public partial class PersistCompletedOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "OrderItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OrderNumber",
                table: "OrderItems",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "OrderNumber",
                table: "OrderItems");
        }
    }
}
