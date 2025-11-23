using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElSentidoDelOido.Datos.Migrations
{
    /// <inheritdoc />
    public partial class ChangeShiftStateIdToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShiftState",
                table: "Shifts");

            migrationBuilder.AlterColumn<string>(
                name: "ShiftStateId",
                table: "Shifts",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ShiftStateId",
                table: "Shifts",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldUnicode: false,
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShiftState",
                table: "Shifts",
                type: "int",
                nullable: true);
        }
    }
}
