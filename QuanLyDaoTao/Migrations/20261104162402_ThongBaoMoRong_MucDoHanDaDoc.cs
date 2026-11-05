using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyDaoTao.Migrations
{
    /// <inheritdoc />
    public partial class ThongBaoMoRong_MucDoHanDaDoc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnhDinhKem",
                table: "ThongBaos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HetHanLuc",
                table: "ThongBaos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MucDo",
                table: "ThongBaos",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "Thường");

            migrationBuilder.CreateTable(
                name: "ThongBaoDaDocs",
                columns: table => new
                {
                    ThongBaoDaDocId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ThongBaoId = table.Column<int>(type: "INTEGER", nullable: false),
                    TenDangNhap = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DocLuc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongBaoDaDocs", x => x.ThongBaoDaDocId);
                    table.ForeignKey(
                        name: "FK_ThongBaoDaDocs_ThongBaos_ThongBaoId",
                        column: x => x.ThongBaoId,
                        principalTable: "ThongBaos",
                        principalColumn: "ThongBaoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ThongBaoDaDocs_TenDangNhap",
                table: "ThongBaoDaDocs",
                column: "TenDangNhap");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBaoDaDocs_ThongBaoId_TenDangNhap",
                table: "ThongBaoDaDocs",
                columns: new[] { "ThongBaoId", "TenDangNhap" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ThongBaoDaDocs");

            migrationBuilder.DropColumn(
                name: "AnhDinhKem",
                table: "ThongBaos");

            migrationBuilder.DropColumn(
                name: "HetHanLuc",
                table: "ThongBaos");

            migrationBuilder.DropColumn(
                name: "MucDo",
                table: "ThongBaos");
        }
    }
}
