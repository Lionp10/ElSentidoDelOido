using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElSentidoDelOido.Datos.Migrations
{
    /// <inheritdoc />
    public partial class RemoveHolidaysShiftTypeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Holidays_ShiftTypes_ShiftTypeId",
                table: "Holidays");

            migrationBuilder.DropIndex(
                name: "IX_Holidays_ShiftTypeId",
                table: "Holidays");

            migrationBuilder.DropColumn(
                name: "ShiftTypeId",
                table: "Holidays");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ShiftTypeId",
                table: "Holidays",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Holidays_ShiftTypeId",
                table: "Holidays",
                column: "ShiftTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Holidays_ShiftTypes_ShiftTypeId",
                table: "Holidays",
                column: "ShiftTypeId",
                principalTable: "ShiftTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
