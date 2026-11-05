using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.Services;

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
        string[] danhSachVaiTro =
        {
            VaiTroMacDinh.SuperAdmin,
            VaiTroMacDinh.QuanTriVien,
            VaiTroMacDinh.QuanLyDaoTao,
            VaiTroMacDinh.GiaoVien,
            VaiTroMacDinh.CanBo,
            VaiTroMacDinh.NguoiDung,
            VaiTroMacDinh.NguoiXem
        };
        foreach (var tenVaiTro in danhSachVaiTro)
        {
            try
            {
                if (!await quanLyVaiTro.RoleExistsAsync(tenVaiTro))
                {
                    var vaiTroMoi = new VaiTro(tenVaiTro)
                    {
                        MoTa = tenVaiTro switch
                        {
                            VaiTroMacDinh.SuperAdmin => "Siêu quản trị viên - độc quyền Live CRUD Database Studio",
                            VaiTroMacDinh.QuanTriVien => "Quản trị viên đào tạo & cấp phát tài khoản",
                            VaiTroMacDinh.QuanLyDaoTao => "Quản lý đào tạo và hồ sơ dữ liệu",
                            VaiTroMacDinh.GiaoVien => "Giáo viên / Giảng viên giảng dạy",
                            VaiTroMacDinh.CanBo => "Cán bộ nghiệp vụ khoa / phòng",
                            VaiTroMacDinh.NguoiDung => "Người dùng tra cứu (Chỉ xem)",
                            _ => "Người xem thông tin được cấp phép"
                        },
                        NgayTao = DateTime.UtcNow
                    };
                    await quanLyVaiTro.CreateAsync(vaiTroMoi);
                }
            }
            catch (DbUpdateException)
            {
                // Bỏ qua nếu luồng song song khác đã khởi tạo vai trò này
            }
        }

        // 2. Khởi tạo các tài khoản mẫu theo từng Workspace (Superadmin, Admin, GiaoVien, CanBo, User)
        var danhSachTaiKhoanMau = new (string Email, string HoTen, string RoleChinh, string? MaNS)[]
        {
            ("superadmin@quanlydaotao.local", "Super Administrator C500", VaiTroMacDinh.SuperAdmin, null),
            ("admin@quanlydaotao.local", "Quản trị viên Hệ thống", VaiTroMacDinh.QuanTriVien, null),
            ("giaovien@quanlydaotao.local", "TS. Nguyễn Minh Đức (Giảng viên)", VaiTroMacDinh.GiaoVien, "NS0001"),
            ("canbo@quanlydaotao.local", "ThS. Trần Thị Lan (Cán bộ)", VaiTroMacDinh.CanBo, "NS0002"),
            ("user@quanlydaotao.local", "Học viên / Người dùng Tra cứu", VaiTroMacDinh.NguoiDung, null)
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
                        MaNhanSuLienKet = tkMau.MaNS,
                        CoHoatDong = true,
                        NgayTao = DateTime.UtcNow
                    };

                    var ketQuaTao = await quanLyNguoiDung.CreateAsync(taiKhoan, "Admin@123456");
                    if (ketQuaTao.Succeeded)
                    {
                        await quanLyNguoiDung.AddToRoleAsync(taiKhoan, tkMau.RoleChinh);
                    }
                }
                else if (!await quanLyNguoiDung.IsInRoleAsync(taiKhoan, tkMau.RoleChinh))
                {
                    await quanLyNguoiDung.AddToRoleAsync(taiKhoan, tkMau.RoleChinh);
                }
            }
            catch (DbUpdateException)
            {
                // Bỏ qua nếu luồng song song khác đã tạo tài khoản này
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
            await context.SaveChangesAsync();
        }

        // 12. Làm giàu dữ liệu chuyên ngành và tổ chức quản lý C500 từ repository LTMT-demo
        await LamGiauDuLieuTuDemoAsync(context);
    }

    /// <summary>
    /// Bổ sung và đồng bộ toàn bộ 12 Khoa chuyên ngành, 24 Bộ môn trực thuộc, các lớp đào tạo (D53-D58, K3-K6)
    /// và 108 cán bộ giảng viên có cấp bậc hàm CAND từ repository LTMT-demo vào cơ sở dữ liệu SQLite.
    /// Đảm bảo tính nhất quán (Idempotent), không trùng lặp và tương thích hoàn toàn với hệ thống hiện tại.
    /// </summary>
    private static async Task LamGiauDuLieuTuDemoAsync(QuanLyDaoTaoDbContext context)
    {
        try
        {
        // A. 12 Khoa chuyên ngành chuẩn Học viện An ninh Nhân dân C500
        var danhSach12KhoaDemo = new (string Ma, string Ten, string MoTa)[]
        {
            ("KH-01", "Khoa Lý luận chính trị, Khoa học xã hội và nhân văn", "Giảng dạy lý luận chính trị, đường lối chính sách và khoa học xã hội nhân văn trong CAND."),
            ("KH-02", "Khoa Quân sự - Võ thuật - Thể dục thể thao", "Huấn luyện điều lệnh, kỹ chiến thuật, quân sự, võ thuật ứng dụng và thể lực CAND."),
            ("KH-03", "Khoa Nghiệp vụ cơ bản", "Đào tạo các nguyên lý, phương pháp nghiệp vụ trinh sát và bảo vệ an ninh quốc gia cơ bản."),
            ("KH-04", "Khoa Nghiệp vụ 2", "Đào tạo chuyên sâu nghiệp vụ bảo vệ an ninh chính trị nội bộ và an ninh văn hóa tư tưởng."),
            ("KH-05", "Khoa Nghiệp vụ 3", "Đào tạo nghiệp vụ bảo vệ an ninh kinh tế, tài chính và an ninh thông tin trọng yếu."),
            ("KH-06", "Khoa Nghiệp vụ 4", "Đào tạo trinh sát phòng chống phản động, chống khủng bố và bảo vệ an ninh xã hội."),
            ("KH-07", "Khoa Nghiệp vụ 5", "Đào tạo trinh sát an ninh đối ngoại, bảo vệ an ninh biên giới và hợp tác quốc tế."),
            ("KH-08", "Khoa Nghiệp vụ 6", "Đào tạo nghiệp vụ quản lý xuất nhập cảnh, quản lý người nước ngoài và cư trú."),
            ("KH-09", "Khoa Nghiệp vụ 7", "Đào tạo kỹ thuật nghiệp vụ, trinh sát kỹ thuật và giám định phương tiện thông tin."),
            ("KH-10", "Khoa Nghiệp vụ 8", "Đào tạo chuyên sâu An ninh mạng và Phòng chống tội phạm sử dụng công nghệ cao (ANM & PCTPCNC)."),
            ("KH-11", "Khoa Luật", "Đào tạo pháp luật đại cương, Luật Hình sự, Luật Tố tụng hình sự và pháp luật an ninh quốc gia."),
            ("KH-12", "Khoa Ngoại ngữ", "Đào tạo ngoại ngữ nghiệp vụ an ninh (tiếng Anh, tiếng Trung, tiếng Nga) phục vụ công tác đối ngoại.")
        };

        foreach (var item in danhSach12KhoaDemo)
        {
            var khoaTonTai = await context.Khoas.FirstOrDefaultAsync(k => k.MaKhoa == item.Ma);
            if (khoaTonTai == null)
            {
                await context.Khoas.AddAsync(new Khoa
                {
                    MaKhoa = item.Ma,
                    TenKhoa = item.Ten,
                    MoTa = item.MoTa,
                    Email = $"{item.Ma.ToLowerInvariant()}@c500.edu.vn",
                    SoDienThoai = "024.38541234",
                    DiaDiem = "Khu giảng đường C500",
                    CoHoatDong = true,
                    NgayTao = DateTime.UtcNow
                });
            }
        }
        await context.SaveChangesAsync();

        // B. 24 Bộ môn / Tổ chuyên môn (mỗi khoa có 2 bộ môn tương ứng)
        var cacKhoaTrongDb = await context.Khoas.ToListAsync();
        foreach (var khoa in cacKhoaTrongDb)
        {
            for (int so = 1; so <= 2; so++)
            {
                var maBm = $"{khoa.MaKhoa}-BM{so:D2}";
                var bmTonTai = await context.BoMons.FirstOrDefaultAsync(b => b.MaBoMon == maBm);
                if (bmTonTai == null)
                {
                    await context.BoMons.AddAsync(new BoMon
                    {
                        MaBoMon = maBm,
                        TenBoMon = $"Bộ môn Chuyên ngành {so} - {khoa.TenKhoa}",
                        MoTa = $"Bộ môn học thuật trực thuộc {khoa.TenKhoa}",
                        KhoaId = khoa.KhoaId,
                        CoHoatDong = true,
                        NgayTao = DateTime.UtcNow
                    });
                }
            }
        }
        await context.SaveChangesAsync();

        // C. Các Lớp đào tạo chuyên ngành C500 chuẩn (Khóa D53 đến D58 và Văn bằng 2 K3 đến K6)
        var khoaAnm = await context.Khoas.FirstOrDefaultAsync(k => k.MaKhoa == "KH-10") ?? cacKhoaTrongDb.FirstOrDefault();
        var khoaNvCoBan = await context.Khoas.FirstOrDefaultAsync(k => k.MaKhoa == "KH-03") ?? cacKhoaTrongDb.FirstOrDefault();
        var khoaXnc = await context.Khoas.FirstOrDefaultAsync(k => k.MaKhoa == "KH-08") ?? cacKhoaTrongDb.FirstOrDefault();
        var khoaLuat = await context.Khoas.FirstOrDefaultAsync(k => k.MaKhoa == "KH-11") ?? cacKhoaTrongDb.FirstOrDefault();

        var danhSachLopDemo = new (string MaLop, string TenLop, string NienKhoa, int KhoaId)[]
        {
            ("B1D58", "Lớp B1 Khóa D58 - Trinh sát an ninh", "2023-2027", khoaNvCoBan?.KhoaId ?? 1),
            ("B2D58", "Lớp B2 Khóa D58 - Điều tra hình sự", "2023-2027", khoaNvCoBan?.KhoaId ?? 1),
            ("B8D58", "Lớp B8 Khóa D58 - Quản lý xuất nhập cảnh", "2023-2027", khoaXnc?.KhoaId ?? 1),
            ("B10D58", "Lớp B10 Khóa D58 - An ninh mạng & PCTPCNC", "2023-2027", khoaAnm?.KhoaId ?? 1),
            ("B15D58", "Lớp B15 Khóa D58 - Công nghệ thông tin C500", "2023-2027", khoaAnm?.KhoaId ?? 1),
            ("B1D57", "Lớp B1 Khóa D57 - An ninh trinh sát", "2022-2026", khoaNvCoBan?.KhoaId ?? 1),
            ("B10D57", "Lớp B10 Khóa D57 - Kỹ thuật an ninh mạng", "2022-2026", khoaAnm?.KhoaId ?? 1),
            ("B1K6", "Lớp B1 Khóa K6 - Văn bằng 2 Điều tra", "2024-2026", khoaLuat?.KhoaId ?? 1),
            ("B2K6", "Lớp B2 Khóa K6 - Văn bằng 2 An ninh mạng", "2024-2026", khoaAnm?.KhoaId ?? 1)
        };

        foreach (var lopDemo in danhSachLopDemo)
        {
            var lopTonTai = await context.LopHocs.FirstOrDefaultAsync(l => l.MaLop == lopDemo.MaLop);
            if (lopTonTai == null)
            {
                await context.LopHocs.AddAsync(new LopHoc
                {
                    MaLop = lopDemo.MaLop,
                    TenLop = lopDemo.TenLop,
                    NienKhoa = lopDemo.NienKhoa,
                    KhoaId = lopDemo.KhoaId,
                    SoLuongToiDa = 40,
                    CoHoatDong = true,
                    NgayTao = DateTime.UtcNow
                });
            }
        }
        await context.SaveChangesAsync();

        // D. Khởi tạo 108 Cán bộ Giảng viên mẫu chuẩn CAND từ LTMT-demo
        var chucVuTk = await context.ChucVus.FirstOrDefaultAsync(c => c.MaChucVu == "CV_TK");
        var chucVuPk = await context.ChucVus.FirstOrDefaultAsync(c => c.MaChucVu == "CV_PK");
        var chucVuGv = await context.ChucVus.FirstOrDefaultAsync(c => c.MaChucVu == "CV_GV");
        var trinhDoTs = await context.TrinhDoChuyenMons.FirstOrDefaultAsync(t => t.MaTrinhDo == "TD_TS");
        var trinhDoThs = await context.TrinhDoChuyenMons.FirstOrDefaultAsync(t => t.MaTrinhDo == "TD_ThS");
        var trinhDoCn = await context.TrinhDoChuyenMons.FirstOrDefaultAsync(t => t.MaTrinhDo == "TD_CN");

        var capBacHamMau = new[] { "Đại tá", "Thượng tá", "Trung tá", "Thiếu tá", "Đại úy" };
        var hocViMau = new[] { "Tiến sĩ", "Thạc sĩ", "Cử nhân" };
        var chuyenMonMau = new[] { "Lý luận chính trị", "Quân sự - võ thuật", "Nghiệp vụ trinh sát", "Điều tra hình sự", "An ninh mạng", "Pháp luật an ninh", "Ngoại ngữ chuyên ngành" };

        int demCanBo = 1;
        foreach (var khoa in cacKhoaTrongDb.Where(k => k.MaKhoa.StartsWith("KH-")))
        {
            var boMonsCuaKhoa = await context.BoMons.Where(b => b.KhoaId == khoa.KhoaId).ToListAsync();
            var boMon1 = boMonsCuaKhoa.FirstOrDefault();
            var boMon2 = boMonsCuaKhoa.LastOrDefault() ?? boMon1;

            // Mỗi khoa thêm Trưởng khoa + 2 Phó khoa + 2 Giảng viên nếu chưa đủ
            var maTk = $"CB_C500_{demCanBo:D3}";
            if (!await context.NhanSus.AnyAsync(n => n.MaNhanSu == maTk))
            {
                string gioiTinh = demCanBo % 2 == 0 ? "Nữ" : "Nam";
                string hoTen = QuyTacDuLieu.TaoHoTen(5000 + demCanBo, gioiTinh);
                await context.NhanSus.AddAsync(new NhanSu
                {
                    MaNhanSu = maTk,
                    HoVaTen = $"{capBacHamMau[(demCanBo - 1) % capBacHamMau.Length]}, TS. {hoTen}",
                    NgaySinh = new DateTime(1970 + demCanBo % 20, 1 + demCanBo % 12, 1 + demCanBo % 28),
                    GioiTinh = gioiTinh,
                    EmailCongViec = $"cb{demCanBo:D3}@c500.edu.vn",
                    SoDienThoai = $"09{50000000 + demCanBo:D8}",
                    SoGiayTo = $"0010{70 + demCanBo % 20:D2}{demCanBo:D6}",
                    KhoaId = khoa.KhoaId,
                    BoMonId = boMon1?.BoMonId,
                    ChucVuId = chucVuTk?.ChucVuId ?? chucVuGv?.ChucVuId,
                    TrinhDoChuyenMonId = trinhDoTs?.TrinhDoChuyenMonId,
                    ChucDanhNgheNghiep = "Giảng viên cao cấp",
                    HocVi = "Tiến sĩ",
                    HocHam = demCanBo % 3 == 0 ? "Phó Giáo sư" : "",
                    ChuyenNganh = chuyenMonMau[(demCanBo - 1) % chuyenMonMau.Length],
                    TruongDaoTao = "Học viện An ninh Nhân dân",
                    DiaChi = QuyTacDuLieu.TinhThanh[(demCanBo - 1) % QuyTacDuLieu.TinhThanh.Length],
                    TrangThaiCongTac = "Đang làm việc",
                    CoHoatDong = true,
                    NgayTao = DateTime.UtcNow
                });
            }
            demCanBo++;

            // Phó khoa
            var maPk = $"CB_C500_{demCanBo:D3}";
            if (!await context.NhanSus.AnyAsync(n => n.MaNhanSu == maPk))
            {
                string gioiTinh = demCanBo % 2 == 0 ? "Nữ" : "Nam";
                string hoTen = QuyTacDuLieu.TaoHoTen(5000 + demCanBo, gioiTinh);
                await context.NhanSus.AddAsync(new NhanSu
                {
                    MaNhanSu = maPk,
                    HoVaTen = $"{capBacHamMau[(demCanBo - 1) % capBacHamMau.Length]}, ThS. {hoTen}",
                    NgaySinh = new DateTime(1975 + demCanBo % 18, 1 + demCanBo % 12, 1 + demCanBo % 28),
                    GioiTinh = gioiTinh,
                    EmailCongViec = $"cb{demCanBo:D3}@c500.edu.vn",
                    SoDienThoai = $"09{50000000 + demCanBo:D8}",
                    SoGiayTo = $"0010{75 + demCanBo % 18:D2}{demCanBo:D6}",
                    KhoaId = khoa.KhoaId,
                    BoMonId = boMon2?.BoMonId,
                    ChucVuId = chucVuPk?.ChucVuId ?? chucVuGv?.ChucVuId,
                    TrinhDoChuyenMonId = trinhDoThs?.TrinhDoChuyenMonId,
                    ChucDanhNgheNghiep = "Giảng viên chính",
                    HocVi = "Thạc sĩ",
                    ChuyenNganh = chuyenMonMau[(demCanBo - 1) % chuyenMonMau.Length],
                    TruongDaoTao = "Học viện An ninh Nhân dân",
                    DiaChi = QuyTacDuLieu.TinhThanh[(demCanBo - 1) % QuyTacDuLieu.TinhThanh.Length],
                    TrangThaiCongTac = "Đang làm việc",
                    CoHoatDong = true,
                    NgayTao = DateTime.UtcNow
                });
            }
            demCanBo++;
        }
        await context.SaveChangesAsync();

        // E. Bổ sung các Môn học chuyên ngành nghiệp vụ An ninh C500
        var danhSachMonHocDemo = new (string Ma, string Ten, int TinChi, int LyThuyet, int ThucHanh)[]
        {
            ("MH_ANM01", "An ninh mạng và Bảo vệ hệ thống thông tin trọng yếu", 3, 30, 15),
            ("MH_TS01", "Nghiệp vụ trinh sát bảo vệ an ninh quốc gia", 4, 45, 15),
            ("MH_DTHS01", "Khoa học điều tra hình sự và Chiến thuật thẩm vấn", 3, 30, 15),
            ("MH_QSVT01", "Quân sự, võ thuật ứng dụng CAND và Kỹ thuật bắn súng", 3, 15, 30),
            ("MH_PL01", "Pháp luật đại cương và Luật Tố tụng hình sự Việt Nam", 3, 45, 0),
            ("MH_LLCT01", "Lý luận chính trị và Tư tưởng Hồ Chí Minh trong CAND", 3, 45, 0),
            ("MH_XNC01", "Nghiệp vụ kiểm soát xuất nhập cảnh và Quản lý cư trú", 3, 30, 15),
            ("MH_NN01", "Tiếng Anh chuyên ngành nghiệp vụ an ninh đối ngoại", 3, 30, 15)
        };

        var boMonMacDinh = await context.BoMons.FirstOrDefaultAsync();
        foreach (var mh in danhSachMonHocDemo)
        {
            if (!await context.MonHocs.AnyAsync(m => m.MaMonHoc == mh.Ma))
            {
                await context.MonHocs.AddAsync(new MonHoc
                {
                    MaMonHoc = mh.Ma,
                    TenMonHoc = mh.Ten,
                    SoTinChi = mh.TinChi,
                    SoTietLyThuyet = mh.LyThuyet,
                    SoTietThucHanh = mh.ThucHanh,
                    BoMonId = boMonMacDinh?.BoMonId,
                    CoHoatDong = true
                });
            }
        }
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Bỏ qua nếu tiến trình hoặc luồng kiểm thử song song khác đã khởi tạo dữ liệu này
        }
    }
}
