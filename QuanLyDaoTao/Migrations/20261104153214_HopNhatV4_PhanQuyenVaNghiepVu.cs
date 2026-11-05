using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyDaoTao.Migrations
{
    /// <inheritdoc />
    public partial class HopNhatV4_PhanQuyenVaNghiepVu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CapBacHam",
                table: "NhanSus",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HocHam",
                table: "NhanSus",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QueQuan",
                table: "NhanSus",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChuyenNganh",
                table: "LopHocs",
                type: "TEXT",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeDaoTao",
                table: "LopHocs",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "Chính quy");

            migrationBuilder.AddColumn<string>(
                name: "KhoaHoc",
                table: "LopHocs",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayCapNhat",
                table: "LopHocs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChucVuLop",
                table: "HocViens",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayCapNhat",
                table: "HocViens",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SoCCCD",
                table: "HocViens",
                type: "TEXT",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KhoaPhuTrachId",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LanDangNhapCuoi",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VungQuanLy",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ThongBaos",
                columns: table => new
                {
                    ThongBaoId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TieuDe = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    NoiDung = table.Column<string>(type: "TEXT", maxLength: 20000, nullable: false),
                    PhamVi = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    DaGhim = table.Column<bool>(type: "INTEGER", nullable: false),
                    NguoiDang = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    NgayDang = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NgaySua = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongBaos", x => x.ThongBaoId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NhanSus_SoCCCD",
                table: "NhanSus",
                column: "SoCCCD");

            migrationBuilder.CreateIndex(
                name: "IX_NhanSus_SoDienThoai",
                table: "NhanSus",
                column: "SoDienThoai");

            migrationBuilder.CreateIndex(
                name: "IX_LopHocs_HeDaoTao_KhoaHoc",
                table: "LopHocs",
                columns: new[] { "HeDaoTao", "KhoaHoc" });

            migrationBuilder.CreateIndex(
                name: "IX_HocViens_SoCCCD",
                table: "HocViens",
                column: "SoCCCD");

            migrationBuilder.CreateIndex(
                name: "IX_HocViens_SoDienThoai",
                table: "HocViens",
                column: "SoDienThoai");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBaos_NgayDang",
                table: "ThongBaos",
                column: "NgayDang");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ThongBaos");

            migrationBuilder.DropIndex(
                name: "IX_NhanSus_SoCCCD",
                table: "NhanSus");

            migrationBuilder.DropIndex(
                name: "IX_NhanSus_SoDienThoai",
                table: "NhanSus");

            migrationBuilder.DropIndex(
                name: "IX_LopHocs_HeDaoTao_KhoaHoc",
                table: "LopHocs");

            migrationBuilder.DropIndex(
                name: "IX_HocViens_SoCCCD",
                table: "HocViens");

            migrationBuilder.DropIndex(
                name: "IX_HocViens_SoDienThoai",
                table: "HocViens");

            migrationBuilder.DropColumn(
                name: "CapBacHam",
                table: "NhanSus");

            migrationBuilder.DropColumn(
                name: "HocHam",
                table: "NhanSus");

            migrationBuilder.DropColumn(
                name: "QueQuan",
                table: "NhanSus");

            migrationBuilder.DropColumn(
                name: "ChuyenNganh",
                table: "LopHocs");

            migrationBuilder.DropColumn(
                name: "HeDaoTao",
                table: "LopHocs");

            migrationBuilder.DropColumn(
                name: "KhoaHoc",
                table: "LopHocs");

            migrationBuilder.DropColumn(
                name: "NgayCapNhat",
                table: "LopHocs");

            migrationBuilder.DropColumn(
                name: "ChucVuLop",
                table: "HocViens");

            migrationBuilder.DropColumn(
                name: "NgayCapNhat",
                table: "HocViens");

            migrationBuilder.DropColumn(
                name: "SoCCCD",
                table: "HocViens");

            migrationBuilder.DropColumn(
                name: "KhoaPhuTrachId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LanDangNhapCuoi",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "VungQuanLy",
                table: "AspNetUsers");
        }
    }
}
