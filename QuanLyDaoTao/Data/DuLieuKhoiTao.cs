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
    }
}
