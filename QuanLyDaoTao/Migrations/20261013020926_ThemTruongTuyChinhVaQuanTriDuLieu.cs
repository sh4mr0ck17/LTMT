using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyDaoTao.Migrations
{
    /// <inheritdoc />
    public partial class ThemTruongTuyChinhVaQuanTriDuLieu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TruongTuyChinhs",
                columns: table => new
                {
                    TruongTuyChinhId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenBang = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MaTruong = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TenHienThi = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    KieuDuLieu = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    TuyChonDanhSach = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    GiaTriMacDinh = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    LaBatBuoc = table.Column<bool>(type: "INTEGER", nullable: false),
                    ThuTuHienThi = table.Column<int>(type: "INTEGER", nullable: false),
                    CoHoatDong = table.Column<bool>(type: "INTEGER", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TruongTuyChinhs", x => x.TruongTuyChinhId);
                });

            migrationBuilder.CreateTable(
                name: "GiaTriTruongTuyChinhs",
                columns: table => new
                {
                    GiaTriId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TruongTuyChinhId = table.Column<int>(type: "INTEGER", nullable: false),
                    TenBang = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    KhoaBanGhi = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    GiaTri = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    NgayCapNhat = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiaTriTruongTuyChinhs", x => x.GiaTriId);
                    table.ForeignKey(
                        name: "FK_GiaTriTruongTuyChinhs_TruongTuyChinhs_TruongTuyChinhId",
                        column: x => x.TruongTuyChinhId,
                        principalTable: "TruongTuyChinhs",
                        principalColumn: "TruongTuyChinhId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GiaTriTruongTuyChinhs_TenBang_KhoaBanGhi",
                table: "GiaTriTruongTuyChinhs",
                columns: new[] { "TenBang", "KhoaBanGhi" });

            migrationBuilder.CreateIndex(
                name: "IX_GiaTriTruongTuyChinhs_TruongTuyChinhId_KhoaBanGhi",
                table: "GiaTriTruongTuyChinhs",
                columns: new[] { "TruongTuyChinhId", "KhoaBanGhi" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TruongTuyChinhs_TenBang",
                table: "TruongTuyChinhs",
                column: "TenBang");

            migrationBuilder.CreateIndex(
                name: "IX_TruongTuyChinhs_TenBang_MaTruong",
                table: "TruongTuyChinhs",
                columns: new[] { "TenBang", "MaTruong" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GiaTriTruongTuyChinhs");

            migrationBuilder.DropTable(
                name: "TruongTuyChinhs");
        }
    }
}
