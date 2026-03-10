using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiInator.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSteamAndGameInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TinyImageUrl",
                table: "SteamInfo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "WorthFactor",
                table: "GameInfo",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TinyImageUrl",
                table: "SteamInfo");

            migrationBuilder.DropColumn(
                name: "WorthFactor",
                table: "GameInfo");
        }
    }
}
