using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Data;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.Models.QuanTri;
using System.Globalization;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Lớp triển khai dịch vụ Quản trị dữ liệu trực tuyến (Live Manual CRUD &amp; Custom Schema) cho tất cả các nhóm quản lý trong hệ thống.
/// </summary>
public class QuanTriDuLieuDichVu : IQuanTriDuLieuDichVu
{
    private readonly QuanLyDaoTaoDbContext _coSoDuLieu;
    private readonly INhatKyDichVu _nhatKyDichVu;

    /// <summary>
    /// Khởi tạo dịch vụ Quản trị dữ liệu với DbContext và Dịch vụ Nhật ký.
    /// </summary>
    public QuanTriDuLieuDichVu(QuanLyDaoTaoDbContext coSoDuLieu, INhatKyDichVu nhatKyDichVu)
    {
        ArgumentNullException.ThrowIfNull(coSoDuLieu);
        ArgumentNullException.ThrowIfNull(nhatKyDichVu);

        _coSoDuLieu = coSoDuLieu;
        _nhatKyDichVu = nhatKyDichVu;
    }

    /// <summary>
    /// Lấy danh sách toàn bộ 11 nhóm quản lý trong hệ thống kèm thống kê.
    /// </summary>
    public async Task<List<ThongTinBangHeThong>> LayDanhSachBangHeThongAsync(CancellationToken cancellationToken = default)
    {
        var demTruong = await _coSoDuLieu.TruongTuyChinhs
            .AsNoTracking()
            .Where(t => t.CoHoatDong)
            .GroupBy(t => t.TenBang)
            .Select(g => new { TenBang = g.Key, SoLuong = g.Count() })
            .ToDictionaryAsync(x => x.TenBang, x => x.SoLuong, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var danhSach = new List<ThongTinBangHeThong>
        {
            new() { TenBang = "Khoa", TenHienThi = "Khoa Đào Tạo", BieuTuong = "bi-bank", SoBanGhi = await _coSoDuLieu.Khoas.CountAsync(cancellationToken) },
            new() { TenBang = "BoMon", TenHienThi = "Bộ Môn Trực Thuộc", BieuTuong = "bi-diagram-2", SoBanGhi = await _coSoDuLieu.BoMons.CountAsync(cancellationToken) },
            new() { TenBang = "LopHoc", TenHienThi = "Lớp Học Đào Tạo", BieuTuong = "bi-people", SoBanGhi = await _coSoDuLieu.LopHocs.CountAsync(cancellationToken) },
            new() { TenBang = "HocVien", TenHienThi = "Học Viên", BieuTuong = "bi-person-badge", SoBanGhi = await _coSoDuLieu.HocViens.CountAsync(cancellationToken) },
            new() { TenBang = "NhanSu", TenHienThi = "Hồ Sơ Cán Bộ / Giảng Viên", BieuTuong = "bi-person-lines-fill", SoBanGhi = await _coSoDuLieu.NhanSus.CountAsync(cancellationToken) },
            new() { TenBang = "MonHoc", TenHienThi = "Môn Học Đào Tạo", BieuTuong = "bi-journal-bookmark", SoBanGhi = await _coSoDuLieu.MonHocs.CountAsync(cancellationToken) },
            new() { TenBang = "PhanCongGiangDay", TenHienThi = "Phân Công Giảng Dạy", BieuTuong = "bi-calendar-check", SoBanGhi = await _coSoDuLieu.PhanCongGiangDays.CountAsync(cancellationToken) },
            new() { TenBang = "ChucVu", TenHienThi = "Danh Mục Chức Vụ", BieuTuong = "bi-award", SoBanGhi = await _coSoDuLieu.ChucVus.CountAsync(cancellationToken) },
            new() { TenBang = "TrinhDoChuyenMon", TenHienThi = "Trình Độ Chuyên Môn", BieuTuong = "bi-mortarboard", SoBanGhi = await _coSoDuLieu.TrinhDoChuyenMons.CountAsync(cancellationToken) },
            new() { TenBang = "TaiKhoan", TenHienThi = "Tài Khoản Hệ Thống", BieuTuong = "bi-shield-lock", SoBanGhi = await _coSoDuLieu.Users.CountAsync(cancellationToken) },
            new() { TenBang = "NhatKyHeThong", TenHienThi = "Nhật Ký Kiểm Toán", BieuTuong = "bi-clock-history", SoBanGhi = await _coSoDuLieu.NhatKyHeThongs.CountAsync(cancellationToken) }
        };

        foreach (var item in danhSach)
        {
            if (demTruong.TryGetValue(item.TenBang, out int sl))
            {
                item.SoTruongTuyChinh = sl;
            }
        }

        return danhSach;
    }

    /// <summary>
    /// Chuẩn hóa tên bảng chuẩn trong hệ thống.
    /// </summary>
    public static string ChuanHoaTenBang(string tenBang)
    {
        if (string.IsNullOrWhiteSpace(tenBang))
        {
            return "Khoa";
        }

        return tenBang.Trim().ToLowerInvariant() switch
        {
            "khoa" => "Khoa",
            "bomon" => "BoMon",
            "lophoc" => "LopHoc",
            "hocvien" => "HocVien",
            "nhansu" => "NhanSu",
            "monhoc" => "MonHoc",
            "phanconggiangday" or "phancong" => "PhanCongGiangDay",
            "chucvu" => "ChucVu",
            "trinhdochuyenmon" or "trinhdo" => "TrinhDoChuyenMon",
            "taikhoan" => "TaiKhoan",
            "nhatkyhethong" or "nhatky" => "NhatKyHeThong",
            _ => "Khoa"
        };
    }

    /// <summary>
    /// Lấy toàn bộ cấu trúc cột và dữ liệu của một bảng để hiển thị và sửa trực tiếp.
    /// </summary>
    public async Task<DuLieuBangTrucTuyen> LayDuLieuBangTrucTuyenAsync(
        string tenBang,
        string? tuKhoa = null,
        CancellationToken cancellationToken = default)
    {
        var bangChuan = ChuanHoaTenBang(tenBang);
        var ketQua = new DuLieuBangTrucTuyen
        {
            TenBang = bangChuan
        };

        // Lấy danh sách khóa ngoại dùng chung
        var luaChonKhoa = await _coSoDuLieu.Khoas.AsNoTracking().OrderBy(k => k.MaKhoa)
            .Select(k => new LuaChonKhoaNgoai { GiaTri = k.KhoaId.ToString(), NhanHienThi = $"{k.TenKhoa} ({k.MaKhoa})" })
            .ToListAsync(cancellationToken);

        var luaChonBoMon = await _coSoDuLieu.BoMons.AsNoTracking().OrderBy(b => b.MaBoMon)
            .Select(b => new LuaChonKhoaNgoai { GiaTri = b.BoMonId.ToString(), NhanHienThi = $"{b.TenBoMon} ({b.MaBoMon})" })
            .ToListAsync(cancellationToken);

        var luaChonLop = await _coSoDuLieu.LopHocs.AsNoTracking().OrderBy(l => l.MaLop)
            .Select(l => new LuaChonKhoaNgoai { GiaTri = l.LopHocId.ToString(), NhanHienThi = $"{l.TenLop} ({l.MaLop})" })
            .ToListAsync(cancellationToken);

        var luaChonChucVu = await _coSoDuLieu.ChucVus.AsNoTracking().OrderBy(c => c.MaChucVu)
            .Select(c => new LuaChonKhoaNgoai { GiaTri = c.ChucVuId.ToString(), NhanHienThi = $"{c.TenChucVu} ({c.MaChucVu})" })
            .ToListAsync(cancellationToken);

        var luaChonTrinhDo = await _coSoDuLieu.TrinhDoChuyenMons.AsNoTracking().OrderBy(t => t.MaTrinhDo)
            .Select(t => new LuaChonKhoaNgoai { GiaTri = t.TrinhDoChuyenMonId.ToString(), NhanHienThi = $"{t.TenTrinhDo} ({t.MaTrinhDo})" })
            .ToListAsync(cancellationToken);

        var luaChonMonHoc = await _coSoDuLieu.MonHocs.AsNoTracking().OrderBy(m => m.MaMonHoc)
            .Select(m => new LuaChonKhoaNgoai { GiaTri = m.MonHocId.ToString(), NhanHienThi = $"{m.TenMonHoc} ({m.MaMonHoc})" })
            .ToListAsync(cancellationToken);

        var luaChonNhanSu = await _coSoDuLieu.NhanSus.AsNoTracking().OrderBy(n => n.MaNhanSu)
            .Select(n => new LuaChonKhoaNgoai { GiaTri = n.NhanSuId.ToString(), NhanHienThi = $"{n.HoVaTen} ({n.MaNhanSu})" })
            .ToListAsync(cancellationToken);

        var tkChuan = string.IsNullOrWhiteSpace(tuKhoa) ? null : tuKhoa.Trim();

        switch (bangChuan)
        {
            case "Khoa":
                ketQua.TenHienThi = "Khoa Đào Tạo";
                ketQua.TenKhoaChinh = "KhoaId";
                ketQua.DanhSachCot.AddRange(new[]
                {
                    new MoTaCotDuLieu { MaCot = "KhoaId", TenHienThi = "ID", KieuDuLieu = "SoNguyen", LaKhoaChinh = true },
                    new MoTaCotDuLieu { MaCot = "MaKhoa", TenHienThi = "Mã Khoa", KieuDuLieu = "VanBan", LaBatBuoc = true },
                    new MoTaCotDuLieu { MaCot = "TenKhoa", TenHienThi = "Tên Khoa", KieuDuLieu = "VanBan", LaBatBuoc = true },
                    new MoTaCotDuLieu { MaCot = "SoDienThoai", TenHienThi = "Số Điện Thoại", KieuDuLieu = "VanBan" },
                    new MoTaCotDuLieu { MaCot = "Email", TenHienThi = "Email", KieuDuLieu = "VanBan" },
                    new MoTaCotDuLieu { MaCot = "DiaDiem", TenHienThi = "Địa Điểm", KieuDuLieu = "VanBan" },
                    new MoTaCotDuLieu { MaCot = "MoTa", TenHienThi = "Mô Tả", KieuDuLieu = "VanBan" },
                    new MoTaCotDuLieu { MaCot = "CoHoatDong", TenHienThi = "Hoạt Động", KieuDuLieu = "Logic" }
                });

                var dsKhoa = await _coSoDuLieu.Khoas.AsNoTracking()
                    .Where(k => tkChuan == null || k.MaKhoa.Contains(tkChuan) || k.TenKhoa.Contains(tkChuan))
                    .OrderBy(k => k.MaKhoa)
                    .ToListAsync(cancellationToken);

                foreach (var k in dsKhoa)
                {
                    ketQua.DanhSachDong.Add(new Dictionary<string, string?>
                    {
                        ["KhoaId"] = k.KhoaId.ToString(),
                        ["MaKhoa"] = k.MaKhoa,
                        ["TenKhoa"] = k.TenKhoa,
                        ["SoDienThoai"] = k.SoDienThoai,
                        ["Email"] = k.Email,
                        ["DiaDiem"] = k.DiaDiem,
                        ["MoTa"] = k.MoTa,
                        ["CoHoatDong"] = k.CoHoatDong ? "true" : "false"
                    });
                }
                break;

            case "BoMon":
                ketQua.TenHienThi = "Bộ Môn Trực Thuộc";
                ketQua.TenKhoaChinh = "BoMonId";
                ketQua.DanhSachCot.AddRange(new[]
                {
                    new MoTaCotDuLieu { MaCot = "BoMonId", TenHienThi = "ID", KieuDuLieu = "SoNguyen", LaKhoaChinh = true },
                    new MoTaCotDuLieu { MaCot = "MaBoMon", TenHienThi = "Mã Bộ Môn", KieuDuLieu = "VanBan", LaBatBuoc = true },
                    new MoTaCotDuLieu { MaCot = "TenBoMon", TenHienThi = "Tên Bộ Môn", KieuDuLieu = "VanBan", LaBatBuoc = true },
                    new MoTaCotDuLieu { MaCot = "KhoaId", TenHienThi = "Khoa Chủ Quản", KieuDuLieu = "KhoaNgoai", LaBatBuoc = true, DanhSachLuaChon = luaChonKhoa },
                    new MoTaCotDuLieu { MaCot = "MoTa", TenHienThi = "Mô Tả", KieuDuLieu = "VanBan" },
                    new MoTaCotDuLieu { MaCot = "CoHoatDong", TenHienThi = "Hoạt Động", KieuDuLieu = "Logic" }
                });

                var dsBoMon = await _coSoDuLieu.BoMons.AsNoTracking()
                    .Where(b => tkChuan == null || b.MaBoMon.Contains(tkChuan) || b.TenBoMon.Contains(tkChuan))
                    .OrderBy(b => b.MaBoMon)
                    .ToListAsync(cancellationToken);

                foreach (var b in dsBoMon)
                {
                    ketQua.DanhSachDong.Add(new Dictionary<string, string?>
                    {
                        ["BoMonId"] = b.BoMonId.ToString(),
                        ["MaBoMon"] = b.MaBoMon,
                        ["TenBoMon"] = b.TenBoMon,
                        ["KhoaId"] = b.KhoaId.ToString(),
                        ["MoTa"] = b.MoTa,
                        ["CoHoatDong"] = b.CoHoatDong ? "true" : "false"
                    });
                }
                break;

            case "LopHoc":
                ketQua.TenHienThi = "Lớp Học Đào Tạo";
                ketQua.TenKhoaChinh = "LopHocId";
                ketQua.DanhSachCot.AddRange(new[]
                {
                    new MoTaCotDuLieu { MaCot = "LopHocId", TenHienThi = "ID", KieuDuLieu = "SoNguyen", LaKhoaChinh = true },
                    new MoTaCotDuLieu { MaCot = "MaLop", TenHienThi = "Mã Lớp", KieuDuLieu = "VanBan", LaBatBuoc = true },
                    new MoTaCotDuLieu { MaCot = "TenLop", TenHienThi = "Tên Lớp", KieuDuLieu = "VanBan", LaBatBuoc = true },
                    new MoTaCotDuLieu { MaCot = "NienKhoa", TenHienThi = "Niên Khóa", KieuDuLieu = "VanBan", LaBatBuoc = true },
                    new MoTaCotDuLieu { MaCot = "KhoaId", TenHienThi = "Khoa Quản Lý", KieuDuLieu = "KhoaNgoai", LaBatBuoc = true, DanhSachLuaChon = luaChonKhoa },
                    new MoTaCotDuLieu { MaCot = "SoLuongToiDa", TenHienThi = "Sĩ Số Tối Đa", KieuDuLieu = "SoNguyen" },
                    new MoTaCotDuLieu { MaCot = "CoHoatDong", TenHienThi = "Hoạt Động", KieuDuLieu = "Logic" }
                });

                var dsLop = await _coSoDuLieu.LopHocs.AsNoTracking()
                    .Where(l => tkChuan == null || l.MaLop.Contains(tkChuan) || l.TenLop.Contains(tkChuan) || l.NienKhoa.Contains(tkChuan))
                    .OrderBy(l => l.MaLop)
                    .ToListAsync(cancellationToken);

                foreach (var l in dsLop)
                {
                    ketQua.DanhSachDong.Add(new Dictionary<string, string?>
                    {
                        ["LopHocId"] = l.LopHocId.ToString(),
                        ["MaLop"] = l.MaLop,
                        ["TenLop"] = l.TenLop,
                        ["NienKhoa"] = l.NienKhoa,
                        ["KhoaId"] = l.KhoaId.ToString(),
                        ["SoLuongToiDa"] = l.SoLuongToiDa.ToString(),
                        ["CoHoatDong"] = l.CoHoatDong ? "true" : "false"
                    });
                }
                break;

            case "HocVien":
                ketQua.TenHienThi = "Học Viên Đào Tạo";
                ketQua.TenKhoaChinh = "HocVienId";
                ketQua.DanhSachCot.AddRange(new[]
                {
                    new MoTaCotDuLieu { MaCot = "HocVienId", TenHienThi = "ID", KieuDuLieu = "SoNguyen", LaKhoaChinh = true },
                    new MoTaCotDuLieu { MaCot = "MaHocVien", TenHienThi = "Mã Học Viên", KieuDuLieu = "VanBan", LaBatBuoc = true },
                    new MoTaCotDuLieu { MaCot = "HoVaTen", TenHienThi = "Họ và Tên", KieuDuLieu = "VanBan", LaBatBuoc = true },
                    new MoTaCotDuLieu { MaCot = "NgaySinh", TenHienThi = "Ngày Sinh", KieuDuLieu = "NgayThang", LaBatBuoc = true },
                    new MoTaCotDuLieu
                    {
                        MaCot = "GioiTinh",
                        TenHienThi = "Giới Tính",
                        KieuDuLieu = "DanhSach",
                        LaBatBuoc = true,
                        DanhSachLuaChon = new()
                        {
                            new() { GiaTri = "Nam", NhanHienThi = "Nam" },
                            new() { GiaTri = "Nữ", NhanHienThi = "Nữ" },
                            new() { GiaTri = "Khác", NhanHienThi = "Khác" }
                        }
                    },
                    new MoTaCotDuLieu { MaCot = "LopHocId", TenHienThi = "Lớp Học", KieuDuLieu = "KhoaNgoai", LaBatBuoc = true, DanhSachLuaChon = luaChonLop },
                    new MoTaCotDuLieu { MaCot = "Email", TenHienThi = "Email", KieuDuLieu = "VanBan" },
                    new MoTaCotDuLieu { MaCot = "SoDienThoai", TenHienThi = "Số Điện Thoại", KieuDuLieu = "VanBan" },
                    new MoTaCotDuLieu { MaCot = "DiaChi", TenHienThi = "Địa Chỉ", KieuDuLieu = "VanBan" },
                    new MoTaCotDuLieu { MaCot = "CoHoatDong", TenHienThi = "Đang Học", KieuDuLieu = "Logic" }
                });

                var dsHocVien = await _coSoDuLieu.HocViens.AsNoTracking()
                    .Where(h => tkChuan == null || h.MaHocVien.Contains(tkChuan) || h.HoVaTen.Contains(tkChuan))
                    .OrderBy(h => h.MaHocVien)
                    .ToListAsync(cancellationToken);

                foreach (var h in dsHocVien)
                {
                    ketQua.DanhSachDong.Add(new Dictionary<string, string?>
                    {
                        ["HocVienId"] = h.HocVienId.ToString(),
                        ["MaHocVien"] = h.MaHocVien,
                        ["HoVaTen"] = h.HoVaTen,
                        ["NgaySinh"] = h.NgaySinh.ToString("yyyy-MM-dd"),
                        ["GioiTinh"] = h.GioiTinh,
                        ["LopHocId"] = h.LopHocId.ToString(),
                        ["Email"] = h.Email,
                        ["SoDienThoai"] = h.SoDienThoai,
                        ["DiaChi"] = h.DiaChi,
                        ["CoHoatDong"] = h.CoHoatDong ? "true" : "false"
                    });
                }
                break;

            case "NhanSu":
                ketQua.TenHienThi = "Hồ Sơ Cán Bộ / Giảng Viên";
                ketQua.TenKhoaChinh = "NhanSuId";
                ketQua.DanhSachCot.AddRange(new[]
                {
                    new MoTaCotDuLieu { MaCot = "NhanSuId", TenHienThi = "ID", KieuDuLieu = "SoNguyen", LaKhoaChinh = true },
                    new MoTaCotDuLieu { MaCot = "MaNhanSu", TenHienThi = "Mã Cán Bộ", KieuDuLieu = "VanBan", LaBatBuoc = true },
                    new MoTaCotDuLieu { MaCot = "HoVaTen", TenHienThi = "Họ và Tên", KieuDuLieu = "VanBan", LaBatBuoc = true },
                    new MoTaCotDuLieu { MaCot = "NgaySinh", TenHienThi = "Ngày Sinh", KieuDuLieu = "NgayThang", LaBatBuoc = true },
                    new MoTaCotDuLieu
                    {
                        MaCot = "GioiTinh",
                        TenHienThi = "Giới Tính",
                        KieuDuLieu = "DanhSach",
                        LaBatBuoc = true,
                        DanhSachLuaChon = new()
                        {
                            new() { GiaTri = "Nam", NhanHienThi = "Nam" },
                            new() { GiaTri = "Nữ", NhanHienThi = "Nữ" },
                            new() { GiaTri = "Khác", NhanHienThi = "Khác" }
                        }
                    },
                    new MoTaCotDuLieu { MaCot = "EmailCongViec", TenHienThi = "Email Công Việc", KieuDuLieu = "VanBan", LaBatBuoc = true },
                    new MoTaCotDuLieu { MaCot = "SoDienThoai", TenHienThi = "Số Điện Thoại", KieuDuLieu = "VanBan" },
                    new MoTaCotDuLieu { MaCot = "SoGiayTo", TenHienThi = "Số CCCD/Hộ Chiếu", KieuDuLieu = "VanBan" },
                    new MoTaCotDuLieu { MaCot = "KhoaId", TenHienThi = "Khoa", KieuDuLieu = "KhoaNgoai", DanhSachLuaChon = luaChonKhoa },
                    new MoTaCotDuLieu { MaCot = "BoMonId", TenHienThi = "Bộ Môn", KieuDuLieu = "KhoaNgoai", DanhSachLuaChon = luaChonBoMon },
                    new MoTaCotDuLieu { MaCot = "ChucVuId", TenHienThi = "Chức Vụ", KieuDuLieu = "KhoaNgoai", DanhSachLuaChon = luaChonChucVu },
                    new MoTaCotDuLieu { MaCot = "TrinhDoChuyenMonId", TenHienThi = "Trình Độ", KieuDuLieu = "KhoaNgoai", DanhSachLuaChon = luaChonTrinhDo },
                    new MoTaCotDuLieu { MaCot = "ChucDanhNgheNghiep", TenHienThi = "Chức Danh", KieuDuLieu = "VanBan" },
                    new MoTaCotDuLieu { MaCot = "HocVi", TenHienThi = "Học Vị", KieuDuLieu = "VanBan" },
                    new MoTaCotDuLieu { MaCot = "HocHam", TenHienThi = "Học Hàm", KieuDuLieu = "VanBan" },
                    new MoTaCotDuLieu { MaCot = "ChuyenNganh", TenHienThi = "Chuyên Ngành", KieuDuLieu = "VanBan" },
                    new MoTaCotDuLieu { MaCot = "TruongDaoTao", TenHienThi = "Trường Đào Tạo", KieuDuLieu = "VanBan" },
                    new MoTaCotDuLieu
                    {
                        MaCot = "TrangThaiCongTac",
                        TenHienThi = "Trạng Thái Công Tác",
                        KieuDuLieu = "DanhSach",
                        DanhSachLuaChon = new()
                        {
                            new() { GiaTri = "Đang làm việc", NhanHienThi = "Đang làm việc" },
                            new() { GiaTri = "Tạm nghỉ", NhanHienThi = "Tạm nghỉ" },
                            new() { GiaTri = "Đã nghỉ việc", NhanHienThi = "Đã nghỉ việc" }
                        }
                    },
                    new MoTaCotDuLieu { MaCot = "CoHoatDong", TenHienThi = "Kích Hoạt", KieuDuLieu = "Logic" }
                });

                var dsNhanSu = await _coSoDuLieu.NhanSus.AsNoTracking()
                    .Where(n => tkChuan == null || n.MaNhanSu.Contains(tkChuan) || n.HoVaTen.Contains(tkChuan) || n.EmailCongViec.Contains(tkChuan))
                    .OrderBy(n => n.MaNhanSu)
                    .ToListAsync(cancellationToken);

                foreach (var n in dsNhanSu)
                {
                    ketQua.DanhSachDong.Add(new Dictionary<string, string?>
                    {
                        ["NhanSuId"] = n.NhanSuId.ToString(),
                        ["MaNhanSu"] = n.MaNhanSu,
                        ["HoVaTen"] = n.HoVaTen,
                        ["NgaySinh"] = n.NgaySinh.ToString("yyyy-MM-dd"),
                        ["GioiTinh"] = n.GioiTinh,
                        ["EmailCongViec"] = n.EmailCongViec,
                        ["SoDienThoai"] = n.SoDienThoai,
                        ["SoGiayTo"] = n.SoGiayTo,
                        ["KhoaId"] = n.KhoaId?.ToString() ?? "",
                        ["BoMonId"] = n.BoMonId?.ToString() ?? "",
                        ["ChucVuId"] = n.ChucVuId?.ToString() ?? "",
                        ["TrinhDoChuyenMonId"] = n.TrinhDoChuyenMonId?.ToString() ?? "",
                        ["ChucDanhNgheNghiep"] = n.ChucDanhNgheNghiep,
                        ["HocVi"] = n.HocVi,
                        ["HocHam"] = n.HocHam,
                        ["ChuyenNganh"] = n.ChuyenNganh,
                        ["TruongDaoTao"] = n.TruongDaoTao,
                        ["TrangThaiCongTac"] = n.TrangThaiCongTac,
                        ["CoHoatDong"] = n.CoHoatDong ? "true" : "false"
                    });
                }
                break;

            case "MonHoc":
                ketQua.TenHienThi = "Môn Học Đào Tạo";
                ketQua.TenKhoaChinh = "MonHocId";
                ketQua.DanhSachCot.AddRange(new[]
                {
                    new MoTaCotDuLieu { MaCot = "MonHocId", TenHienThi = "ID", KieuDuLieu = "SoNguyen", LaKhoaChinh = true },
                    new MoTaCotDuLieu { MaCot = "MaMonHoc", TenHienThi = "Mã Môn Học", KieuDuLieu = "VanBan", LaBatBuoc = true },
                    new MoTaCotDuLieu { MaCot = "TenMonHoc", TenHienThi = "Tên Môn Học", KieuDuLieu = "VanBan", LaBatBuoc = true },
                    new MoTaCotDuLieu { MaCot = "SoTinChi", TenHienThi = "Số Tín Chỉ", KieuDuLieu = "SoNguyen", LaBatBuoc = true },
                    new MoTaCotDuLieu { MaCot = "SoTietLyThuyet", TenHienThi = "Tiết Lý Thuyết", KieuDuLieu = "SoNguyen" },
                    new MoTaCotDuLieu { MaCot = "SoTietThucHanh", TenHienThi = "Tiết Thực Hành", KieuDuLieu = "SoNguyen" },
                    new MoTaCotDuLieu { MaCot = "BoMonId", TenHienThi = "Bộ Môn Phụ Trách", KieuDuLieu = "KhoaNgoai", DanhSachLuaChon = luaChonBoMon },
                    new MoTaCotDuLieu { MaCot = "CoHoatDong", TenHienThi = "Hoạt Động", KieuDuLieu = "Logic" }
                });

                var dsMonHoc = await _coSoDuLieu.MonHocs.AsNoTracking()
                    .Where(m => tkChuan == null || m.MaMonHoc.Contains(tkChuan) || m.TenMonHoc.Contains(tkChuan))
                    .OrderBy(m => m.MaMonHoc)
                    .ToListAsync(cancellationToken);

                foreach (var m in dsMonHoc)
                {
                    ketQua.DanhSachDong.Add(new Dictionary<string, string?>
                    {
                        ["MonHocId"] = m.MonHocId.ToString(),
                        ["MaMonHoc"] = m.MaMonHoc,
                        ["TenMonHoc"] = m.TenMonHoc,
                        ["SoTinChi"] = m.SoTinChi.ToString(),
                        ["SoTietLyThuyet"] = m.SoTietLyThuyet.ToString(),
                        ["SoTietThucHanh"] = m.SoTietThucHanh.ToString(),
                        ["BoMonId"] = m.BoMonId?.ToString() ?? "",
                        ["CoHoatDong"] = m.CoHoatDong ? "true" : "false"
                    });
                }
                break;

            case "PhanCongGiangDay":
                ketQua.TenHienThi = "Phân Công Giảng Dạy";
                ketQua.TenKhoaChinh = "PhanCongId";
                ketQua.DanhSachCot.AddRange(new[]
                {
                    new MoTaCotDuLieu { MaCot = "PhanCongId", TenHienThi = "ID", KieuDuLieu = "SoNguyen", LaKhoaChinh = true },
                    new MoTaCotDuLieu { MaCot = "MaPhanCong", TenHienThi = "Mã Phân Công", KieuDuLieu = "VanBan", LaBatBuoc = true },
                    new MoTaCotDuLieu { MaCot = "NhanSuId", TenHienThi = "Giảng Viên", KieuDuLieu = "KhoaNgoai", LaBatBuoc = true, DanhSachLuaChon = luaChonNhanSu },
                    new MoTaCotDuLieu { MaCot = "MonHocId", TenHienThi = "Môn Học", KieuDuLieu = "KhoaNgoai", LaBatBuoc = true, DanhSachLuaChon = luaChonMonHoc },
                    new MoTaCotDuLieu { MaCot = "LopHocId", TenHienThi = "Lớp Học", KieuDuLieu = "KhoaNgoai", LaBatBuoc = true, DanhSachLuaChon = luaChonLop },
                    new MoTaCotDuLieu
                    {
                        MaCot = "HocKy",
                        TenHienThi = "Học Kỳ",
                        KieuDuLieu = "DanhSach",
                        LaBatBuoc = true,
                        DanhSachLuaChon = new()
                        {
                            new() { GiaTri = "Học kỳ 1", NhanHienThi = "Học kỳ 1" },
                            new() { GiaTri = "Học kỳ 2", NhanHienThi = "Học kỳ 2" },
                            new() { GiaTri = "Học kỳ hè", NhanHienThi = "Học kỳ hè" }
                        }
                    },
                    new MoTaCotDuLieu { MaCot = "NamHoc", TenHienThi = "Năm Học", KieuDuLieu = "VanBan", LaBatBuoc = true },
                    new MoTaCotDuLieu
                    {
                        MaCot = "VaiTroGiangDay",
                        TenHienThi = "Vai Trò",
                        KieuDuLieu = "DanhSach",
                        DanhSachLuaChon = new()
                        {
                            new() { GiaTri = "Giảng viên chính", NhanHienThi = "Giảng viên chính" },
                            new() { GiaTri = "Trợ giảng", NhanHienThi = "Trợ giảng" },
                            new() { GiaTri = "Hướng dẫn thực hành", NhanHienThi = "Hướng dẫn thực hành" }
                        }
                    },
                    new MoTaCotDuLieu { MaCot = "LaChuNhiem", TenHienThi = "GV Chủ Nhiệm", KieuDuLieu = "Logic" },
                    new MoTaCotDuLieu { MaCot = "GhiChu", TenHienThi = "Ghi Chú", KieuDuLieu = "VanBan" }
                });

                var dsPhanCong = await _coSoDuLieu.PhanCongGiangDays.AsNoTracking()
                    .Where(p => tkChuan == null || p.MaPhanCong.Contains(tkChuan) || p.NamHoc.Contains(tkChuan))
                    .OrderByDescending(p => p.NgayTao)
                    .ToListAsync(cancellationToken);

                foreach (var p in dsPhanCong)
                {
                    ketQua.DanhSachDong.Add(new Dictionary<string, string?>
                    {
                        ["PhanCongId"] = p.PhanCongId.ToString(),
                        ["MaPhanCong"] = p.MaPhanCong,
                        ["NhanSuId"] = p.NhanSuId.ToString(),
                        ["MonHocId"] = p.MonHocId.ToString(),
                        ["LopHocId"] = p.LopHocId.ToString(),
                        ["HocKy"] = p.HocKy,
                        ["NamHoc"] = p.NamHoc,
                        ["VaiTroGiangDay"] = p.VaiTroGiangDay,
                        ["LaChuNhiem"] = p.LaChuNhiem ? "true" : "false",
                        ["GhiChu"] = p.GhiChu
                    });
                }
                break;

            case "ChucVu":
                ketQua.TenHienThi = "Danh Mục Chức Vụ";
                ketQua.TenKhoaChinh = "ChucVuId";
                ketQua.DanhSachCot.AddRange(new[]
                {
                    new MoTaCotDuLieu { MaCot = "ChucVuId", TenHienThi = "ID", KieuDuLieu = "SoNguyen", LaKhoaChinh = true },
                    new MoTaCotDuLieu { MaCot = "MaChucVu", TenHienThi = "Mã Chức Vụ", KieuDuLieu = "VanBan", LaBatBuoc = true },
                    new MoTaCotDuLieu { MaCot = "TenChucVu", TenHienThi = "Tên Chức Vụ", KieuDuLieu = "VanBan", LaBatBuoc = true },
                    new MoTaCotDuLieu { MaCot = "MoTa", TenHienThi = "Mô Tả", KieuDuLieu = "VanBan" },
                    new MoTaCotDuLieu { MaCot = "CoHoatDong", TenHienThi = "Hoạt Động", KieuDuLieu = "Logic" }
                });

                var dsChucVu = await _coSoDuLieu.ChucVus.AsNoTracking()
                    .Where(c => tkChuan == null || c.MaChucVu.Contains(tkChuan) || c.TenChucVu.Contains(tkChuan))
                    .OrderBy(c => c.MaChucVu)
                    .ToListAsync(cancellationToken);

                foreach (var c in dsChucVu)
                {
                    ketQua.DanhSachDong.Add(new Dictionary<string, string?>
                    {
                        ["ChucVuId"] = c.ChucVuId.ToString(),
                        ["MaChucVu"] = c.MaChucVu,
                        ["TenChucVu"] = c.TenChucVu,
                        ["MoTa"] = c.MoTa,
                        ["CoHoatDong"] = c.CoHoatDong ? "true" : "false"
                    });
                }
                break;

            case "TrinhDoChuyenMon":
                ketQua.TenHienThi = "Trình Độ Chuyên Môn";
                ketQua.TenKhoaChinh = "TrinhDoChuyenMonId";
                ketQua.DanhSachCot.AddRange(new[]
                {
                    new MoTaCotDuLieu { MaCot = "TrinhDoChuyenMonId", TenHienThi = "ID", KieuDuLieu = "SoNguyen", LaKhoaChinh = true },
                    new MoTaCotDuLieu { MaCot = "MaTrinhDo", TenHienThi = "Mã Trình Độ", KieuDuLieu = "VanBan", LaBatBuoc = true },
                    new MoTaCotDuLieu { MaCot = "TenTrinhDo", TenHienThi = "Tên Trình Độ", KieuDuLieu = "VanBan", LaBatBuoc = true },
                    new MoTaCotDuLieu { MaCot = "MoTa", TenHienThi = "Mô Tả", KieuDuLieu = "VanBan" },
                    new MoTaCotDuLieu { MaCot = "CoHoatDong", TenHienThi = "Hoạt Động", KieuDuLieu = "Logic" }
                });

                var dsTrinhDo = await _coSoDuLieu.TrinhDoChuyenMons.AsNoTracking()
                    .Where(t => tkChuan == null || t.MaTrinhDo.Contains(tkChuan) || t.TenTrinhDo.Contains(tkChuan))
                    .OrderBy(t => t.MaTrinhDo)
                    .ToListAsync(cancellationToken);

                foreach (var t in dsTrinhDo)
                {
                    ketQua.DanhSachDong.Add(new Dictionary<string, string?>
                    {
                        ["TrinhDoChuyenMonId"] = t.TrinhDoChuyenMonId.ToString(),
                        ["MaTrinhDo"] = t.MaTrinhDo,
                        ["TenTrinhDo"] = t.TenTrinhDo,
                        ["MoTa"] = t.MoTa,
                        ["CoHoatDong"] = t.CoHoatDong ? "true" : "false"
                    });
                }
                break;

            case "TaiKhoan":
                ketQua.TenHienThi = "Tài Khoản & Phân Quyền Hệ Thống";
                ketQua.TenKhoaChinh = "Id";
                ketQua.DanhSachCot.AddRange(new[]
                {
                    new MoTaCotDuLieu { MaCot = "Id", TenHienThi = "ID", KieuDuLieu = "VanBan", LaKhoaChinh = true },
                    new MoTaCotDuLieu { MaCot = "Email", TenHienThi = "Email Đăng Nhập", KieuDuLieu = "VanBan", LaBatBuoc = true },
                    new MoTaCotDuLieu { MaCot = "HoVaTen", TenHienThi = "Họ và Tên", KieuDuLieu = "VanBan", LaBatBuoc = true },
                    new MoTaCotDuLieu { MaCot = "MaNhanSuLienKet", TenHienThi = "Mã Nhân Sự Liên Kết", KieuDuLieu = "VanBan" },
                    new MoTaCotDuLieu
                    {
                        MaCot = "VaiTro",
                        TenHienThi = "Vai Trò Phân Quyền",
                        KieuDuLieu = "DanhSach",
                        LaBatBuoc = true,
                        DanhSachLuaChon = new()
                        {
                            new() { GiaTri = VaiTroMacDinh.QuanTriVien, NhanHienThi = "Quản trị viên (Admin)" },
                            new() { GiaTri = VaiTroMacDinh.QuanLyDaoTao, NhanHienThi = "Quản lý đào tạo (DataManager)" },
                            new() { GiaTri = VaiTroMacDinh.NguoiXem, NhanHienThi = "Người xem (Viewer)" }
                        }
                    },
                    new MoTaCotDuLieu { MaCot = "SoDienThoai", TenHienThi = "Số Điện Thoại", KieuDuLieu = "VanBan" },
                    new MoTaCotDuLieu { MaCot = "CoHoatDong", TenHienThi = "Hoạt Động", KieuDuLieu = "Logic" }
                });

                var dsUser = await _coSoDuLieu.Users.AsNoTracking()
                    .Where(u => tkChuan == null || (u.Email != null && u.Email.Contains(tkChuan)) || u.HoVaTen.Contains(tkChuan))
                    .OrderByDescending(u => u.NgayTao)
                    .ToListAsync(cancellationToken);

                var mapRoles = await _coSoDuLieu.Roles.AsNoTracking()
                    .ToDictionaryAsync(r => r.Id, r => r.Name ?? VaiTroMacDinh.NguoiXem, cancellationToken);
                var dsUserRoles = await _coSoDuLieu.UserRoles.AsNoTracking().ToListAsync(cancellationToken);

                foreach (var u in dsUser)
                {
                    var roleId = dsUserRoles.FirstOrDefault(ur => ur.UserId == u.Id)?.RoleId;
                    var roleName = roleId != null && mapRoles.TryGetValue(roleId, out var rn) ? rn : VaiTroMacDinh.NguoiXem;

                    ketQua.DanhSachDong.Add(new Dictionary<string, string?>
                    {
                        ["Id"] = u.Id,
                        ["Email"] = u.Email,
                        ["HoVaTen"] = u.HoVaTen,
                        ["MaNhanSuLienKet"] = u.MaNhanSuLienKet,
                        ["VaiTro"] = roleName,
                        ["SoDienThoai"] = u.PhoneNumber,
                        ["CoHoatDong"] = u.CoHoatDong ? "true" : "false"
                    });
                }
                break;

            case "NhatKyHeThong":
                ketQua.TenHienThi = "Nhật Ký Kiểm Toán Hệ Thống";
                ketQua.TenKhoaChinh = "NhatKyId";
                ketQua.DanhSachCot.AddRange(new[]
                {
                    new MoTaCotDuLieu { MaCot = "NhatKyId", TenHienThi = "ID", KieuDuLieu = "SoNguyen", LaKhoaChinh = true },
                    new MoTaCotDuLieu { MaCot = "NguoiThucHien", TenHienThi = "Người Thực Hiện", KieuDuLieu = "VanBan", LaBatBuoc = true },
                    new MoTaCotDuLieu { MaCot = "HanhDong", TenHienThi = "Hành Động", KieuDuLieu = "VanBan", LaBatBuoc = true },
                    new MoTaCotDuLieu { MaCot = "BangDuLieu", TenHienThi = "Bảng Dữ Liệu", KieuDuLieu = "VanBan", LaBatBuoc = true },
                    new MoTaCotDuLieu { MaCot = "KhoaBanGhi", TenHienThi = "Khóa Bản Ghi", KieuDuLieu = "VanBan" },
                    new MoTaCotDuLieu { MaCot = "GiaTriMoi", TenHienThi = "Nội Dung Chi Tiết", KieuDuLieu = "VanBan" },
                    new MoTaCotDuLieu
                    {
                        MaCot = "KetQua",
                        TenHienThi = "Kết Quả",
                        KieuDuLieu = "DanhSach",
                        DanhSachLuaChon = new()
                        {
                            new() { GiaTri = "Thành công", NhanHienThi = "Thành công" },
                            new() { GiaTri = "Thất bại", NhanHienThi = "Thất bại" }
                        }
                    }
                });

                var dsNhatKy = await _coSoDuLieu.NhatKyHeThongs.AsNoTracking()
                    .Where(nk => tkChuan == null || nk.NguoiThucHien.Contains(tkChuan) || nk.HanhDong.Contains(tkChuan) || nk.BangDuLieu.Contains(tkChuan))
                    .OrderByDescending(nk => nk.ThoiDiem)
                    .Take(150)
                    .ToListAsync(cancellationToken);

                foreach (var nk in dsNhatKy)
                {
                    ketQua.DanhSachDong.Add(new Dictionary<string, string?>
                    {
                        ["NhatKyId"] = nk.NhatKyId.ToString(),
                        ["NguoiThucHien"] = nk.NguoiThucHien,
                        ["HanhDong"] = nk.HanhDong,
                        ["BangDuLieu"] = nk.BangDuLieu,
                        ["KhoaBanGhi"] = nk.KhoaBanGhi,
                        ["GiaTriMoi"] = nk.GiaTriMoi,
                        ["KetQua"] = nk.KetQua
                    });
                }
                break;
        }

        // Bổ sung các cột Trường Tùy Chỉnh (Custom Fields) vào bảng
        var dsTruongTuyChinh = await _coSoDuLieu.TruongTuyChinhs.AsNoTracking()
            .Where(t => t.TenBang == bangChuan && t.CoHoatDong)
            .OrderBy(t => t.ThuTuHienThi)
            .ThenBy(t => t.TruongTuyChinhId)
            .ToListAsync(cancellationToken);

        ketQua.DanhSachTruongTuyChinh = dsTruongTuyChinh;

        foreach (var ttc in dsTruongTuyChinh)
        {
            var maCotCustom = $"Custom_{ttc.MaTruong}";
            var luaChonCustom = new List<LuaChonKhoaNgoai>();
            if (ttc.KieuDuLieu == "DanhSach" && !string.IsNullOrWhiteSpace(ttc.TuyChonDanhSach))
            {
                foreach (var opt in ttc.TuyChonDanhSach.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    luaChonCustom.Add(new LuaChonKhoaNgoai { GiaTri = opt, NhanHienThi = opt });
                }
            }

            ketQua.DanhSachCot.Add(new MoTaCotDuLieu
            {
                MaCot = maCotCustom,
                TenHienThi = ttc.TenHienThi,
                KieuDuLieu = ttc.KieuDuLieu,
                LaBatBuoc = ttc.LaBatBuoc,
                LaTruongTuyChinh = true,
                TruongTuyChinhId = ttc.TruongTuyChinhId,
                DanhSachLuaChon = luaChonCustom
            });
        }

        // Gắn giá trị của các Trường Tùy Chỉnh vào từng dòng dữ liệu
        if (dsTruongTuyChinh.Count > 0 && ketQua.DanhSachDong.Count > 0)
        {
            var cacGiaTriCustom = await _coSoDuLieu.GiaTriTruongTuyChinhs.AsNoTracking()
                .Where(g => g.TenBang == bangChuan)
                .ToListAsync(cancellationToken);

            var mapIdSangMaCot = dsTruongTuyChinh.ToDictionary(t => t.TruongTuyChinhId, t => $"Custom_{t.MaTruong}");

            foreach (var dong in ketQua.DanhSachDong)
            {
                var idBanGhi = dong.TryGetValue(ketQua.TenKhoaChinh, out var pkVal) ? pkVal ?? "" : "";
                // Khởi tạo giá trị mặc định
                foreach (var ttc in dsTruongTuyChinh)
                {
                    dong[$"Custom_{ttc.MaTruong}"] = ttc.GiaTriMacDinh ?? "";
                }

                // Điền giá trị đã lưu trong DB
                foreach (var gt in cacGiaTriCustom.Where(x => x.KhoaBanGhi == idBanGhi))
                {
                    if (mapIdSangMaCot.TryGetValue(gt.TruongTuyChinhId, out var tenCotCustom))
                    {
                        dong[tenCotCustom] = gt.GiaTri ?? "";
                    }
                }
            }
        }

        return ketQua;
    }

    /// <summary>
    /// Cập nhật trực tiếp một bản ghi (gồm cả trường gốc trong DB và trường tùy chỉnh) ngay lập tức.
    /// </summary>
    public async Task<bool> CapNhatNhanhBanGhiAsync(
        string tenBang,
        string khoaBanGhi,
        Dictionary<string, string?> duLieuCapNhat,
        string? nguoiThucHien = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(duLieuCapNhat);
        if (string.IsNullOrWhiteSpace(khoaBanGhi))
        {
            throw new ArgumentException("Khóa bản ghi không được để trống.", nameof(khoaBanGhi));
        }

        var bangChuan = ChuanHoaTenBang(tenBang);
        int.TryParse(khoaBanGhi, out int idSo);

        switch (bangChuan)
        {
            case "Khoa":
                var khoa = await _coSoDuLieu.Khoas.FirstOrDefaultAsync(k => k.KhoaId == idSo, cancellationToken)
                    ?? throw new KeyNotFoundException($"Không tìm thấy Khoa có ID {idSo}.");

                var maKhoaMoi = LayChuoi(duLieuCapNhat, "MaKhoa", khoa.MaKhoa);
                var tenKhoaMoi = LayChuoi(duLieuCapNhat, "TenKhoa", khoa.TenKhoa);
                if (string.IsNullOrWhiteSpace(maKhoaMoi) || string.IsNullOrWhiteSpace(tenKhoaMoi))
                {
                    throw new InvalidOperationException("Mã khoa và Tên khoa không được để trống.");
                }
                if (await _coSoDuLieu.Khoas.AnyAsync(k => k.MaKhoa == maKhoaMoi && k.KhoaId != idSo, cancellationToken))
                {
                    throw new InvalidOperationException($"Mã khoa '{maKhoaMoi}' đã tồn tại ở bản ghi khác.");
                }

                khoa.MaKhoa = maKhoaMoi;
                khoa.TenKhoa = tenKhoaMoi;
                khoa.SoDienThoai = LayChuoiHoacNull(duLieuCapNhat, "SoDienThoai", khoa.SoDienThoai);
                khoa.Email = LayChuoiHoacNull(duLieuCapNhat, "Email", khoa.Email);
                khoa.DiaDiem = LayChuoiHoacNull(duLieuCapNhat, "DiaDiem", khoa.DiaDiem);
                khoa.MoTa = LayChuoiHoacNull(duLieuCapNhat, "MoTa", khoa.MoTa);
                khoa.CoHoatDong = LayBoolean(duLieuCapNhat, "CoHoatDong", khoa.CoHoatDong);
                break;

            case "BoMon":
                var boMon = await _coSoDuLieu.BoMons.FirstOrDefaultAsync(b => b.BoMonId == idSo, cancellationToken)
                    ?? throw new KeyNotFoundException($"Không tìm thấy Bộ môn có ID {idSo}.");

                var maBmMoi = LayChuoi(duLieuCapNhat, "MaBoMon", boMon.MaBoMon);
                var tenBmMoi = LayChuoi(duLieuCapNhat, "TenBoMon", boMon.TenBoMon);
                if (string.IsNullOrWhiteSpace(maBmMoi) || string.IsNullOrWhiteSpace(tenBmMoi))
                {
                    throw new InvalidOperationException("Mã bộ môn và Tên bộ môn không được để trống.");
                }
                if (await _coSoDuLieu.BoMons.AnyAsync(b => b.MaBoMon == maBmMoi && b.BoMonId != idSo, cancellationToken))
                {
                    throw new InvalidOperationException($"Mã bộ môn '{maBmMoi}' đã tồn tại ở bản ghi khác.");
                }

                boMon.MaBoMon = maBmMoi;
                boMon.TenBoMon = tenBmMoi;
                boMon.KhoaId = LaySoNguyen(duLieuCapNhat, "KhoaId", boMon.KhoaId);
                boMon.MoTa = LayChuoiHoacNull(duLieuCapNhat, "MoTa", boMon.MoTa);
                boMon.CoHoatDong = LayBoolean(duLieuCapNhat, "CoHoatDong", boMon.CoHoatDong);
                break;

            case "LopHoc":
                var lop = await _coSoDuLieu.LopHocs.FirstOrDefaultAsync(l => l.LopHocId == idSo, cancellationToken)
                    ?? throw new KeyNotFoundException($"Không tìm thấy Lớp học có ID {idSo}.");

                var maLopMoi = LayChuoi(duLieuCapNhat, "MaLop", lop.MaLop);
                var tenLopMoi = LayChuoi(duLieuCapNhat, "TenLop", lop.TenLop);
                if (string.IsNullOrWhiteSpace(maLopMoi) || string.IsNullOrWhiteSpace(tenLopMoi))
                {
                    throw new InvalidOperationException("Mã lớp và Tên lớp không được để trống.");
                }
                if (await _coSoDuLieu.LopHocs.AnyAsync(l => l.MaLop == maLopMoi && l.LopHocId != idSo, cancellationToken))
                {
                    throw new InvalidOperationException($"Mã lớp '{maLopMoi}' đã tồn tại ở bản ghi khác.");
                }

                lop.MaLop = maLopMoi;
                lop.TenLop = tenLopMoi;
                lop.NienKhoa = LayChuoi(duLieuCapNhat, "NienKhoa", lop.NienKhoa);
                lop.KhoaId = LaySoNguyen(duLieuCapNhat, "KhoaId", lop.KhoaId);
                lop.SoLuongToiDa = LaySoNguyen(duLieuCapNhat, "SoLuongToiDa", lop.SoLuongToiDa);
                lop.CoHoatDong = LayBoolean(duLieuCapNhat, "CoHoatDong", lop.CoHoatDong);
                break;

            case "HocVien":
                var hv = await _coSoDuLieu.HocViens.FirstOrDefaultAsync(h => h.HocVienId == idSo, cancellationToken)
                    ?? throw new KeyNotFoundException($"Không tìm thấy Học viên có ID {idSo}.");

                var maHvMoi = LayChuoi(duLieuCapNhat, "MaHocVien", hv.MaHocVien);
                var hoTenHvMoi = LayChuoi(duLieuCapNhat, "HoVaTen", hv.HoVaTen);
                if (string.IsNullOrWhiteSpace(maHvMoi) || string.IsNullOrWhiteSpace(hoTenHvMoi))
                {
                    throw new InvalidOperationException("Mã học viên và Họ tên không được để trống.");
                }
                if (await _coSoDuLieu.HocViens.AnyAsync(h => h.MaHocVien == maHvMoi && h.HocVienId != idSo, cancellationToken))
                {
                    throw new InvalidOperationException($"Mã học viên '{maHvMoi}' đã tồn tại ở bản ghi khác.");
                }

                hv.MaHocVien = maHvMoi;
                hv.HoVaTen = hoTenHvMoi;
                hv.NgaySinh = LayNgayThang(duLieuCapNhat, "NgaySinh", hv.NgaySinh);
                hv.GioiTinh = LayChuoi(duLieuCapNhat, "GioiTinh", hv.GioiTinh);
                hv.LopHocId = LaySoNguyen(duLieuCapNhat, "LopHocId", hv.LopHocId);
                hv.Email = LayChuoiHoacNull(duLieuCapNhat, "Email", hv.Email);
                hv.SoDienThoai = LayChuoiHoacNull(duLieuCapNhat, "SoDienThoai", hv.SoDienThoai);
                hv.DiaChi = LayChuoiHoacNull(duLieuCapNhat, "DiaChi", hv.DiaChi);
                hv.CoHoatDong = LayBoolean(duLieuCapNhat, "CoHoatDong", hv.CoHoatDong);
                break;

            case "NhanSu":
                var ns = await _coSoDuLieu.NhanSus.FirstOrDefaultAsync(n => n.NhanSuId == idSo, cancellationToken)
                    ?? throw new KeyNotFoundException($"Không tìm thấy Nhân sự có ID {idSo}.");

                var maNsMoi = LayChuoi(duLieuCapNhat, "MaNhanSu", ns.MaNhanSu);
                var hoTenNsMoi = LayChuoi(duLieuCapNhat, "HoVaTen", ns.HoVaTen);
                var emailNsMoi = LayChuoi(duLieuCapNhat, "EmailCongViec", ns.EmailCongViec);
                if (string.IsNullOrWhiteSpace(maNsMoi) || string.IsNullOrWhiteSpace(hoTenNsMoi) || string.IsNullOrWhiteSpace(emailNsMoi))
                {
                    throw new InvalidOperationException("Mã nhân sự, Họ tên và Email công việc là bắt buộc.");
                }
                if (await _coSoDuLieu.NhanSus.AnyAsync(n => n.MaNhanSu == maNsMoi && n.NhanSuId != idSo, cancellationToken))
                {
                    throw new InvalidOperationException($"Mã nhân sự '{maNsMoi}' đã tồn tại ở bản ghi khác.");
                }

                ns.MaNhanSu = maNsMoi;
                ns.HoVaTen = hoTenNsMoi;
                ns.NgaySinh = LayNgayThang(duLieuCapNhat, "NgaySinh", ns.NgaySinh);
                ns.GioiTinh = LayChuoi(duLieuCapNhat, "GioiTinh", ns.GioiTinh);
                ns.EmailCongViec = emailNsMoi;
                ns.SoDienThoai = LayChuoiHoacNull(duLieuCapNhat, "SoDienThoai", ns.SoDienThoai);
                ns.SoGiayTo = LayChuoiHoacNull(duLieuCapNhat, "SoGiayTo", ns.SoGiayTo);
                ns.KhoaId = LaySoNguyenHoacNull(duLieuCapNhat, "KhoaId", ns.KhoaId);
                ns.BoMonId = LaySoNguyenHoacNull(duLieuCapNhat, "BoMonId", ns.BoMonId);
                ns.ChucVuId = LaySoNguyenHoacNull(duLieuCapNhat, "ChucVuId", ns.ChucVuId);
                ns.TrinhDoChuyenMonId = LaySoNguyenHoacNull(duLieuCapNhat, "TrinhDoChuyenMonId", ns.TrinhDoChuyenMonId);
                ns.ChucDanhNgheNghiep = LayChuoiHoacNull(duLieuCapNhat, "ChucDanhNgheNghiep", ns.ChucDanhNgheNghiep);
                ns.HocVi = LayChuoiHoacNull(duLieuCapNhat, "HocVi", ns.HocVi);
                ns.HocHam = LayChuoiHoacNull(duLieuCapNhat, "HocHam", ns.HocHam);
                ns.ChuyenNganh = LayChuoiHoacNull(duLieuCapNhat, "ChuyenNganh", ns.ChuyenNganh);
                ns.TruongDaoTao = LayChuoiHoacNull(duLieuCapNhat, "TruongDaoTao", ns.TruongDaoTao);
                ns.TrangThaiCongTac = LayChuoi(duLieuCapNhat, "TrangThaiCongTac", ns.TrangThaiCongTac);
                ns.CoHoatDong = LayBoolean(duLieuCapNhat, "CoHoatDong", ns.CoHoatDong);
                ns.NgayCapNhat = DateTime.UtcNow;
                ns.NguoiCapNhat = nguoiThucHien;
                break;

            case "MonHoc":
                var mh = await _coSoDuLieu.MonHocs.FirstOrDefaultAsync(m => m.MonHocId == idSo, cancellationToken)
                    ?? throw new KeyNotFoundException($"Không tìm thấy Môn học có ID {idSo}.");

                var maMhMoi = LayChuoi(duLieuCapNhat, "MaMonHoc", mh.MaMonHoc);
                var tenMhMoi = LayChuoi(duLieuCapNhat, "TenMonHoc", mh.TenMonHoc);
                if (string.IsNullOrWhiteSpace(maMhMoi) || string.IsNullOrWhiteSpace(tenMhMoi))
                {
                    throw new InvalidOperationException("Mã môn học và Tên môn học không được để trống.");
                }
                if (await _coSoDuLieu.MonHocs.AnyAsync(m => m.MaMonHoc == maMhMoi && m.MonHocId != idSo, cancellationToken))
                {
                    throw new InvalidOperationException($"Mã môn học '{maMhMoi}' đã tồn tại ở bản ghi khác.");
                }

                mh.MaMonHoc = maMhMoi;
                mh.TenMonHoc = tenMhMoi;
                mh.SoTinChi = LaySoNguyen(duLieuCapNhat, "SoTinChi", mh.SoTinChi);
                mh.SoTietLyThuyet = LaySoNguyen(duLieuCapNhat, "SoTietLyThuyet", mh.SoTietLyThuyet);
                mh.SoTietThucHanh = LaySoNguyen(duLieuCapNhat, "SoTietThucHanh", mh.SoTietThucHanh);
                mh.BoMonId = LaySoNguyenHoacNull(duLieuCapNhat, "BoMonId", mh.BoMonId);
                mh.CoHoatDong = LayBoolean(duLieuCapNhat, "CoHoatDong", mh.CoHoatDong);
                break;

            case "PhanCongGiangDay":
                var pc = await _coSoDuLieu.PhanCongGiangDays.FirstOrDefaultAsync(p => p.PhanCongId == idSo, cancellationToken)
                    ?? throw new KeyNotFoundException($"Không tìm thấy Phân công có ID {idSo}.");

                var maPcMoi = LayChuoi(duLieuCapNhat, "MaPhanCong", pc.MaPhanCong);
                if (string.IsNullOrWhiteSpace(maPcMoi))
                {
                    throw new InvalidOperationException("Mã phân công không được để trống.");
                }
                if (await _coSoDuLieu.PhanCongGiangDays.AnyAsync(p => p.MaPhanCong == maPcMoi && p.PhanCongId != idSo, cancellationToken))
                {
                    throw new InvalidOperationException($"Mã phân công '{maPcMoi}' đã tồn tại ở bản ghi khác.");
                }

                pc.MaPhanCong = maPcMoi;
                pc.NhanSuId = LaySoNguyen(duLieuCapNhat, "NhanSuId", pc.NhanSuId);
                pc.MonHocId = LaySoNguyen(duLieuCapNhat, "MonHocId", pc.MonHocId);
                pc.LopHocId = LaySoNguyen(duLieuCapNhat, "LopHocId", pc.LopHocId);
                pc.HocKy = LayChuoi(duLieuCapNhat, "HocKy", pc.HocKy);
                pc.NamHoc = LayChuoi(duLieuCapNhat, "NamHoc", pc.NamHoc);
                pc.VaiTroGiangDay = LayChuoi(duLieuCapNhat, "VaiTroGiangDay", pc.VaiTroGiangDay);
                pc.LaChuNhiem = LayBoolean(duLieuCapNhat, "LaChuNhiem", pc.LaChuNhiem);
                pc.GhiChu = LayChuoiHoacNull(duLieuCapNhat, "GhiChu", pc.GhiChu);
                break;

            case "ChucVu":
                var cv = await _coSoDuLieu.ChucVus.FirstOrDefaultAsync(c => c.ChucVuId == idSo, cancellationToken)
                    ?? throw new KeyNotFoundException($"Không tìm thấy Chức vụ có ID {idSo}.");

                var maCvMoi = LayChuoi(duLieuCapNhat, "MaChucVu", cv.MaChucVu);
                var tenCvMoi = LayChuoi(duLieuCapNhat, "TenChucVu", cv.TenChucVu);
                if (string.IsNullOrWhiteSpace(maCvMoi) || string.IsNullOrWhiteSpace(tenCvMoi))
                {
                    throw new InvalidOperationException("Mã chức vụ và Tên chức vụ không được để trống.");
                }
                if (await _coSoDuLieu.ChucVus.AnyAsync(c => c.MaChucVu == maCvMoi && c.ChucVuId != idSo, cancellationToken))
                {
                    throw new InvalidOperationException($"Mã chức vụ '{maCvMoi}' đã tồn tại ở bản ghi khác.");
                }

                cv.MaChucVu = maCvMoi;
                cv.TenChucVu = tenCvMoi;
                cv.MoTa = LayChuoiHoacNull(duLieuCapNhat, "MoTa", cv.MoTa);
                cv.CoHoatDong = LayBoolean(duLieuCapNhat, "CoHoatDong", cv.CoHoatDong);
                break;

            case "TrinhDoChuyenMon":
                var td = await _coSoDuLieu.TrinhDoChuyenMons.FirstOrDefaultAsync(t => t.TrinhDoChuyenMonId == idSo, cancellationToken)
                    ?? throw new KeyNotFoundException($"Không tìm thấy Trình độ có ID {idSo}.");

                var maTdMoi = LayChuoi(duLieuCapNhat, "MaTrinhDo", td.MaTrinhDo);
                var tenTdMoi = LayChuoi(duLieuCapNhat, "TenTrinhDo", td.TenTrinhDo);
                if (string.IsNullOrWhiteSpace(maTdMoi) || string.IsNullOrWhiteSpace(tenTdMoi))
                {
                    throw new InvalidOperationException("Mã trình độ và Tên trình độ không được để trống.");
                }
                if (await _coSoDuLieu.TrinhDoChuyenMons.AnyAsync(t => t.MaTrinhDo == maTdMoi && t.TrinhDoChuyenMonId != idSo, cancellationToken))
                {
                    throw new InvalidOperationException($"Mã trình độ '{maTdMoi}' đã tồn tại ở bản ghi khác.");
                }

                td.MaTrinhDo = maTdMoi;
                td.TenTrinhDo = tenTdMoi;
                td.MoTa = LayChuoiHoacNull(duLieuCapNhat, "MoTa", td.MoTa);
                td.CoHoatDong = LayBoolean(duLieuCapNhat, "CoHoatDong", td.CoHoatDong);
                break;

            case "TaiKhoan":
                var user = await _coSoDuLieu.Users.FirstOrDefaultAsync(u => u.Id == khoaBanGhi, cancellationToken)
                    ?? throw new KeyNotFoundException($"Không tìm thấy Tài khoản có ID {khoaBanGhi}.");

                var emailMoi = LayChuoi(duLieuCapNhat, "Email", user.Email ?? "");
                var hoTenUserMoi = LayChuoi(duLieuCapNhat, "HoVaTen", user.HoVaTen);
                if (string.IsNullOrWhiteSpace(emailMoi) || string.IsNullOrWhiteSpace(hoTenUserMoi))
                {
                    throw new InvalidOperationException("Email và Họ tên tài khoản không được để trống.");
                }

                user.Email = emailMoi;
                user.NormalizedEmail = emailMoi.ToUpperInvariant();
                user.UserName = emailMoi;
                user.NormalizedUserName = emailMoi.ToUpperInvariant();
                user.HoVaTen = hoTenUserMoi;
                user.MaNhanSuLienKet = LayChuoiHoacNull(duLieuCapNhat, "MaNhanSuLienKet", user.MaNhanSuLienKet);
                user.PhoneNumber = LayChuoiHoacNull(duLieuCapNhat, "SoDienThoai", user.PhoneNumber);
                user.CoHoatDong = LayBoolean(duLieuCapNhat, "CoHoatDong", user.CoHoatDong);

                // Cập nhật vai trò nếu có truyền lên
                if (duLieuCapNhat.TryGetValue("VaiTro", out var vaiTroMoi) && !string.IsNullOrWhiteSpace(vaiTroMoi))
                {
                    var roleObj = await _coSoDuLieu.Roles.FirstOrDefaultAsync(r => r.Name == vaiTroMoi, cancellationToken);
                    if (roleObj != null)
                    {
                        var currentRoles = await _coSoDuLieu.UserRoles.Where(ur => ur.UserId == user.Id).ToListAsync(cancellationToken);
                        _coSoDuLieu.UserRoles.RemoveRange(currentRoles);
                        await _coSoDuLieu.UserRoles.AddAsync(new IdentityUserRole<string>
                        {
                            UserId = user.Id,
                            RoleId = roleObj.Id
                        }, cancellationToken);
                    }
                }
                break;

            case "NhatKyHeThong":
                var nk = await _coSoDuLieu.NhatKyHeThongs.FirstOrDefaultAsync(x => x.NhatKyId == idSo, cancellationToken)
                    ?? throw new KeyNotFoundException($"Không tìm thấy bản ghi Nhật ký có ID {idSo}.");

                nk.NguoiThucHien = LayChuoi(duLieuCapNhat, "NguoiThucHien", nk.NguoiThucHien);
                nk.HanhDong = LayChuoi(duLieuCapNhat, "HanhDong", nk.HanhDong);
                nk.BangDuLieu = LayChuoi(duLieuCapNhat, "BangDuLieu", nk.BangDuLieu);
                nk.KhoaBanGhi = LayChuoiHoacNull(duLieuCapNhat, "KhoaBanGhi", nk.KhoaBanGhi);
                nk.GiaTriMoi = LayChuoiHoacNull(duLieuCapNhat, "GiaTriMoi", nk.GiaTriMoi);
                nk.KetQua = LayChuoi(duLieuCapNhat, "KetQua", nk.KetQua);
                break;
        }

        await _coSoDuLieu.SaveChangesAsync(cancellationToken);

        // Lưu các giá trị Trường Tùy Chỉnh (Custom_*)
        await LuuGiaTriTruongTuyChinhAsync(bangChuan, khoaBanGhi, duLieuCapNhat, cancellationToken);

        if (bangChuan != "NhatKyHeThong")
        {
            await _nhatKyDichVu.GhiNhatKyAsync(
                nguoiThucHien ?? "Admin Live CRUD",
                "Sửa trực tiếp DB",
                bangChuan,
                khoaBanGhi,
                null,
                $"Cập nhật trực tuyến bản ghi #{khoaBanGhi} bảng {bangChuan}",
                "Thành công",
                null,
                cancellationToken);
        }

        return true;
    }

    /// <summary>
    /// Thêm mới nhanh một bản ghi trực tiếp vào bảng bất kỳ kèm các giá trị trường tùy chỉnh.
    /// </summary>
    public async Task<string> ThemNhanhBanGhiAsync(
        string tenBang,
        Dictionary<string, string?> duLieuMoi,
        string? nguoiThucHien = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(duLieuMoi);
        var bangChuan = ChuanHoaTenBang(tenBang);
        string khoaBanGhiMoi = "";

        switch (bangChuan)
        {
            case "Khoa":
                var maKhoa = LayChuoi(duLieuMoi, "MaKhoa", "");
                var tenKhoa = LayChuoi(duLieuMoi, "TenKhoa", "");
                if (string.IsNullOrWhiteSpace(maKhoa) || string.IsNullOrWhiteSpace(tenKhoa))
                {
                    throw new InvalidOperationException("Mã khoa và Tên khoa là bắt buộc.");
                }
                if (await _coSoDuLieu.Khoas.AnyAsync(k => k.MaKhoa == maKhoa, cancellationToken))
                {
                    throw new InvalidOperationException($"Mã khoa '{maKhoa}' đã tồn tại.");
                }

                var khoaMoi = new Khoa
                {
                    MaKhoa = maKhoa,
                    TenKhoa = tenKhoa,
                    SoDienThoai = LayChuoiHoacNull(duLieuMoi, "SoDienThoai", null),
                    Email = LayChuoiHoacNull(duLieuMoi, "Email", null),
                    DiaDiem = LayChuoiHoacNull(duLieuMoi, "DiaDiem", null),
                    MoTa = LayChuoiHoacNull(duLieuMoi, "MoTa", null),
                    CoHoatDong = LayBoolean(duLieuMoi, "CoHoatDong", true),
                    NgayTao = DateTime.UtcNow
                };
                await _coSoDuLieu.Khoas.AddAsync(khoaMoi, cancellationToken);
                await _coSoDuLieu.SaveChangesAsync(cancellationToken);
                khoaBanGhiMoi = khoaMoi.KhoaId.ToString();
                break;

            case "BoMon":
                var maBm = LayChuoi(duLieuMoi, "MaBoMon", "");
                var tenBm = LayChuoi(duLieuMoi, "TenBoMon", "");
                var khoaIdBm = LaySoNguyen(duLieuMoi, "KhoaId", 0);
                if (string.IsNullOrWhiteSpace(maBm) || string.IsNullOrWhiteSpace(tenBm) || khoaIdBm <= 0)
                {
                    throw new InvalidOperationException("Mã bộ môn, Tên bộ môn và Khoa chủ quản là bắt buộc.");
                }
                if (await _coSoDuLieu.BoMons.AnyAsync(b => b.MaBoMon == maBm, cancellationToken))
                {
                    throw new InvalidOperationException($"Mã bộ môn '{maBm}' đã tồn tại.");
                }

                var bmMoi = new BoMon
                {
                    MaBoMon = maBm,
                    TenBoMon = tenBm,
                    KhoaId = khoaIdBm,
                    MoTa = LayChuoiHoacNull(duLieuMoi, "MoTa", null),
                    CoHoatDong = LayBoolean(duLieuMoi, "CoHoatDong", true),
                    NgayTao = DateTime.UtcNow
                };
                await _coSoDuLieu.BoMons.AddAsync(bmMoi, cancellationToken);
                await _coSoDuLieu.SaveChangesAsync(cancellationToken);
                khoaBanGhiMoi = bmMoi.BoMonId.ToString();
                break;

            case "LopHoc":
                var maLop = LayChuoi(duLieuMoi, "MaLop", "");
                var tenLop = LayChuoi(duLieuMoi, "TenLop", "");
                var nienKhoa = LayChuoi(duLieuMoi, "NienKhoa", "2024-2028");
                var khoaIdLop = LaySoNguyen(duLieuMoi, "KhoaId", 0);
                if (string.IsNullOrWhiteSpace(maLop) || string.IsNullOrWhiteSpace(tenLop) || khoaIdLop <= 0)
                {
                    throw new InvalidOperationException("Mã lớp, Tên lớp và Khoa quản lý là bắt buộc.");
                }
                if (await _coSoDuLieu.LopHocs.AnyAsync(l => l.MaLop == maLop, cancellationToken))
                {
                    throw new InvalidOperationException($"Mã lớp '{maLop}' đã tồn tại.");
                }

                var lopMoi = new LopHoc
                {
                    MaLop = maLop,
                    TenLop = tenLop,
                    NienKhoa = nienKhoa,
                    KhoaId = khoaIdLop,
                    SoLuongToiDa = LaySoNguyen(duLieuMoi, "SoLuongToiDa", 50),
                    CoHoatDong = LayBoolean(duLieuMoi, "CoHoatDong", true),
                    NgayTao = DateTime.UtcNow
                };
                await _coSoDuLieu.LopHocs.AddAsync(lopMoi, cancellationToken);
                await _coSoDuLieu.SaveChangesAsync(cancellationToken);
                khoaBanGhiMoi = lopMoi.LopHocId.ToString();
                break;

            case "HocVien":
                var maHv = LayChuoi(duLieuMoi, "MaHocVien", "");
                var hoTenHv = LayChuoi(duLieuMoi, "HoVaTen", "");
                var lopIdHv = LaySoNguyen(duLieuMoi, "LopHocId", 0);
                if (string.IsNullOrWhiteSpace(maHv) || string.IsNullOrWhiteSpace(hoTenHv) || lopIdHv <= 0)
                {
                    throw new InvalidOperationException("Mã học viên, Họ tên và Lớp học là bắt buộc.");
                }
                if (await _coSoDuLieu.HocViens.AnyAsync(h => h.MaHocVien == maHv, cancellationToken))
                {
                    throw new InvalidOperationException($"Mã học viên '{maHv}' đã tồn tại.");
                }

                var hvMoi = new HocVien
                {
                    MaHocVien = maHv,
                    HoVaTen = hoTenHv,
                    NgaySinh = LayNgayThang(duLieuMoi, "NgaySinh", new DateTime(2005, 1, 1)),
                    GioiTinh = LayChuoi(duLieuMoi, "GioiTinh", "Nam"),
                    LopHocId = lopIdHv,
                    Email = LayChuoiHoacNull(duLieuMoi, "Email", null),
                    SoDienThoai = LayChuoiHoacNull(duLieuMoi, "SoDienThoai", null),
                    DiaChi = LayChuoiHoacNull(duLieuMoi, "DiaChi", null),
                    CoHoatDong = LayBoolean(duLieuMoi, "CoHoatDong", true),
                    NgayTao = DateTime.UtcNow
                };
                await _coSoDuLieu.HocViens.AddAsync(hvMoi, cancellationToken);
                await _coSoDuLieu.SaveChangesAsync(cancellationToken);
                khoaBanGhiMoi = hvMoi.HocVienId.ToString();
                break;

            case "NhanSu":
                var maNs = LayChuoi(duLieuMoi, "MaNhanSu", "");
                var hoTenNs = LayChuoi(duLieuMoi, "HoVaTen", "");
                var emailNs = LayChuoi(duLieuMoi, "EmailCongViec", "");
                if (string.IsNullOrWhiteSpace(maNs) || string.IsNullOrWhiteSpace(hoTenNs) || string.IsNullOrWhiteSpace(emailNs))
                {
                    throw new InvalidOperationException("Mã cán bộ, Họ tên và Email công việc là bắt buộc.");
                }
                if (await _coSoDuLieu.NhanSus.AnyAsync(n => n.MaNhanSu == maNs, cancellationToken))
                {
                    throw new InvalidOperationException($"Mã nhân sự '{maNs}' đã tồn tại.");
                }

                var nsMoi = new NhanSu
                {
                    MaNhanSu = maNs,
                    HoVaTen = hoTenNs,
                    NgaySinh = LayNgayThang(duLieuMoi, "NgaySinh", new DateTime(1990, 1, 1)),
                    GioiTinh = LayChuoi(duLieuMoi, "GioiTinh", "Nam"),
                    EmailCongViec = emailNs,
                    SoDienThoai = LayChuoiHoacNull(duLieuMoi, "SoDienThoai", null),
                    SoGiayTo = LayChuoiHoacNull(duLieuMoi, "SoGiayTo", null),
                    KhoaId = LaySoNguyenHoacNull(duLieuMoi, "KhoaId", null),
                    BoMonId = LaySoNguyenHoacNull(duLieuMoi, "BoMonId", null),
                    ChucVuId = LaySoNguyenHoacNull(duLieuMoi, "ChucVuId", null),
                    TrinhDoChuyenMonId = LaySoNguyenHoacNull(duLieuMoi, "TrinhDoChuyenMonId", null),
                    ChucDanhNgheNghiep = LayChuoiHoacNull(duLieuMoi, "ChucDanhNgheNghiep", null),
                    HocVi = LayChuoiHoacNull(duLieuMoi, "HocVi", null),
                    HocHam = LayChuoiHoacNull(duLieuMoi, "HocHam", null),
                    ChuyenNganh = LayChuoiHoacNull(duLieuMoi, "ChuyenNganh", null),
                    TruongDaoTao = LayChuoiHoacNull(duLieuMoi, "TruongDaoTao", null),
                    TrangThaiCongTac = LayChuoi(duLieuMoi, "TrangThaiCongTac", "Đang làm việc"),
                    CoHoatDong = LayBoolean(duLieuMoi, "CoHoatDong", true),
                    NgayTao = DateTime.UtcNow,
                    NguoiTao = nguoiThucHien
                };
                await _coSoDuLieu.NhanSus.AddAsync(nsMoi, cancellationToken);
                await _coSoDuLieu.SaveChangesAsync(cancellationToken);
                khoaBanGhiMoi = nsMoi.NhanSuId.ToString();
                break;

            case "MonHoc":
                var maMh = LayChuoi(duLieuMoi, "MaMonHoc", "");
                var tenMh = LayChuoi(duLieuMoi, "TenMonHoc", "");
                if (string.IsNullOrWhiteSpace(maMh) || string.IsNullOrWhiteSpace(tenMh))
                {
                    throw new InvalidOperationException("Mã môn học và Tên môn học là bắt buộc.");
                }
                if (await _coSoDuLieu.MonHocs.AnyAsync(m => m.MaMonHoc == maMh, cancellationToken))
                {
                    throw new InvalidOperationException($"Mã môn học '{maMh}' đã tồn tại.");
                }

                var mhMoi = new MonHoc
                {
                    MaMonHoc = maMh,
                    TenMonHoc = tenMh,
                    SoTinChi = LaySoNguyen(duLieuMoi, "SoTinChi", 3),
                    SoTietLyThuyet = LaySoNguyen(duLieuMoi, "SoTietLyThuyet", 30),
                    SoTietThucHanh = LaySoNguyen(duLieuMoi, "SoTietThucHanh", 15),
                    BoMonId = LaySoNguyenHoacNull(duLieuMoi, "BoMonId", null),
                    CoHoatDong = LayBoolean(duLieuMoi, "CoHoatDong", true)
                };
                await _coSoDuLieu.MonHocs.AddAsync(mhMoi, cancellationToken);
                await _coSoDuLieu.SaveChangesAsync(cancellationToken);
                khoaBanGhiMoi = mhMoi.MonHocId.ToString();
                break;

            case "PhanCongGiangDay":
                var maPc = LayChuoi(duLieuMoi, "MaPhanCong", "");
                var nsIdPc = LaySoNguyen(duLieuMoi, "NhanSuId", 0);
                var mhIdPc = LaySoNguyen(duLieuMoi, "MonHocId", 0);
                var lopIdPc = LaySoNguyen(duLieuMoi, "LopHocId", 0);
                if (string.IsNullOrWhiteSpace(maPc) || nsIdPc <= 0 || mhIdPc <= 0 || lopIdPc <= 0)
                {
                    throw new InvalidOperationException("Mã phân công, Giảng viên, Môn học và Lớp học là bắt buộc.");
                }
                if (await _coSoDuLieu.PhanCongGiangDays.AnyAsync(p => p.MaPhanCong == maPc, cancellationToken))
                {
                    throw new InvalidOperationException($"Mã phân công '{maPc}' đã tồn tại.");
                }

                var pcMoi = new PhanCongGiangDay
                {
                    MaPhanCong = maPc,
                    NhanSuId = nsIdPc,
                    MonHocId = mhIdPc,
                    LopHocId = lopIdPc,
                    HocKy = LayChuoi(duLieuMoi, "HocKy", "Học kỳ 1"),
                    NamHoc = LayChuoi(duLieuMoi, "NamHoc", "2026-2027"),
                    VaiTroGiangDay = LayChuoi(duLieuMoi, "VaiTroGiangDay", "Giảng viên chính"),
                    LaChuNhiem = LayBoolean(duLieuMoi, "LaChuNhiem", false),
                    GhiChu = LayChuoiHoacNull(duLieuMoi, "GhiChu", null),
                    NgayTao = DateTime.UtcNow
                };
                await _coSoDuLieu.PhanCongGiangDays.AddAsync(pcMoi, cancellationToken);
                await _coSoDuLieu.SaveChangesAsync(cancellationToken);
                khoaBanGhiMoi = pcMoi.PhanCongId.ToString();
                break;

            case "ChucVu":
                var maCv = LayChuoi(duLieuMoi, "MaChucVu", "");
                var tenCv = LayChuoi(duLieuMoi, "TenChucVu", "");
                if (string.IsNullOrWhiteSpace(maCv) || string.IsNullOrWhiteSpace(tenCv))
                {
                    throw new InvalidOperationException("Mã chức vụ và Tên chức vụ là bắt buộc.");
                }
                if (await _coSoDuLieu.ChucVus.AnyAsync(c => c.MaChucVu == maCv, cancellationToken))
                {
                    throw new InvalidOperationException($"Mã chức vụ '{maCv}' đã tồn tại.");
                }

                var cvMoi = new ChucVu
                {
                    MaChucVu = maCv,
                    TenChucVu = tenCv,
                    MoTa = LayChuoiHoacNull(duLieuMoi, "MoTa", null),
                    CoHoatDong = LayBoolean(duLieuMoi, "CoHoatDong", true)
                };
                await _coSoDuLieu.ChucVus.AddAsync(cvMoi, cancellationToken);
                await _coSoDuLieu.SaveChangesAsync(cancellationToken);
                khoaBanGhiMoi = cvMoi.ChucVuId.ToString();
                break;

            case "TrinhDoChuyenMon":
                var maTd = LayChuoi(duLieuMoi, "MaTrinhDo", "");
                var tenTd = LayChuoi(duLieuMoi, "TenTrinhDo", "");
                if (string.IsNullOrWhiteSpace(maTd) || string.IsNullOrWhiteSpace(tenTd))
                {
                    throw new InvalidOperationException("Mã trình độ và Tên trình độ là bắt buộc.");
                }
                if (await _coSoDuLieu.TrinhDoChuyenMons.AnyAsync(t => t.MaTrinhDo == maTd, cancellationToken))
                {
                    throw new InvalidOperationException($"Mã trình độ '{maTd}' đã tồn tại.");
                }

                var tdMoi = new TrinhDoChuyenMon
                {
                    MaTrinhDo = maTd,
                    TenTrinhDo = tenTd,
                    MoTa = LayChuoiHoacNull(duLieuMoi, "MoTa", null),
                    CoHoatDong = LayBoolean(duLieuMoi, "CoHoatDong", true)
                };
                await _coSoDuLieu.TrinhDoChuyenMons.AddAsync(tdMoi, cancellationToken);
                await _coSoDuLieu.SaveChangesAsync(cancellationToken);
                khoaBanGhiMoi = tdMoi.TrinhDoChuyenMonId.ToString();
                break;

            case "TaiKhoan":
                var emailTk = LayChuoi(duLieuMoi, "Email", "");
                var hoTenTk = LayChuoi(duLieuMoi, "HoVaTen", "");
                if (string.IsNullOrWhiteSpace(emailTk) || string.IsNullOrWhiteSpace(hoTenTk))
                {
                    throw new InvalidOperationException("Email và Họ tên tài khoản là bắt buộc.");
                }
                if (await _coSoDuLieu.Users.AnyAsync(u => u.Email == emailTk, cancellationToken))
                {
                    throw new InvalidOperationException($"Email '{emailTk}' đã tồn tại.");
                }

                var tkMoi = new TaiKhoan
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = emailTk,
                    NormalizedUserName = emailTk.ToUpperInvariant(),
                    Email = emailTk,
                    NormalizedEmail = emailTk.ToUpperInvariant(),
                    EmailConfirmed = true,
                    HoVaTen = hoTenTk,
                    MaNhanSuLienKet = LayChuoiHoacNull(duLieuMoi, "MaNhanSuLienKet", null),
                    PhoneNumber = LayChuoiHoacNull(duLieuMoi, "SoDienThoai", null),
                    CoHoatDong = LayBoolean(duLieuMoi, "CoHoatDong", true),
                    SecurityStamp = Guid.NewGuid().ToString(),
                    NgayTao = DateTime.UtcNow
                };
                var hasher = new PasswordHasher<TaiKhoan>();
                var matKhauMacDinh = LayChuoi(duLieuMoi, "MatKhau", "Cand@123456");
                tkMoi.PasswordHash = hasher.HashPassword(tkMoi, matKhauMacDinh);

                await _coSoDuLieu.Users.AddAsync(tkMoi, cancellationToken);
                await _coSoDuLieu.SaveChangesAsync(cancellationToken);

                var vaiTroChon = LayChuoi(duLieuMoi, "VaiTro", VaiTroMacDinh.NguoiXem);
                var roleTim = await _coSoDuLieu.Roles.FirstOrDefaultAsync(r => r.Name == vaiTroChon, cancellationToken);
                if (roleTim != null)
                {
                    await _coSoDuLieu.UserRoles.AddAsync(new IdentityUserRole<string>
                    {
                        UserId = tkMoi.Id,
                        RoleId = roleTim.Id
                    }, cancellationToken);
                    await _coSoDuLieu.SaveChangesAsync(cancellationToken);
                }
                khoaBanGhiMoi = tkMoi.Id;
                break;

            case "NhatKyHeThong":
                var nkMoi = new NhatKyHeThong
                {
                    NguoiThucHien = LayChuoi(duLieuMoi, "NguoiThucHien", nguoiThucHien ?? "Admin"),
                    HanhDong = LayChuoi(duLieuMoi, "HanhDong", "Ghi chú kiểm toán"),
                    BangDuLieu = LayChuoi(duLieuMoi, "BangDuLieu", "HeThong"),
                    KhoaBanGhi = LayChuoiHoacNull(duLieuMoi, "KhoaBanGhi", null),
                    GiaTriMoi = LayChuoiHoacNull(duLieuMoi, "GiaTriMoi", null),
                    KetQua = LayChuoi(duLieuMoi, "KetQua", "Thành công"),
                    ThoiDiem = DateTime.UtcNow
                };
                await _coSoDuLieu.NhatKyHeThongs.AddAsync(nkMoi, cancellationToken);
                await _coSoDuLieu.SaveChangesAsync(cancellationToken);
                khoaBanGhiMoi = nkMoi.NhatKyId.ToString();
                break;
        }

        // Lưu các giá trị trường tùy chỉnh nếu có
        await LuuGiaTriTruongTuyChinhAsync(bangChuan, khoaBanGhiMoi, duLieuMoi, cancellationToken);

        if (bangChuan != "NhatKyHeThong")
        {
            await _nhatKyDichVu.GhiNhatKyAsync(
                nguoiThucHien ?? "Admin Live CRUD",
                "Thêm trực tiếp DB",
                bangChuan,
                khoaBanGhiMoi,
                null,
                $"Thêm nhanh bản ghi #{khoaBanGhiMoi} vào bảng {bangChuan}",
                "Thành công",
                null,
                cancellationToken);
        }

        return khoaBanGhiMoi;
    }

