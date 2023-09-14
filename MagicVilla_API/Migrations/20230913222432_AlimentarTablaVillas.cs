using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MagicVilla_API.Migrations
{
    /// <inheritdoc />
    public partial class AlimentarTablaVillas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Villas",
                columns: new[] { "Id", "Amenidad", "Detalle", "FechaActualizacion", "FechaCreacion", "ImagenUrl", "MetrosCuadrados", "Nombre", "Ocupantes", "Tarifa" },
                values: new object[,]
                {
                    { 1, "", "Detalle de la villa... ", new DateTime(2023, 9, 13, 16, 24, 32, 227, DateTimeKind.Local).AddTicks(5628), new DateTime(2023, 9, 13, 16, 24, 32, 227, DateTimeKind.Local).AddTicks(5617), "", 50, "Villa Real", 5, 150.0 },
                    { 2, "", "Detalle de la villa Premium... ", new DateTime(2023, 9, 13, 16, 24, 32, 227, DateTimeKind.Local).AddTicks(5632), new DateTime(2023, 9, 13, 16, 24, 32, 227, DateTimeKind.Local).AddTicks(5632), "", 70, "Villa Premium", 6, 350.0 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Villas",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Villas",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
