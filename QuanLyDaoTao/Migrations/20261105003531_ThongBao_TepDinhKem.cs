using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyDaoTao.Migrations
{
    /// <inheritdoc />
    public partial class ThongBao_TepDinhKem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoQuanBanHanh",
                table: "ThongBaos",
                type: "TEXT",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LoaiVanBan",
                table: "ThongBaos",
                type: "TEXT",
                maxLength: 30,
                nullable: false,
                defaultValue: "Thông báo");

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayBanHanh",
                table: "ThongBaos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NguoiKy",
                table: "ThongBaos",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SoKyHieu",
                table: "ThongBaos",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrichYeu",
                table: "ThongBaos",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TepDinhKemThongBaos",
                columns: table => new
                {
                    TepDinhKemThongBaoId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ThongBaoId = table.Column<int>(type: "INTEGER", nullable: false),
                    TenTep = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    KieuNoiDung = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    KichThuoc = table.Column<long>(type: "INTEGER", nullable: false),
                    DuLieu = table.Column<byte[]>(type: "BLOB", nullable: false),
                    NgayTaiLen = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TepDinhKemThongBaos", x => x.TepDinhKemThongBaoId);
                    table.ForeignKey(
                        name: "FK_TepDinhKemThongBaos_ThongBaos_ThongBaoId",
                        column: x => x.ThongBaoId,
                        principalTable: "ThongBaos",
                        principalColumn: "ThongBaoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ThongBaos_LoaiVanBan",
                table: "ThongBaos",
                column: "LoaiVanBan");

            migrationBuilder.CreateIndex(
                name: "IX_TepDinhKemThongBaos_ThongBaoId",
                table: "TepDinhKemThongBaos",
                column: "ThongBaoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TepDinhKemThongBaos");

            migrationBuilder.DropIndex(
                name: "IX_ThongBaos_LoaiVanBan",
                table: "ThongBaos");

            migrationBuilder.DropColumn(
                name: "CoQuanBanHanh",
                table: "ThongBaos");

            migrationBuilder.DropColumn(
                name: "LoaiVanBan",
                table: "ThongBaos");

            migrationBuilder.DropColumn(
                name: "NgayBanHanh",
                table: "ThongBaos");

            migrationBuilder.DropColumn(
                name: "NguoiKy",
                table: "ThongBaos");

            migrationBuilder.DropColumn(
                name: "SoKyHieu",
                table: "ThongBaos");

            migrationBuilder.DropColumn(
                name: "TrichYeu",
                table: "ThongBaos");
        }
    }
}
