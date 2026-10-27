using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyDaoTao.Migrations
{
    /// <inheritdoc />
    public partial class GopTruongThongTinTinhGon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_PhanCongGiangDays_NhanSuId_MonHocId_LopHocId_HocKy_NamHoc\";");

            migrationBuilder.DropColumn(
                name: "HocKy",
                table: "PhanCongGiangDays");

            migrationBuilder.DropColumn(
                name: "NamHoc",
                table: "PhanCongGiangDays");

            migrationBuilder.DropColumn(
                name: "NgayBatDau",
                table: "PhanCongGiangDays");

            migrationBuilder.DropColumn(
                name: "NgayKetThuc",
                table: "PhanCongGiangDays");

            migrationBuilder.DropColumn(
                name: "ChucDanhNgheNghiep",
                table: "NhanSus");

            migrationBuilder.DropColumn(
                name: "ChuyenNganh",
                table: "NhanSus");

            migrationBuilder.DropColumn(
                name: "CoHoatDong",
                table: "NhanSus");

            migrationBuilder.DropColumn(
                name: "DanToc",
                table: "NhanSus");

            migrationBuilder.DropColumn(
                name: "EmailCaNhan",
                table: "NhanSus");

            migrationBuilder.DropColumn(
                name: "GhiChuChuyenMon",
                table: "NhanSus");

            migrationBuilder.DropColumn(
                name: "HocHam",
                table: "NhanSus");

            migrationBuilder.DropColumn(
                name: "HocVi",
                table: "NhanSus");

            migrationBuilder.DropColumn(
                name: "LinhVucChuyenMon",
                table: "NhanSus");

            migrationBuilder.DropColumn(
                name: "LoaiGiayTo",
                table: "NhanSus");

            migrationBuilder.DropColumn(
                name: "LoaiHopDong",
                table: "NhanSus");

            migrationBuilder.DropColumn(
                name: "NgayBatDauHopDong",
                table: "NhanSus");

            migrationBuilder.DropColumn(
                name: "NgayCap",
                table: "NhanSus");

            migrationBuilder.DropColumn(
                name: "NgayKetThucHopDong",
                table: "NhanSus");

            migrationBuilder.DropColumn(
                name: "NguoiLienHeKhanCap",
                table: "NhanSus");

            migrationBuilder.DropColumn(
                name: "NguoiQuanLyTrucTiep",
                table: "NhanSus");

            migrationBuilder.DropColumn(
                name: "NoiCap",
                table: "NhanSus");

            migrationBuilder.DropColumn(
                name: "QuocTich",
                table: "NhanSus");

            migrationBuilder.DropColumn(
                name: "SoDienThoaiKhanCap",
                table: "NhanSus");

            migrationBuilder.DropColumn(
                name: "SoGiayTo",
                table: "NhanSus");

            migrationBuilder.DropColumn(
                name: "TrangThaiCongTac",
                table: "NhanSus");

            migrationBuilder.DropColumn(
                name: "TrinhDoHocVan",
                table: "NhanSus");

            migrationBuilder.DropColumn(
                name: "TruongDaoTao",
                table: "NhanSus");

            migrationBuilder.DropColumn(
                name: "CoHoatDong",
                table: "HocViens");

            migrationBuilder.RenameColumn(
                name: "TrangThaiHoSo",
                table: "NhanSus",
                newName: "TrangThai");

            migrationBuilder.RenameColumn(
                name: "TinhTrangHonNhan",
                table: "NhanSus",
                newName: "SoCCCD");

            migrationBuilder.RenameColumn(
                name: "KinhNghiemCongTac",
                table: "NhanSus",
                newName: "GhiChu");

            migrationBuilder.RenameColumn(
                name: "EmailCongViec",
                table: "NhanSus",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "DiaChiThuongTru",
                table: "NhanSus",
                newName: "DiaChi");

            migrationBuilder.RenameColumn(
                name: "DiaChiLienHe",
                table: "NhanSus",
                newName: "ChuyenMonDaoTao");

            migrationBuilder.RenameIndex(
                name: "IX_NhanSus_EmailCongViec",
                table: "NhanSus",
                newName: "IX_NhanSus_Email");

            migrationBuilder.AddColumn<string>(
                name: "HocKyNamHoc",
                table: "PhanCongGiangDays",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TrangThai",
                table: "HocViens",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_PhanCongGiangDays_NhanSuId_MonHocId_LopHocId_HocKyNamHoc",
                table: "PhanCongGiangDays",
                columns: new[] { "NhanSuId", "MonHocId", "LopHocId", "HocKyNamHoc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PhanCongGiangDays_NhanSuId_MonHocId_LopHocId_HocKyNamHoc",
                table: "PhanCongGiangDays");

            migrationBuilder.DropColumn(
                name: "HocKyNamHoc",
                table: "PhanCongGiangDays");

            migrationBuilder.DropColumn(
                name: "TrangThai",
                table: "HocViens");

            migrationBuilder.RenameColumn(
                name: "TrangThai",
                table: "NhanSus",
                newName: "TrangThaiHoSo");

            migrationBuilder.RenameColumn(
                name: "SoCCCD",
                table: "NhanSus",
                newName: "TinhTrangHonNhan");

            migrationBuilder.RenameColumn(
                name: "GhiChu",
                table: "NhanSus",
                newName: "KinhNghiemCongTac");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "NhanSus",
                newName: "EmailCongViec");

            migrationBuilder.RenameColumn(
                name: "DiaChi",
                table: "NhanSus",
                newName: "DiaChiThuongTru");

            migrationBuilder.RenameColumn(
                name: "ChuyenMonDaoTao",
                table: "NhanSus",
                newName: "DiaChiLienHe");

            migrationBuilder.RenameIndex(
                name: "IX_NhanSus_Email",
                table: "NhanSus",
                newName: "IX_NhanSus_EmailCongViec");

            migrationBuilder.AddColumn<string>(
                name: "HocKy",
                table: "PhanCongGiangDays",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NamHoc",
                table: "PhanCongGiangDays",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayBatDau",
                table: "PhanCongGiangDays",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayKetThuc",
                table: "PhanCongGiangDays",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChucDanhNgheNghiep",
                table: "NhanSus",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChuyenNganh",
                table: "NhanSus",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CoHoatDong",
                table: "NhanSus",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DanToc",
                table: "NhanSus",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailCaNhan",
                table: "NhanSus",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GhiChuChuyenMon",
                table: "NhanSus",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HocHam",
                table: "NhanSus",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HocVi",
                table: "NhanSus",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinhVucChuyenMon",
                table: "NhanSus",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LoaiGiayTo",
                table: "NhanSus",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LoaiHopDong",
                table: "NhanSus",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayBatDauHopDong",
                table: "NhanSus",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayCap",
                table: "NhanSus",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayKetThucHopDong",
                table: "NhanSus",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NguoiLienHeKhanCap",
                table: "NhanSus",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NguoiQuanLyTrucTiep",
                table: "NhanSus",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoiCap",
                table: "NhanSus",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuocTich",
                table: "NhanSus",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SoDienThoaiKhanCap",
                table: "NhanSus",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SoGiayTo",
                table: "NhanSus",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrangThaiCongTac",
                table: "NhanSus",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TrinhDoHocVan",
                table: "NhanSus",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TruongDaoTao",
                table: "NhanSus",
                type: "TEXT",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CoHoatDong",
                table: "HocViens",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_PhanCongGiangDays_NhanSuId_MonHocId_LopHocId_HocKy_NamHoc",
                table: "PhanCongGiangDays",
                columns: new[] { "NhanSuId", "MonHocId", "LopHocId", "HocKy", "NamHoc" });
        }
    }
}