    /// <summary>
    /// Xóa trực tiếp một bản ghi khỏi bảng (có kiểm tra ràng buộc toàn vẹn).
    /// </summary>
    public async Task<bool> XoaNhanhBanGhiAsync(
        string tenBang,
        string khoaBanGhi,
        string? nguoiThucHien = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(khoaBanGhi))
        {
            throw new ArgumentException("Khóa bản ghi không hợp lệ.", nameof(khoaBanGhi));
        }

        var bangChuan = ChuanHoaTenBang(tenBang);
        int.TryParse(khoaBanGhi, out int idSo);

        switch (bangChuan)
        {
            case "Khoa":
                var khoa = await _coSoDuLieu.Khoas.FirstOrDefaultAsync(k => k.KhoaId == idSo, cancellationToken)
                    ?? throw new KeyNotFoundException($"Không tìm thấy Khoa có ID {idSo}.");
                if (await _coSoDuLieu.BoMons.AnyAsync(b => b.KhoaId == idSo, cancellationToken) ||
                    await _coSoDuLieu.LopHocs.AnyAsync(l => l.KhoaId == idSo, cancellationToken) ||
                    await _coSoDuLieu.NhanSus.AnyAsync(n => n.KhoaId == idSo, cancellationToken))
                {
                    throw new InvalidOperationException("Không thể xóa Khoa vì còn Bộ môn, Lớp học hoặc Nhân sự trực thuộc.");
                }
                _coSoDuLieu.Khoas.Remove(khoa);
                break;

            case "BoMon":
                var bm = await _coSoDuLieu.BoMons.FirstOrDefaultAsync(b => b.BoMonId == idSo, cancellationToken)
                    ?? throw new KeyNotFoundException($"Không tìm thấy Bộ môn có ID {idSo}.");
                if (await _coSoDuLieu.NhanSus.AnyAsync(n => n.BoMonId == idSo, cancellationToken) ||
                    await _coSoDuLieu.MonHocs.AnyAsync(m => m.BoMonId == idSo, cancellationToken))
                {
                    throw new InvalidOperationException("Không thể xóa Bộ môn vì còn Nhân sự hoặc Môn học trực thuộc.");
                }
                _coSoDuLieu.BoMons.Remove(bm);
                break;

            case "LopHoc":
                var lop = await _coSoDuLieu.LopHocs.FirstOrDefaultAsync(l => l.LopHocId == idSo, cancellationToken)
                    ?? throw new KeyNotFoundException($"Không tìm thấy Lớp học có ID {idSo}.");
                if (await _coSoDuLieu.HocViens.AnyAsync(h => h.LopHocId == idSo, cancellationToken) ||
                    await _coSoDuLieu.PhanCongGiangDays.AnyAsync(p => p.LopHocId == idSo, cancellationToken))
                {
                    throw new InvalidOperationException("Không thể xóa Lớp học vì còn Học viên hoặc Phân công giảng dạy.");
                }
                _coSoDuLieu.LopHocs.Remove(lop);
                break;

            case "HocVien":
                var hv = await _coSoDuLieu.HocViens.FirstOrDefaultAsync(h => h.HocVienId == idSo, cancellationToken)
                    ?? throw new KeyNotFoundException($"Không tìm thấy Học viên có ID {idSo}.");
                _coSoDuLieu.HocViens.Remove(hv);
                break;

            case "NhanSu":
                var ns = await _coSoDuLieu.NhanSus.FirstOrDefaultAsync(n => n.NhanSuId == idSo, cancellationToken)
                    ?? throw new KeyNotFoundException($"Không tìm thấy Nhân sự có ID {idSo}.");
                if (await _coSoDuLieu.PhanCongGiangDays.AnyAsync(p => p.NhanSuId == idSo, cancellationToken))
                {
                    throw new InvalidOperationException("Không thể xóa Nhân sự vì đang có Phân công giảng dạy.");
                }
                _coSoDuLieu.NhanSus.Remove(ns);
                break;

            case "MonHoc":
                var mh = await _coSoDuLieu.MonHocs.FirstOrDefaultAsync(m => m.MonHocId == idSo, cancellationToken)
                    ?? throw new KeyNotFoundException($"Không tìm thấy Môn học có ID {idSo}.");
                if (await _coSoDuLieu.PhanCongGiangDays.AnyAsync(p => p.MonHocId == idSo, cancellationToken))
                {
                    throw new InvalidOperationException("Không thể xóa Môn học vì đang có Phân công giảng dạy.");
                }
                _coSoDuLieu.MonHocs.Remove(mh);
                break;

            case "PhanCongGiangDay":
                var pc = await _coSoDuLieu.PhanCongGiangDays.FirstOrDefaultAsync(p => p.PhanCongId == idSo, cancellationToken)
                    ?? throw new KeyNotFoundException($"Không tìm thấy Phân công có ID {idSo}.");
                _coSoDuLieu.PhanCongGiangDays.Remove(pc);
                break;

            case "ChucVu":
                var cv = await _coSoDuLieu.ChucVus.FirstOrDefaultAsync(c => c.ChucVuId == idSo, cancellationToken)
                    ?? throw new KeyNotFoundException($"Không tìm thấy Chức vụ có ID {idSo}.");
                if (await _coSoDuLieu.NhanSus.AnyAsync(n => n.ChucVuId == idSo, cancellationToken))
                {
                    throw new InvalidOperationException("Không thể xóa Chức vụ vì đang có Nhân sự đảm nhiệm.");
                }
                _coSoDuLieu.ChucVus.Remove(cv);
                break;

            case "TrinhDoChuyenMon":
                var td = await _coSoDuLieu.TrinhDoChuyenMons.FirstOrDefaultAsync(t => t.TrinhDoChuyenMonId == idSo, cancellationToken)
                    ?? throw new KeyNotFoundException($"Không tìm thấy Trình độ có ID {idSo}.");
                if (await _coSoDuLieu.NhanSus.AnyAsync(n => n.TrinhDoChuyenMonId == idSo, cancellationToken))
                {
                    throw new InvalidOperationException("Không thể xóa Trình độ vì đang có Nhân sự liên kết.");
                }
                _coSoDuLieu.TrinhDoChuyenMons.Remove(td);
                break;

            case "TaiKhoan":
                var user = await _coSoDuLieu.Users.FirstOrDefaultAsync(u => u.Id == khoaBanGhi, cancellationToken)
                    ?? throw new KeyNotFoundException($"Không tìm thấy Tài khoản có ID {khoaBanGhi}.");
                if (string.Equals(user.Email, "admin@quanlydaotao.local", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Không thể xóa tài khoản Quản trị viên gốc của hệ thống.");
                }
                _coSoDuLieu.Users.Remove(user);
                break;

            case "NhatKyHeThong":
                var nk = await _coSoDuLieu.NhatKyHeThongs.FirstOrDefaultAsync(x => x.NhatKyId == idSo, cancellationToken)
                    ?? throw new KeyNotFoundException($"Không tìm thấy bản ghi Nhật ký có ID {idSo}.");
                _coSoDuLieu.NhatKyHeThongs.Remove(nk);
                break;
        }

        // Xóa các giá trị trường tùy chỉnh của bản ghi này
        var giaTriCu = await _coSoDuLieu.GiaTriTruongTuyChinhs
            .Where(g => g.TenBang == bangChuan && g.KhoaBanGhi == khoaBanGhi)
            .ToListAsync(cancellationToken);
        if (giaTriCu.Count > 0)
        {
            _coSoDuLieu.GiaTriTruongTuyChinhs.RemoveRange(giaTriCu);
        }

        await _coSoDuLieu.SaveChangesAsync(cancellationToken);

        if (bangChuan != "NhatKyHeThong")
        {
            await _nhatKyDichVu.GhiNhatKyAsync(
                nguoiThucHien ?? "Admin Live CRUD",
                "Xóa trực tiếp DB",
                bangChuan,
                khoaBanGhi,
                null,
                $"Xóa bản ghi #{khoaBanGhi} khỏi bảng {bangChuan}",
                "Thành công",
                null,
                cancellationToken);
        }

        return true;
    }

