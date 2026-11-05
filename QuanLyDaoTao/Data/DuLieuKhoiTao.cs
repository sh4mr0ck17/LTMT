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

        // 1. Khởi tạo 4 vai trò chuẩn (v4)
        foreach (var tenVaiTro in VaiTroMacDinh.TatCa)
        {
            try
            {
                if (!await quanLyVaiTro.RoleExistsAsync(tenVaiTro))
                {
                    var vaiTroMoi = new VaiTro(tenVaiTro)
                    {
                        MoTa = tenVaiTro switch
                        {
                            VaiTroMacDinh.SuperAdmin => "Siêu quản trị viên - toàn quyền, Live CRUD Database Studio",
                            VaiTroMacDinh.QuanTriVien => "Quản trị viên - toàn quyền nghiệp vụ, cấp phát và phân quyền tài khoản",
                            VaiTroMacDinh.QuanLy => "Quản lý - ghi dữ liệu trong vùng và khoa được giao",
                            _ => "Người xem - chỉ tra cứu, thông tin cá nhân bị che"
                        },
                        NgayTao = DateTime.UtcNow
                    };
                    await quanLyVaiTro.CreateAsync(vaiTroMoi);
                }
            }
            catch (DbUpdateException)
            {
                // Luồng song song khác đã khởi tạo vai trò này.
            }
        }

        // 2. Chuyển tài khoản đang giữ vai trò cũ sang 4 vai trò mới, rồi xóa vai trò cũ
        await ChuyenDoiVaiTroCuAsync(quanLyNguoiDung, quanLyVaiTro);

        // 3. Tài khoản mẫu (chỉ tạo khi chưa có; mật khẩu mẫu phải đổi khi triển khai thật)
        var danhSachTaiKhoanMau = new (string Email, string HoTen, string VaiTro, string? Vung)[]
        {
            ("superadmin@quanlydaotao.local", "Super Administrator C500", VaiTroMacDinh.SuperAdmin, null),
            ("admin@quanlydaotao.local", "Quản trị viên Hệ thống", VaiTroMacDinh.QuanTriVien, null),
            ("quanly.tochuc@quanlydaotao.local", "Quản lý Tổ chức - Cán bộ", VaiTroMacDinh.QuanLy, VungQuanLy.ToChuc),
            ("quanly.daotao@quanlydaotao.local", "Quản lý Đào tạo", VaiTroMacDinh.QuanLy, VungQuanLy.DaoTao),
            ("nguoixem@quanlydaotao.local", "Người xem tra cứu", VaiTroMacDinh.NguoiXem, null)
        };

        foreach (var tkMau in danhSachTaiKhoanMau)
        {
            try
            {
                var taiKhoan = await quanLyNguoiDung.FindByEmailAsync(tkMau.Email);
                if (taiKhoan == null)
                {
                    taiKhoan = new TaiKhoan
                    {
                        UserName = tkMau.Email,
                        Email = tkMau.Email,
                        EmailConfirmed = true,
                        HoVaTen = tkMau.HoTen,
                        VungQuanLy = tkMau.Vung,
                        CoHoatDong = true,
                        LockoutEnabled = true,
                        NgayTao = DateTime.UtcNow
                    };

                    var ketQuaTao = await quanLyNguoiDung.CreateAsync(taiKhoan, MatKhauMau);
                    if (ketQuaTao.Succeeded)
                    {
                        await quanLyNguoiDung.AddToRoleAsync(taiKhoan, tkMau.VaiTro);
                    }
                }
                else if (!await quanLyNguoiDung.IsInRoleAsync(taiKhoan, tkMau.VaiTro) &&
                         (await quanLyNguoiDung.GetRolesAsync(taiKhoan)).Count == 0)
                {
                    await quanLyNguoiDung.AddToRoleAsync(taiKhoan, tkMau.VaiTro);
                }
            }
            catch (DbUpdateException)
            {
                // Luồng song song khác đã tạo tài khoản này.
            }
        }

        // 4. Chuyển đổi dữ liệu nghiệp vụ lên mô hình v4 (idempotent)
        await ChuyenDoiDuLieuV4Async(context);

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
                    Email = "duc.nm@c500.edu.vn",
                    SoDienThoai = "0912345678",
                    SoCCCD = "001078001234",
                    KhoaId = khoaDau.KhoaId,
                    BoMonId = boMonDau?.BoMonId,
                    ChucVuId = chucVuGv?.ChucVuId,
                    TrinhDoChuyenMonId = trinhDoTs?.TrinhDoChuyenMonId,
                    HocHam = "Phó Giáo sư",
                    ChuyenMonDaoTao = "An ninh mạng & Công nghệ thông tin — Học viện An ninh Nhân dân",
                    TrangThai = "Đang làm việc",
                    NgayTao = DateTime.UtcNow
                };
                var ns2 = new NhanSu
                {
                    MaNhanSu = "CB_C500_02",
                    HoVaTen = "Thượng tá, ThS. Trần Thu Hà",
                    NgaySinh = new DateTime(1984, 10, 20),
                    GioiTinh = "Nữ",
                    Email = "ha.tt@c500.edu.vn",
                    SoDienThoai = "0987654321",
                    SoCCCD = "001184005678",
                    KhoaId = khoaDau.KhoaId,
                    BoMonId = boMonDau?.BoMonId,
                    ChucVuId = chucVuGv?.ChucVuId,
                    TrinhDoChuyenMonId = trinhDoTs?.TrinhDoChuyenMonId,
                    ChuyenMonDaoTao = "Hệ thống thông tin nghiệp vụ — Học viện An ninh Nhân dân",
                    TrangThai = "Đang làm việc",
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

    /// <summary>
    /// Mật khẩu của tài khoản mẫu (chỉ dùng cho môi trường phát triển và demo).
    /// </summary>
    public const string MatKhauMau = "Admin@123456";

    private const string MaTruongChucVuHocVienCu = "ChucVu";

    /// <summary>
    /// Chuyển tài khoản đang giữ vai trò cũ (DataManager, CanBo, GiaoVien, User) sang vai trò v4,
    /// gán vùng quản lý tương ứng, đổi SecurityStamp để phiên cũ hết hiệu lực, rồi xóa vai trò cũ.
    /// </summary>
    public static async Task ChuyenDoiVaiTroCuAsync(UserManager<TaiKhoan> quanLyNguoiDung, RoleManager<VaiTro> quanLyVaiTro)
    {
        ArgumentNullException.ThrowIfNull(quanLyNguoiDung);
        ArgumentNullException.ThrowIfNull(quanLyVaiTro);

        foreach (var (vaiTroCu, anhXa) in VaiTroMacDinh.AnhXaVaiTroCu)
        {
            var vaiTro = await quanLyVaiTro.FindByNameAsync(vaiTroCu);
            if (vaiTro is null)
            {
                continue;
            }

            foreach (var taiKhoan in await quanLyNguoiDung.GetUsersInRoleAsync(vaiTroCu))
            {
                await quanLyNguoiDung.RemoveFromRoleAsync(taiKhoan, vaiTroCu);
                var vaiTroHienCo = await quanLyNguoiDung.GetRolesAsync(taiKhoan);
                if (!vaiTroHienCo.Any(VaiTroMacDinh.TatCa.Contains))
                {
                    await quanLyNguoiDung.AddToRoleAsync(taiKhoan, anhXa.VaiTroMoi);
                    taiKhoan.VungQuanLy = anhXa.Vung;
                }

                await quanLyNguoiDung.UpdateSecurityStampAsync(taiKhoan);
            }

            await quanLyVaiTro.DeleteAsync(vaiTro);
        }
    }

    /// <summary>
    /// Chuyển dữ liệu lên mô hình v4: điền Hệ/Khóa/Chuyên ngành cho lớp, chuyển chức vụ học viên
    /// từ trường tùy chỉnh sang cột ChucVuLop, bổ sung chức vụ Phó trưởng bộ môn. Chạy lặp lại không gây trùng.
    /// </summary>
    public static async Task ChuyenDoiDuLieuV4Async(QuanLyDaoTaoDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Lớp: suy khóa học từ mã B{số}D{khóa} và điền chuyên ngành theo số lớp hệ Chính quy.
        var lopCanDien = await context.LopHocs.Where(l => l.KhoaHoc == null).ToListAsync();
        foreach (var lop in lopCanDien)
        {
            lop.KhoaHoc = Services.QuyTacNghiepVu.SuyRaKhoaHoc(lop.MaLop);
            if (lop.KhoaHoc is not null && lop.ChuyenNganh is null &&
                int.TryParse(lop.MaLop[1..^lop.KhoaHoc.Length], out var soLop))
            {
                lop.ChuyenNganh = soLop switch
                {
                    <= 3 => "Trinh sát an ninh",
                    <= 7 => "Điều tra hình sự",
                    8 => "Quản lý xuất nhập cảnh",
                    <= 12 => "An ninh mạng & PCTP sử dụng CNC",
                    _ => "Công nghệ thông tin"
                };
            }
        }

        // Học viên: chuyển chức vụ từ trường tùy chỉnh "ChucVu" sang cột thật để áp chỉ tiêu.
        var truongChucVu = await context.TruongTuyChinhs
            .FirstOrDefaultAsync(t => t.TenBang == "HocVien" && t.MaTruong == MaTruongChucVuHocVienCu);
        if (truongChucVu is not null)
        {
            var giaTris = await context.GiaTriTruongTuyChinhs
                .Where(g => g.TruongTuyChinhId == truongChucVu.TruongTuyChinhId)
                .ToListAsync();
            var theoMa = giaTris
                .Where(g => !string.IsNullOrWhiteSpace(g.GiaTri))
                .GroupBy(g => g.KhoaBanGhi)
                .ToDictionary(g => g.Key, g => g.First().GiaTri);
            var hocViens = await context.HocViens.Where(hv => hv.ChucVuLop == null).ToListAsync();
            foreach (var hocVien in hocViens)
            {
                if (theoMa.TryGetValue(hocVien.MaHocVien, out var chucVu) ||
                    theoMa.TryGetValue(hocVien.HocVienId.ToString(System.Globalization.CultureInfo.InvariantCulture), out chucVu))
                {
                    hocVien.ChucVuLop = Services.QuyTacNghiepVu.GhepChucVuLop(Services.QuyTacNghiepVu.TachChucVuLop(chucVu));
                }
            }

            context.GiaTriTruongTuyChinhs.RemoveRange(giaTris);
            context.TruongTuyChinhs.Remove(truongChucVu);
        }

        // Chuẩn hóa trạng thái: dữ liệu cũ dùng "Hoạt động" cho học viên và giá trị tự do cho cán bộ.
        await context.HocViens.Where(hv => hv.TrangThai == "Hoạt động" || hv.TrangThai == "")
            .ExecuteUpdateAsync(cn => cn.SetProperty(hv => hv.TrangThai, "Đang học"));
        string[] trangThaiCanBoChuan = ["Đang làm việc", "Tạm nghỉ", "Đi học", "Đã nghỉ việc", "Đã khóa"];
        await context.NhanSus.Where(ns => !trangThaiCanBoChuan.Contains(ns.TrangThai))
            .ExecuteUpdateAsync(cn => cn.SetProperty(ns => ns.TrangThai, "Đang làm việc"));

        if (!await context.ChucVus.AnyAsync(cv => cv.MaChucVu == "CV_PBM"))
        {
            context.ChucVus.Add(new ChucVu
            {
                MaChucVu = "CV_PBM",
                TenChucVu = "Phó trưởng bộ môn",
                MoTa = "Hỗ trợ trưởng bộ môn",
                CoHoatDong = true
            });
        }

        await context.SaveChangesAsync();
    }
}
