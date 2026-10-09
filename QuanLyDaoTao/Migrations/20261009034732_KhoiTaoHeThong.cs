using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyDaoTao.Migrations
{
    /// <inheritdoc />
    public partial class KhoiTaoHeThong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    MoTa = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    NgayTao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    HoVaTen = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MaNhanSuLienKet = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CoHoatDong = table.Column<bool>(type: "INTEGER", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChucVus",
                columns: table => new
                {
                    ChucVuId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaChucVu = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TenChucVu = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    CoHoatDong = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChucVus", x => x.ChucVuId);
                });

            migrationBuilder.CreateTable(
                name: "Khoas",
                columns: table => new
                {
                    KhoaId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaKhoa = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TenKhoa = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    MoTa = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    SoDienThoai = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    DiaDiem = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CoHoatDong = table.Column<bool>(type: "INTEGER", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Khoas", x => x.KhoaId);
                });

            migrationBuilder.CreateTable(
                name: "NhatKyHeThongs",
                columns: table => new
                {
                    NhatKyId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NguoiThucHien = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ThoiDiem = table.Column<DateTime>(type: "TEXT", nullable: false),
                    HanhDong = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    BangDuLieu = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    KhoaBanGhi = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    GiaTriCu = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    GiaTriMoi = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    KetQua = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DiaChiIp = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhatKyHeThongs", x => x.NhatKyId);
                });

            migrationBuilder.CreateTable(
                name: "TrinhDoChuyenMons",
                columns: table => new
                {
                    TrinhDoChuyenMonId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaTrinhDo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TenTrinhDo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    CoHoatDong = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrinhDoChuyenMons", x => x.TrinhDoChuyenMonId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderKey = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BoMons",
                columns: table => new
                {
                    BoMonId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaBoMon = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TenBoMon = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    MoTa = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    KhoaId = table.Column<int>(type: "INTEGER", nullable: false),
                    CoHoatDong = table.Column<bool>(type: "INTEGER", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoMons", x => x.BoMonId);
                    table.ForeignKey(
                        name: "FK_BoMons_Khoas_KhoaId",
                        column: x => x.KhoaId,
                        principalTable: "Khoas",
                        principalColumn: "KhoaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LopHocs",
                columns: table => new
                {
                    LopHocId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaLop = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TenLop = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    NienKhoa = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    KhoaId = table.Column<int>(type: "INTEGER", nullable: false),
                    SoLuongToiDa = table.Column<int>(type: "INTEGER", nullable: false),
                    CoHoatDong = table.Column<bool>(type: "INTEGER", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LopHocs", x => x.LopHocId);
                    table.ForeignKey(
                        name: "FK_LopHocs_Khoas_KhoaId",
                        column: x => x.KhoaId,
                        principalTable: "Khoas",
                        principalColumn: "KhoaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MonHocs",
                columns: table => new
                {
                    MonHocId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaMonHoc = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TenMonHoc = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    SoTinChi = table.Column<int>(type: "INTEGER", nullable: false),
                    SoTietLyThuyet = table.Column<int>(type: "INTEGER", nullable: false),
                    SoTietThucHanh = table.Column<int>(type: "INTEGER", nullable: false),
                    BoMonId = table.Column<int>(type: "INTEGER", nullable: true),
                    CoHoatDong = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonHocs", x => x.MonHocId);
                    table.ForeignKey(
                        name: "FK_MonHocs_BoMons_BoMonId",
                        column: x => x.BoMonId,
                        principalTable: "BoMons",
                        principalColumn: "BoMonId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NhanSus",
                columns: table => new
                {
                    NhanSuId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaNhanSu = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TrangThaiHoSo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    HinhAnh = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    HoVaTen = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GioiTinh = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    QuocTich = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DanToc = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    TinhTrangHonNhan = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    LoaiGiayTo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SoGiayTo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    NgayCap = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NoiCap = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SoDienThoai = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    EmailCongViec = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EmailCaNhan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    DiaChiThuongTru = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DiaChiLienHe = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    NguoiLienHeKhanCap = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SoDienThoaiKhanCap = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    KhoaId = table.Column<int>(type: "INTEGER", nullable: true),
                    BoMonId = table.Column<int>(type: "INTEGER", nullable: true),
                    ChucVuId = table.Column<int>(type: "INTEGER", nullable: true),
                    ChucDanhNgheNghiep = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    NguoiQuanLyTrucTiep = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    NgayVaoLam = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LoaiHopDong = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    NgayBatDauHopDong = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NgayKetThucHopDong = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TrangThaiCongTac = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TrinhDoChuyenMonId = table.Column<int>(type: "INTEGER", nullable: true),
                    TrinhDoHocVan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ChuyenNganh = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    TruongDaoTao = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    HocVi = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    HocHam = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    LinhVucChuyenMon = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    KinhNghiemCongTac = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    GhiChuChuyenMon = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CoHoatDong = table.Column<bool>(type: "INTEGER", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NguoiTao = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    NguoiCapNhat = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhanSus", x => x.NhanSuId);
                    table.ForeignKey(
                        name: "FK_NhanSus_BoMons_BoMonId",
                        column: x => x.BoMonId,
                        principalTable: "BoMons",
                        principalColumn: "BoMonId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NhanSus_ChucVus_ChucVuId",
                        column: x => x.ChucVuId,
                        principalTable: "ChucVus",
                        principalColumn: "ChucVuId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NhanSus_Khoas_KhoaId",
                        column: x => x.KhoaId,
                        principalTable: "Khoas",
                        principalColumn: "KhoaId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NhanSus_TrinhDoChuyenMons_TrinhDoChuyenMonId",
                        column: x => x.TrinhDoChuyenMonId,
                        principalTable: "TrinhDoChuyenMons",
                        principalColumn: "TrinhDoChuyenMonId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HocViens",
                columns: table => new
                {
                    HocVienId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaHocVien = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    HoVaTen = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GioiTinh = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SoDienThoai = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    DiaChi = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    LopHocId = table.Column<int>(type: "INTEGER", nullable: false),
                    CoHoatDong = table.Column<bool>(type: "INTEGER", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HocViens", x => x.HocVienId);
                    table.ForeignKey(
                        name: "FK_HocViens_LopHocs_LopHocId",
                        column: x => x.LopHocId,
                        principalTable: "LopHocs",
                        principalColumn: "LopHocId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhanCongGiangDays",
                columns: table => new
                {
                    PhanCongId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaPhanCong = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    NhanSuId = table.Column<int>(type: "INTEGER", nullable: false),
                    MonHocId = table.Column<int>(type: "INTEGER", nullable: false),
                    LopHocId = table.Column<int>(type: "INTEGER", nullable: false),
                    HocKy = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    NamHoc = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    VaiTroGiangDay = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LaChuNhiem = table.Column<bool>(type: "INTEGER", nullable: false),
                    NgayBatDau = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NgayKetThuc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GhiChu = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    NgayTao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhanCongGiangDays", x => x.PhanCongId);
                    table.ForeignKey(
                        name: "FK_PhanCongGiangDays_LopHocs_LopHocId",
                        column: x => x.LopHocId,
                        principalTable: "LopHocs",
                        principalColumn: "LopHocId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhanCongGiangDays_MonHocs_MonHocId",
                        column: x => x.MonHocId,
                        principalTable: "MonHocs",
                        principalColumn: "MonHocId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhanCongGiangDays_NhanSus_NhanSuId",
                        column: x => x.NhanSuId,
                        principalTable: "NhanSus",
                        principalColumn: "NhanSuId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BoMons_KhoaId",
                table: "BoMons",
                column: "KhoaId");

            migrationBuilder.CreateIndex(
                name: "IX_BoMons_MaBoMon",
                table: "BoMons",
                column: "MaBoMon",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BoMons_TenBoMon",
                table: "BoMons",
                column: "TenBoMon");

            migrationBuilder.CreateIndex(
                name: "IX_ChucVus_MaChucVu",
                table: "ChucVus",
                column: "MaChucVu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HocViens_HoVaTen",
                table: "HocViens",
                column: "HoVaTen");

            migrationBuilder.CreateIndex(
                name: "IX_HocViens_LopHocId",
                table: "HocViens",
                column: "LopHocId");

            migrationBuilder.CreateIndex(
                name: "IX_HocViens_MaHocVien",
                table: "HocViens",
                column: "MaHocVien",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Khoas_MaKhoa",
                table: "Khoas",
                column: "MaKhoa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Khoas_TenKhoa",
                table: "Khoas",
                column: "TenKhoa");

            migrationBuilder.CreateIndex(
                name: "IX_LopHocs_KhoaId",
                table: "LopHocs",
                column: "KhoaId");

            migrationBuilder.CreateIndex(
                name: "IX_LopHocs_MaLop",
                table: "LopHocs",
                column: "MaLop",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LopHocs_NienKhoa",
                table: "LopHocs",
                column: "NienKhoa");

            migrationBuilder.CreateIndex(
                name: "IX_LopHocs_TenLop",
                table: "LopHocs",
                column: "TenLop");

            migrationBuilder.CreateIndex(
                name: "IX_MonHocs_BoMonId",
                table: "MonHocs",
                column: "BoMonId");

            migrationBuilder.CreateIndex(
                name: "IX_MonHocs_MaMonHoc",
                table: "MonHocs",
                column: "MaMonHoc",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MonHocs_TenMonHoc",
                table: "MonHocs",
                column: "TenMonHoc");

            migrationBuilder.CreateIndex(
                name: "IX_NhanSus_BoMonId",
                table: "NhanSus",
                column: "BoMonId");

            migrationBuilder.CreateIndex(
                name: "IX_NhanSus_ChucVuId",
                table: "NhanSus",
                column: "ChucVuId");

            migrationBuilder.CreateIndex(
                name: "IX_NhanSus_EmailCongViec",
                table: "NhanSus",
                column: "EmailCongViec");

            migrationBuilder.CreateIndex(
                name: "IX_NhanSus_HoVaTen",
                table: "NhanSus",
                column: "HoVaTen");

            migrationBuilder.CreateIndex(
                name: "IX_NhanSus_KhoaId",
                table: "NhanSus",
                column: "KhoaId");

            migrationBuilder.CreateIndex(
                name: "IX_NhanSus_MaNhanSu",
                table: "NhanSus",
                column: "MaNhanSu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NhanSus_TrinhDoChuyenMonId",
                table: "NhanSus",
                column: "TrinhDoChuyenMonId");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyHeThongs_BangDuLieu",
                table: "NhatKyHeThongs",
                column: "BangDuLieu");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyHeThongs_NguoiThucHien",
                table: "NhatKyHeThongs",
                column: "NguoiThucHien");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyHeThongs_ThoiDiem",
                table: "NhatKyHeThongs",
                column: "ThoiDiem");

            migrationBuilder.CreateIndex(
                name: "IX_PhanCongGiangDays_LopHocId",
                table: "PhanCongGiangDays",
                column: "LopHocId");

            migrationBuilder.CreateIndex(
                name: "IX_PhanCongGiangDays_MaPhanCong",
                table: "PhanCongGiangDays",
                column: "MaPhanCong",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhanCongGiangDays_MonHocId",
                table: "PhanCongGiangDays",
                column: "MonHocId");

            migrationBuilder.CreateIndex(
                name: "IX_PhanCongGiangDays_NhanSuId_MonHocId_LopHocId_HocKy_NamHoc",
                table: "PhanCongGiangDays",
                columns: new[] { "NhanSuId", "MonHocId", "LopHocId", "HocKy", "NamHoc" });

            migrationBuilder.CreateIndex(
                name: "IX_TrinhDoChuyenMons_MaTrinhDo",
                table: "TrinhDoChuyenMons",
                column: "MaTrinhDo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "HocViens");

            migrationBuilder.DropTable(
                name: "NhatKyHeThongs");

            migrationBuilder.DropTable(
                name: "PhanCongGiangDays");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "LopHocs");

            migrationBuilder.DropTable(
                name: "MonHocs");

            migrationBuilder.DropTable(
                name: "NhanSus");

            migrationBuilder.DropTable(
                name: "BoMons");

            migrationBuilder.DropTable(
                name: "ChucVus");

            migrationBuilder.DropTable(
                name: "TrinhDoChuyenMons");

            migrationBuilder.DropTable(
                name: "Khoas");
        }
    }
}