    /// <summary>
    /// Lấy danh sách các trường tùy chỉnh của một bảng.
    /// </summary>
    public async Task<List<TruongTuyChinh>> LayDanhSachTruongTuyChinhAsync(
        string tenBang,
        CancellationToken cancellationToken = default)
    {
        var bangChuan = ChuanHoaTenBang(tenBang);
        return await _coSoDuLieu.TruongTuyChinhs
            .AsNoTracking()
            .Where(t => t.TenBang == bangChuan)
            .OrderBy(t => t.ThuTuHienThi)
            .ThenBy(t => t.TruongTuyChinhId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Thêm mới một trường (cột) tùy chỉnh cho bảng được chỉ định.
    /// </summary>
    public async Task<TruongTuyChinh> ThemTruongTuyChinhAsync(
        TruongTuyChinh truongMoi,
        string? nguoiThucHien = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(truongMoi);

        truongMoi.TenBang = ChuanHoaTenBang(truongMoi.TenBang);
        truongMoi.MaTruong = truongMoi.MaTruong.Trim().Replace(" ", "_");
        truongMoi.TenHienThi = truongMoi.TenHienThi.Trim();
        truongMoi.NgayTao = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(truongMoi.MaTruong) || string.IsNullOrWhiteSpace(truongMoi.TenHienThi))
        {
            throw new InvalidOperationException("Mã trường và Tên hiển thị của trường tùy chỉnh là bắt buộc.");
        }

        var daTonTai = await _coSoDuLieu.TruongTuyChinhs.AnyAsync(
            t => t.TenBang == truongMoi.TenBang && t.MaTruong == truongMoi.MaTruong,
            cancellationToken);

        if (daTonTai)
        {
            throw new InvalidOperationException($"Trường tùy chỉnh có mã '{truongMoi.MaTruong}' đã tồn tại trong bảng {truongMoi.TenBang}.");
        }

        await _coSoDuLieu.TruongTuyChinhs.AddAsync(truongMoi, cancellationToken);
        await _coSoDuLieu.SaveChangesAsync(cancellationToken);

        await _nhatKyDichVu.GhiNhatKyAsync(
            nguoiThucHien ?? "Admin",
            "Thêm cột tùy chỉnh",
            truongMoi.TenBang,
            truongMoi.MaTruong,
            null,
            $"Thêm trường tùy chỉnh '{truongMoi.TenHienThi}' ({truongMoi.KieuDuLieu})",
            "Thành công",
            null,
            cancellationToken);

        return truongMoi;
    }

    /// <summary>
    /// Cập nhật cấu hình một trường (cột) tùy chỉnh.
    /// </summary>
    public async Task<TruongTuyChinh> CapNhatTruongTuyChinhAsync(
        TruongTuyChinh truongCapNhat,
        string? nguoiThucHien = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(truongCapNhat);

        var tonTai = await _coSoDuLieu.TruongTuyChinhs.FirstOrDefaultAsync(
            t => t.TruongTuyChinhId == truongCapNhat.TruongTuyChinhId,
            cancellationToken)
            ?? throw new KeyNotFoundException($"Không tìm thấy trường tùy chỉnh ID {truongCapNhat.TruongTuyChinhId}.");

        tonTai.TenHienThi = truongCapNhat.TenHienThi.Trim();
        tonTai.KieuDuLieu = truongCapNhat.KieuDuLieu;
        tonTai.TuyChonDanhSach = truongCapNhat.TuyChonDanhSach;
        tonTai.GiaTriMacDinh = truongCapNhat.GiaTriMacDinh;
        tonTai.LaBatBuoc = truongCapNhat.LaBatBuoc;
        tonTai.ThuTuHienThi = truongCapNhat.ThuTuHienThi;
        tonTai.CoHoatDong = truongCapNhat.CoHoatDong;

        await _coSoDuLieu.SaveChangesAsync(cancellationToken);

        await _nhatKyDichVu.GhiNhatKyAsync(
            nguoiThucHien ?? "Admin",
            "Sửa cột tùy chỉnh",
            tonTai.TenBang,
            tonTai.MaTruong,
            null,
            $"Cập nhật trường tùy chỉnh '{tonTai.TenHienThi}'",
            "Thành công",
            null,
            cancellationToken);

        return tonTai;
    }

    /// <summary>
    /// Xóa một trường (cột) tùy chỉnh và toàn bộ giá trị đi kèm.
    /// </summary>
    public async Task<bool> XoaTruongTuyChinhAsync(
        int truongTuyChinhId,
        string? nguoiThucHien = null,
        CancellationToken cancellationToken = default)
    {
        var tonTai = await _coSoDuLieu.TruongTuyChinhs.FirstOrDefaultAsync(
            t => t.TruongTuyChinhId == truongTuyChinhId,
            cancellationToken)
            ?? throw new KeyNotFoundException($"Không tìm thấy trường tùy chỉnh ID {truongTuyChinhId}.");

        var tenBang = tonTai.TenBang;
        var maTruong = tonTai.MaTruong;

        _coSoDuLieu.TruongTuyChinhs.Remove(tonTai);
        await _coSoDuLieu.SaveChangesAsync(cancellationToken);

        await _nhatKyDichVu.GhiNhatKyAsync(
            nguoiThucHien ?? "Admin",
            "Xóa cột tùy chỉnh",
            tenBang,
            maTruong,
            $"Xóa trường {tonTai.TenHienThi}",
            null,
            "Thành công",
            null,
            cancellationToken);

        return true;
    }

    // ==========================================
    // CÁC HÀM NỘI BỘ XỬ LÝ GIÁ TRỊ TRƯỜNG TÙY CHỈNH & KIỂU DỮ LIỆU
    // ==========================================

    private async Task LuuGiaTriTruongTuyChinhAsync(
        string bangChuan,
        string khoaBanGhi,
        Dictionary<string, string?> duLieu,
        CancellationToken ct)
    {
        var dsTruong = await _coSoDuLieu.TruongTuyChinhs
            .Where(t => t.TenBang == bangChuan && t.CoHoatDong)
            .ToListAsync(ct);

        if (dsTruong.Count == 0)
        {
            return;
        }

        var giaTriHienCo = await _coSoDuLieu.GiaTriTruongTuyChinhs
            .Where(g => g.TenBang == bangChuan && g.KhoaBanGhi == khoaBanGhi)
            .ToListAsync(ct);

        foreach (var truong in dsTruong)
        {
            var keyCustom = $"Custom_{truong.MaTruong}";
            if (!duLieu.TryGetValue(keyCustom, out var giaTriMoi))
            {
                continue;
            }

            if (truong.LaBatBuoc && string.IsNullOrWhiteSpace(giaTriMoi))
            {
                throw new InvalidOperationException($"Trường tùy chỉnh '{truong.TenHienThi}' là bắt buộc.");
            }

            var banGhiGiaTri = giaTriHienCo.FirstOrDefault(x => x.TruongTuyChinhId == truong.TruongTuyChinhId);
            if (banGhiGiaTri == null)
            {
                await _coSoDuLieu.GiaTriTruongTuyChinhs.AddAsync(new GiaTriTruongTuyChinh
                {
                    TruongTuyChinhId = truong.TruongTuyChinhId,
                    TenBang = bangChuan,
                    KhoaBanGhi = khoaBanGhi,
                    GiaTri = giaTriMoi?.Trim(),
                    NgayCapNhat = DateTime.UtcNow
                }, ct);
            }
            else
            {
                banGhiGiaTri.GiaTri = giaTriMoi?.Trim();
                banGhiGiaTri.NgayCapNhat = DateTime.UtcNow;
            }
        }

        await _coSoDuLieu.SaveChangesAsync(ct);
    }

    private static string LayChuoi(Dictionary<string, string?> dict, string key, string macDinh)
    {
        if (dict.TryGetValue(key, out var val) && val != null)
        {
            return val.Trim();
        }
        return macDinh;
    }

    private static string? LayChuoiHoacNull(Dictionary<string, string?> dict, string key, string? macDinh)
    {
        if (dict.TryGetValue(key, out var val))
        {
            return string.IsNullOrWhiteSpace(val) ? null : val.Trim();
        }
        return macDinh;
    }

    private static int LaySoNguyen(Dictionary<string, string?> dict, string key, int macDinh)
    {
        if (dict.TryGetValue(key, out var val) && int.TryParse(val, out int kq))
        {
            return kq;
        }
        return macDinh;
    }

    private static int? LaySoNguyenHoacNull(Dictionary<string, string?> dict, string key, int? macDinh)
    {
        if (dict.TryGetValue(key, out var val))
        {
            if (string.IsNullOrWhiteSpace(val))
            {
                return null;
            }
            if (int.TryParse(val, out int kq) && kq > 0)
            {
                return kq;
            }
            return null;
        }
        return macDinh;
    }

    private static bool LayBoolean(Dictionary<string, string?> dict, string key, bool macDinh)
    {
        if (dict.TryGetValue(key, out var val) && val != null)
        {
            var v = val.Trim().ToLowerInvariant();
            return v == "true" || v == "1" || v == "on" || v == "có" || v == "hoạt động";
        }
        return macDinh;
    }

    private static DateTime LayNgayThang(Dictionary<string, string?> dict, string key, DateTime macDinh)
    {
        if (dict.TryGetValue(key, out var val) && !string.IsNullOrWhiteSpace(val))
        {
            string[] formats = { "yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy", "MM/dd/yyyy" };
            if (DateTime.TryParseExact(val.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            {
                return dt;
            }
            if (DateTime.TryParse(val.Trim(), out var dt2))
            {
                return dt2;
            }
        }
        return macDinh;
    }
}
