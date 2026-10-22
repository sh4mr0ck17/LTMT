using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Models;

namespace QuanLyDaoTao.Data;

/// <summary>
/// Lớp hỗ trợ khởi tạo dữ liệu mặc định ban đầu cho hệ thống.
/// </summary>
public static class DuLieuKhoiTao
{
    /// <summary>
    /// Thực hiện kiểm tra và nạp dữ liệu ban đầu nếu cơ sở dữ liệu chưa có.
    /// </summary>
    /// <param name="dichVuCungCap">Nhà cung cấp dịch vụ DI.</param>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    public static async Task KhoiTaoDuLieuAsync(IServiceProvider dichVuCungCap)
    {
        ArgumentNullException.ThrowIfNull(dichVuCungCap);

        using var scope = dichVuCungCap.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<QuanLyDaoTaoDbContext>();
        var quanLyNguoiDung = scope.ServiceProvider.GetRequiredService<UserManager<TaiKhoan>>();
        var quanLyVaiTro = scope.ServiceProvider.GetRequiredService<RoleManager<VaiTro>>();

        // Tự động kiểm tra và áp dụng migration
        await context.Database.MigrateAsync();

        // 1. Khởi tạo danh sách các vai trò
        string[] danhSachVaiTro = { VaiTroMacDinh.QuanTriVien, VaiTroMacDinh.QuanLyDaoTao, VaiTroMacDinh.NguoiXem };
        foreach (var tenVaiTro in danhSachVaiTro)
        {
            if (!await quanLyVaiTro.RoleExistsAsync(tenVaiTro))
            {
                var vaiTroMoi = new VaiTro(tenVaiTro)
                {
                    MoTa = tenVaiTro switch
                    {
                        VaiTroMacDinh.QuanTriVien => "Quản trị viên toàn quyền hệ thống",
                        VaiTroMacDinh.QuanLyDaoTao => "Quản lý đào tạo và hồ sơ dữ liệu",
                        _ => "Người xem thông tin được cấp phép"
                    },
                    NgayTao = DateTime.UtcNow
                };
                await quanLyVaiTro.CreateAsync(vaiTroMoi);
            }
        }

        // 2. Khởi tạo tài khoản Quản trị viên mặc định
        const string emailAdmin = "admin@quanlydaotao.local";
        var taiKhoanAdmin = await quanLyNguoiDung.FindByEmailAsync(emailAdmin);
        if (taiKhoanAdmin == null)
        {
            taiKhoanAdmin = new TaiKhoan
            {
                UserName = emailAdmin,
                Email = emailAdmin,
                EmailConfirmed = true,
                HoVaTen = "Quản trị viên Hệ thống",
                CoHoatDong = true,
                NgayTao = DateTime.UtcNow
            };

            var ketQuaTao = await quanLyNguoiDung.CreateAsync(taiKhoanAdmin, "Admin@123456");
            if (ketQuaTao.Succeeded)
            {
                await quanLyNguoiDung.AddToRoleAsync(taiKhoanAdmin, VaiTroMacDinh.QuanTriVien);
            }
        }

        // 3. Khởi tạo chức vụ mẫu
        if (!await context.ChucVus.AnyAsync())
        {
            var danhSachChucVu = new List<ChucVu>
            {
                new() { MaChucVu = "CV_TK", TenChucVu = "Trưởng khoa", MoTa = "Phụ trách quản lý toàn diện khoa", CoHoatDong = true },
                new() { MaChucVu = "CV_PK", TenChucVu = "Phó trưởng khoa", MoTa = "Hỗ trợ quản lý khoa", CoHoatDong = true },
                new() { MaChucVu = "CV_TBM", TenChucVu = "Trưởng bộ môn", MoTa = "Phụ trách học thuật bộ môn", CoHoatDong = true },
                new() { MaChucVu = "CV_GV", TenChucVu = "Giảng viên", MoTa = "Giảng dạy và nghiên cứu khoa học", CoHoatDong = true },
                new() { MaChucVu = "CV_CV", TenChucVu = "Chuyên viên giáo vụ", MoTa = "Hỗ trợ hành chính giáo vụ", CoHoatDong = true }
            };
            await context.ChucVus.AddRangeAsync(danhSachChucVu);
            await context.SaveChangesAsync();
        }

        // 4. Khởi tạo trình độ chuyên môn mẫu
        if (!await context.TrinhDoChuyenMons.AnyAsync())
        {
            var danhSachTrinhDo = new List<TrinhDoChuyenMon>
            {
                new() { MaTrinhDo = "TD_TS", TenTrinhDo = "Tiến sĩ", MoTa = "Học vị Tiến sĩ chuyên ngành", CoHoatDong = true },
                new() { MaTrinhDo = "TD_ThS", TenTrinhDo = "Thạc sĩ", MoTa = "Học vị Thạc sĩ chuyên ngành", CoHoatDong = true },
                new() { MaTrinhDo = "TD_KS", TenTrinhDo = "Kỹ sư", MoTa = "Bằng tốt nghiệp Kỹ sư", CoHoatDong = true },
                new() { MaTrinhDo = "TD_CN", TenTrinhDo = "Cử nhân", MoTa = "Bằng tốt nghiệp Cử nhân", CoHoatDong = true }
            };
            await context.TrinhDoChuyenMons.AddRangeAsync(danhSachTrinhDo);
            await context.SaveChangesAsync();
        }

        // 5. Khởi tạo Khoa mẫu
        if (!await context.Khoas.AnyAsync())
        {
            var khoaCntt = new Khoa
            {
                MaKhoa = "KHOA_CNTT",
                TenKhoa = "Khoa Công nghệ Thông tin",
                MoTa = "Đào tạo các ngành Công nghệ thông tin, Kỹ thuật phần mềm và Khoa học máy tính",
                Email = "cntt@quanlydaotao.local",
                SoDienThoai = "024.38691234",
                DiaDiem = "Tòa nhà A1, Khu trung tâm",
                CoHoatDong = true,
                NgayTao = DateTime.UtcNow
            };
            var khoaDtvt = new Khoa
            {
                MaKhoa = "KHOA_DTVT",
                TenKhoa = "Khoa Điện tử Viễn thông",
                MoTa = "Đào tạo ngành Điện tử, Viễn thông và Kỹ thuật máy tính",
                Email = "dtvt@quanlydaotao.local",
                SoDienThoai = "024.38691235",
                DiaDiem = "Tòa nhà B2, Khu thực nghiệm",
                CoHoatDong = true,
                NgayTao = DateTime.UtcNow
            };
            await context.Khoas.AddRangeAsync(khoaCntt, khoaDtvt);
            await context.SaveChangesAsync();

            // 6. Khởi tạo Bộ môn mẫu
            var boMonKpm = new BoMon
            {
                MaBoMon = "BM_KTPM",
                TenBoMon = "Bộ môn Kỹ thuật Phần mềm",
                MoTa = "Phụ trách các môn học về quy trình phát triển và kiểm thử phần mềm",
                KhoaId = khoaCntt.KhoaId,
                CoHoatDong = true,
                NgayTao = DateTime.UtcNow
            };
            var boMonKhmt = new BoMon
            {
                MaBoMon = "BM_KHMT",
                TenBoMon = "Bộ môn Khoa học Máy tính",
                MoTa = "Phụ trách các môn học về thuật toán, AI và cấu trúc dữ liệu",
                KhoaId = khoaCntt.KhoaId,
                CoHoatDong = true,
                NgayTao = DateTime.UtcNow
            };
            await context.BoMons.AddRangeAsync(boMonKpm, boMonKhmt);
            await context.SaveChangesAsync();

            // 7. Khởi tạo Lớp học mẫu
            var lopCntt1 = new LopHoc
            {
                MaLop = "CNTT_K20A",
                TenLop = "Công nghệ Thông tin K20A",
                NienKhoa = "2024-2028",
                KhoaId = khoaCntt.KhoaId,
                SoLuongToiDa = 60,
                CoHoatDong = true,
                NgayTao = DateTime.UtcNow
            };
            var lopCntt2 = new LopHoc
            {
                MaLop = "CNTT_K20B",
                TenLop = "Công nghệ Thông tin K20B",
                NienKhoa = "2024-2028",
                KhoaId = khoaCntt.KhoaId,
                SoLuongToiDa = 60,
                CoHoatDong = true,
                NgayTao = DateTime.UtcNow
            };
            await context.LopHocs.AddRangeAsync(lopCntt1, lopCntt2);
            await context.SaveChangesAsync();

            // 8. Khởi tạo Môn học mẫu
            var monLapTrinhNet = new MonHoc
            {
                MaMonHoc = "CSHARP_NET",
                TenMonHoc = "Lập trình ứng dụng .NET C#",
                SoTinChi = 3,
                SoTietLyThuyet = 30,
                SoTietThucHanh = 15,
                BoMonId = boMonKpm.BoMonId,
                CoHoatDong = true
            };
            var monCsdl = new MonHoc
            {
                MaMonHoc = "CSDL_COBAN",
                TenMonHoc = "Cơ sở dữ liệu quan hệ",
                SoTinChi = 3,
                SoTietLyThuyet = 30,
                SoTietThucHanh = 15,
                BoMonId = boMonKhmt.BoMonId,
                CoHoatDong = true
            };
            await context.MonHocs.AddRangeAsync(monLapTrinhNet, monCsdl);
            await context.SaveChangesAsync();
        }

        // 9. Khởi tạo Hồ sơ Nhân sự / Giảng viên mẫu nếu chưa có
        if (!await context.NhanSus.AnyAsync())
        {
            var khoaDau = await context.Khoas.FirstOrDefaultAsync();
            var boMonDau = await context.BoMons.FirstOrDefaultAsync();
            var chucVuGv = await context.ChucVus.FirstOrDefaultAsync(c => c.MaChucVu == "CV_GV") ?? await context.ChucVus.FirstOrDefaultAsync();
            var trinhDoTs = await context.TrinhDoChuyenMons.FirstOrDefaultAsync(t => t.MaTrinhDo == "TD_TS") ?? await context.TrinhDoChuyenMons.FirstOrDefaultAsync();

            if (khoaDau != null)
            {
                var ns1 = new NhanSu
                {
                    MaNhanSu = "CB_C500_01",
                    HoVaTen = "Đại tá, TS. Nguyễn Minh Đức",
                    NgaySinh = new DateTime(1978, 5, 19),
                    GioiTinh = "Nam",
                    EmailCongViec = "duc.nm@c500.edu.vn",
                    SoDienThoai = "0912345678",
                    SoGiayTo = "001078001234",
                    KhoaId = khoaDau.KhoaId,
                    BoMonId = boMonDau?.BoMonId,
                    ChucVuId = chucVuGv?.ChucVuId,
                    TrinhDoChuyenMonId = trinhDoTs?.TrinhDoChuyenMonId,
                    ChucDanhNgheNghiep = "Giảng viên cao cấp",
                    HocVi = "Tiến sĩ",
                    HocHam = "Phó Giáo sư",
                    ChuyenNganh = "An ninh mạng & Công nghệ thông tin",
                    TruongDaoTao = "Học viện An ninh Nhân dân",
                    TrangThaiCongTac = "Đang làm việc",
                    CoHoatDong = true,
                    NgayTao = DateTime.UtcNow
                };
                var ns2 = new NhanSu
                {
                    MaNhanSu = "CB_C500_02",
                    HoVaTen = "Thượng tá, ThS. Trần Thu Hà",
                    NgaySinh = new DateTime(1984, 10, 20),
                    GioiTinh = "Nữ",
                    EmailCongViec = "ha.tt@c500.edu.vn",
                    SoDienThoai = "0987654321",
                    SoGiayTo = "001184005678",
                    KhoaId = khoaDau.KhoaId,
                    BoMonId = boMonDau?.BoMonId,
                    ChucVuId = chucVuGv?.ChucVuId,
                    TrinhDoChuyenMonId = trinhDoTs?.TrinhDoChuyenMonId,
                    ChucDanhNgheNghiep = "Giảng viên chính",
                    HocVi = "Thạc sĩ",
                    ChuyenNganh = "Hệ thống thông tin nghiệp vụ",
                    TruongDaoTao = "Học viện An ninh Nhân dân",
                    TrangThaiCongTac = "Đang làm việc",
                    CoHoatDong = true,
                    NgayTao = DateTime.UtcNow
                };
                await context.NhanSus.AddRangeAsync(ns1, ns2);
                await context.SaveChangesAsync();
            }
        }

        // 10. Khởi tạo Học viên mẫu nếu chưa có
        if (!await context.HocViens.AnyAsync())
        {
            var lopDau = await context.LopHocs.FirstOrDefaultAsync();
            if (lopDau != null)
            {
                await context.HocViens.AddRangeAsync(
                    new HocVien
                    {
                        MaHocVien = "B20D52_001",
                        HoVaTen = "Lê Hoàng Nam",
                        NgaySinh = new DateTime(2004, 8, 15),
                        GioiTinh = "Nam",
                        LopHocId = lopDau.LopHocId,
                        Email = "nam.lh@hv.c500.edu.vn",
                        SoDienThoai = "0901112233",
                        DiaChi = "Hà Nội",
                        CoHoatDong = true,
                        NgayTao = DateTime.UtcNow
                    },
                    new HocVien
                    {
                        MaHocVien = "B20D52_002",
                        HoVaTen = "Phạm Mai Anh",
                        NgaySinh = new DateTime(2004, 11, 2),
                        GioiTinh = "Nữ",
                        LopHocId = lopDau.LopHocId,
                        Email = "anh.pm@hv.c500.edu.vn",
                        SoDienThoai = "0902223344",
                        DiaChi = "Hải Phòng",
                        CoHoatDong = true,
                        NgayTao = DateTime.UtcNow
                    });
                await context.SaveChangesAsync();
            }
        }

        // 11. Khởi tạo Phân công giảng dạy mẫu nếu chưa có
        if (!await context.PhanCongGiangDays.AnyAsync())
        {
            var nsDau = await context.NhanSus.FirstOrDefaultAsync();
            var mhDau = await context.MonHocs.FirstOrDefaultAsync();
            var lopDau = await context.LopHocs.FirstOrDefaultAsync();
            if (nsDau != null && mhDau != null && lopDau != null)
            {
                await context.PhanCongGiangDays.AddAsync(new PhanCongGiangDay
                {
                    MaPhanCong = "PC_C500_01",
                    NhanSuId = nsDau.NhanSuId,
                    MonHocId = mhDau.MonHocId,
                    LopHocId = lopDau.LopHocId,
                    HocKy = "Học kỳ 1",
                    NamHoc = "2026-2027",
                    VaiTroGiangDay = "Giảng viên chính",
                    LaChuNhiem = true,
                    GhiChu = "Giảng đường A1 - C500",
                    NgayTao = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
            }
        }
    }
}
