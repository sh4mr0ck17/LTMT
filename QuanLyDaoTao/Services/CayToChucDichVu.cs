using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Data;
using QuanLyDaoTao.Models.ToChuc;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Dựng hai cây đọc riêng (không thay đổi dữ liệu): cây tổ chức theo khoa – bộ môn và cây đào tạo theo hệ – khóa – lớp.
/// </summary>
public class CayToChucDichVu : ICayToChucDichVu
{
    private const string TrangThaiDangHoc = "Đang học";
    private const string KhoaHocChuaXacDinh = "Chưa xác định khóa";
    private readonly QuanLyDaoTaoDbContext _coSoDuLieu;

    /// <summary>Khởi tạo dịch vụ với cơ sở dữ liệu đào tạo.</summary>
    public CayToChucDichVu(QuanLyDaoTaoDbContext coSoDuLieu)
    {
        ArgumentNullException.ThrowIfNull(coSoDuLieu);
        _coSoDuLieu = coSoDuLieu;
    }

    /// <inheritdoc />
    public async Task<NutToChuc> LayCayAsync(int? khoaId = null, int? boMonId = null, bool chiLayDonVi = false, CancellationToken cancellationToken = default)
    {
        var goc = TaoGoc("to-chuc", "Cơ cấu tổ chức: khoa → bộ môn → cán bộ. Cán bộ chưa có bộ môn nằm trong nhóm riêng của khoa.");

        var truyVanKhoa = _coSoDuLieu.Khoas.AsNoTracking();
        if (khoaId.HasValue)
        {
            truyVanKhoa = truyVanKhoa.Where(k => k.KhoaId == khoaId.Value);
        }
        var cacKhoa = await truyVanKhoa.OrderBy(k => k.MaKhoa)
            .Select(k => new { k.KhoaId, k.MaKhoa, k.TenKhoa, k.DiaDiem, k.CoHoatDong }).ToListAsync(cancellationToken);

        var truyVanBoMon = _coSoDuLieu.BoMons.AsNoTracking();
        if (khoaId.HasValue)
        {
            truyVanBoMon = truyVanBoMon.Where(b => b.KhoaId == khoaId.Value);
        }
        if (boMonId.HasValue)
        {
            truyVanBoMon = truyVanBoMon.Where(b => b.BoMonId == boMonId.Value);
        }
        var cacBoMon = await truyVanBoMon.OrderBy(b => b.MaBoMon)
            .Select(b => new { b.BoMonId, b.KhoaId, b.MaBoMon, b.TenBoMon, b.CoHoatDong }).ToListAsync(cancellationToken);

        var nutKhoa = new Dictionary<int, NutToChuc>();
        var nutBoMon = new Dictionary<int, NutToChuc>();
        foreach (var khoa in cacKhoa)
        {
            var nut = new NutToChuc
            {
                KhoaNut = $"khoa-{khoa.KhoaId}", Loai = "Khoa", Ten = khoa.TenKhoa, Ma = khoa.MaKhoa,
                MoTa = khoa.DiaDiem ?? "Chưa cập nhật địa điểm", CoHoatDong = khoa.CoHoatDong,
                TrangThai = LayTrangThai(khoa.CoHoatDong), DuongDan = $"NhanSu?khoa={khoa.KhoaId}"
            };
            nutKhoa.Add(khoa.KhoaId, nut);
            goc.Con.Add(nut);
        }

        foreach (var boMon in cacBoMon)
        {
            if (!nutKhoa.TryGetValue(boMon.KhoaId, out var khoa))
            {
                continue;
            }

            var nut = new NutToChuc
            {
                KhoaNut = $"bo-mon-{boMon.BoMonId}", Loai = "Bộ môn", Ten = boMon.TenBoMon, Ma = boMon.MaBoMon,
                CoHoatDong = boMon.CoHoatDong, TrangThai = LayTrangThai(boMon.CoHoatDong),
                DuongDan = $"NhanSu?bomon={boMon.BoMonId}"
            };
            nutBoMon.Add(boMon.BoMonId, nut);
            khoa.Con.Add(nut);
        }

        // Nếu chỉ lấy cấp đơn vị (khoa, bộ môn) để tải cực nhanh -> trả về ngay
        if (chiLayDonVi)
        {
            return goc;
        }

        var truyVanNhanSu = _coSoDuLieu.NhanSus.AsNoTracking();
        if (khoaId.HasValue)
        {
            truyVanNhanSu = truyVanNhanSu.Where(n => n.KhoaId == khoaId.Value);
        }
        if (boMonId.HasValue)
        {
            truyVanNhanSu = truyVanNhanSu.Where(n => n.BoMonId == boMonId.Value);
        }

        var cacNhanSu = await truyVanNhanSu.OrderBy(n => n.HoVaTen).ThenBy(n => n.MaNhanSu)
            .Select(n => new
            {
                n.NhanSuId, n.KhoaId, n.BoMonId, n.MaNhanSu, n.HoVaTen, n.TrangThai, n.CapBacHam,
                ChucVu = n.ChucVu == null ? "Chưa cập nhật chức vụ" : n.ChucVu.TenChucVu
            }).ToListAsync(cancellationToken);

        var nhomChuaVaoBoMon = new Dictionary<int, NutToChuc>();
        NutToChuc? chuaPhanDonVi = null;
        var khoaCuaBoMon = cacBoMon.ToDictionary(b => b.BoMonId, b => b.KhoaId);
        foreach (var nhanSu in cacNhanSu)
        {
            var nut = new NutToChuc
            {
                KhoaNut = $"nhan-su-{nhanSu.NhanSuId}", Loai = "Nhân sự", Ten = nhanSu.HoVaTen, Ma = nhanSu.MaNhanSu,
                MoTa = string.IsNullOrEmpty(nhanSu.CapBacHam) ? nhanSu.ChucVu : $"{nhanSu.CapBacHam}, {nhanSu.ChucVu}",
                TrangThai = nhanSu.TrangThai,
                CoHoatDong = nhanSu.TrangThai != "Đã khóa" && nhanSu.TrangThai != "Đã nghỉ việc",
                DuongDan = $"NhanSu?khoa={nhanSu.KhoaId}"
            };
            if (nhanSu.BoMonId is int bId && nutBoMon.TryGetValue(bId, out var boMon))
            {
                boMon.Con.Add(nut);
                if (nhanSu.KhoaId.HasValue && khoaCuaBoMon.TryGetValue(bId, out var kId) && nhanSu.KhoaId != kId)
                {
                    var tenKhoaHoSo = nutKhoa.GetValueOrDefault(nhanSu.KhoaId.Value)?.Ten ?? "không xác định";
                    nut.CanhBao = $"Khoa trong hồ sơ ({tenKhoaHoSo}) khác khoa chủ quản bộ môn. Cần kiểm tra hồ sơ gốc.";
                }
            }
            else if (nhanSu.KhoaId is int kId && nutKhoa.TryGetValue(kId, out var khoa))
            {
                if (!nhomChuaVaoBoMon.TryGetValue(kId, out var nhom))
                {
                    nhom = ThemNhom(khoa, "chua-bo-mon", "Cán bộ chưa vào bộ môn", "Cán bộ đã thuộc khoa nhưng chưa được phân vào bộ môn.");
                    nhomChuaVaoBoMon.Add(kId, nhom);
                }

                nhom.Con.Add(nut);
                if (nhanSu.BoMonId.HasValue)
                {
                    nut.CanhBao = "Bộ môn trong hồ sơ không tồn tại. Cần kiểm tra lại phân đơn vị.";
                }
            }
            else
            {
                chuaPhanDonVi ??= ThemNhom(goc, "chua-phan", "Cán bộ chưa xác định đơn vị", "Các hồ sơ chưa có khoa hoặc bộ môn hợp lệ, cần được phân đơn vị.");
                nut.CanhBao = "Chưa xác định được khoa hoặc bộ môn trực thuộc.";
                chuaPhanDonVi.Con.Add(nut);
            }
        }

        return goc;
    }

    /// <inheritdoc />
    public async Task<NutToChuc> LayCayDaoTaoAsync(string? he = null, string? khoaHoc = null, int? lopId = null, bool chiLayLop = false, CancellationToken cancellationToken = default)
    {
        var goc = TaoGoc("dao-tao", "Cơ cấu đào tạo: hệ → khóa → lớp → học viên. Mỗi lớp ghi rõ khoa quản lý.");

        var truyVanLop = _coSoDuLieu.LopHocs.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(he))
        {
            truyVanLop = truyVanLop.Where(l => l.HeDaoTao == he);
        }
        if (!string.IsNullOrWhiteSpace(khoaHoc))
        {
            truyVanLop = truyVanLop.Where(l => l.KhoaHoc == khoaHoc);
        }
        if (lopId.HasValue)
        {
            truyVanLop = truyVanLop.Where(l => l.LopHocId == lopId.Value);
        }

        var cacLop = await truyVanLop.Select(l => new
        {
            l.LopHocId, l.MaLop, l.TenLop, l.NienKhoa, l.CoHoatDong, l.HeDaoTao, l.KhoaHoc, l.ChuyenNganh, l.SoLuongToiDa,
            TenKhoa = l.Khoa != null ? l.Khoa.TenKhoa : null,
            SiSo = l.HocViens.Count(h => h.TrangThai == TrangThaiDangHoc)
        }).ToListAsync(cancellationToken);

        var nutLop = new Dictionary<int, NutToChuc>();
        foreach (var nhomHe in cacLop.GroupBy(l => l.HeDaoTao).OrderBy(g => g.Key))
        {
            var nutHe = new NutToChuc
            {
                KhoaNut = $"he-{nhomHe.Key}", Loai = "Hệ đào tạo", Ten = $"Hệ {nhomHe.Key}",
                MoTa = $"{nhomHe.Select(l => l.KhoaHoc).Distinct().Count()} khóa, {nhomHe.Count()} lớp",
                DuongDan = "LopHoc"
            };
            goc.Con.Add(nutHe);

            foreach (var nhomKhoaHoc in nhomHe.GroupBy(l => l.KhoaHoc ?? KhoaHocChuaXacDinh).OrderByDescending(g => g.Key))
            {
                var nutKhoaHoc = new NutToChuc
                {
                    KhoaNut = $"khoa-hoc-{nhomHe.Key}-{nhomKhoaHoc.Key}", Loai = "Khóa",
                    Ten = nhomKhoaHoc.Key == KhoaHocChuaXacDinh ? nhomKhoaHoc.Key : $"Khóa {nhomKhoaHoc.Key}",
                    Ma = nhomKhoaHoc.Key == KhoaHocChuaXacDinh ? string.Empty : nhomKhoaHoc.Key,
                    MoTa = nhomKhoaHoc.Key == KhoaHocChuaXacDinh
                        ? "Các lớp chưa khai báo khóa học — cần bổ sung tại trang Lớp."
                        : string.Join(", ", nhomKhoaHoc.Select(l => l.NienKhoa).Distinct()),
                    DuongDan = "LopHoc"
                };
                nutHe.Con.Add(nutKhoaHoc);

                foreach (var lop in nhomKhoaHoc.OrderBy(l => l.MaLop.Length).ThenBy(l => l.MaLop, StringComparer.Ordinal))
                {
                    var nut = new NutToChuc
                    {
                        KhoaNut = $"lop-{lop.LopHocId}", Loai = "Lớp học", Ten = lop.MaLop, Ma = lop.ChuyenNganh ?? string.Empty,
                        MoTa = $"{lop.TenKhoa ?? "Chưa có khoa"} · {lop.ChuyenNganh ?? "chưa có chuyên ngành"}",
                        CoHoatDong = lop.CoHoatDong, TrangThai = LayTrangThai(lop.CoHoatDong),
                        DuongDan = $"HocVien?lop={lop.LopHocId}", SiSo = lop.SiSo, SucChua = lop.SoLuongToiDa,
                        CanhBao = lop.SiSo > lop.SoLuongToiDa ? "Sĩ số vượt sức chứa của lớp." : null
                    };
                    nutLop.Add(lop.LopHocId, nut);
                    nutKhoaHoc.Con.Add(nut);
                }
            }
        }

        if (chiLayLop)
        {
            return goc;
        }

        var truyVanHocVien = _coSoDuLieu.HocViens.AsNoTracking();
        if (lopId.HasValue)
        {
            truyVanHocVien = truyVanHocVien.Where(h => h.LopHocId == lopId.Value);
        }
        else if (cacLop.Count > 0)
        {
            var danhSachLopId = cacLop.Select(l => l.LopHocId).ToList();
            truyVanHocVien = truyVanHocVien.Where(h => danhSachLopId.Contains(h.LopHocId));
        }

        var cacHocVien = await truyVanHocVien.OrderBy(h => h.HoVaTen)
            .Select(h => new { h.HocVienId, h.LopHocId, h.MaHocVien, h.HoVaTen, h.TrangThai, h.ChucVuLop })
            .ToListAsync(cancellationToken);

        foreach (var hocVien in cacHocVien)
        {
            if (!nutLop.TryGetValue(hocVien.LopHocId, out var lop))
            {
                continue;
            }

            lop.Con.Add(new NutToChuc
            {
                KhoaNut = $"hoc-vien-{hocVien.HocVienId}", Loai = "Học viên", Ten = hocVien.HoVaTen, Ma = hocVien.MaHocVien,
                MoTa = string.IsNullOrEmpty(hocVien.ChucVuLop) ? "Học viên" : hocVien.ChucVuLop,
                TrangThai = hocVien.TrangThai, CoHoatDong = hocVien.TrangThai == TrangThaiDangHoc,
                DuongDan = $"HocVien?lop={hocVien.LopHocId}"
            });
        }

        return goc;
    }

    private static NutToChuc TaoGoc(string nhanh, string moTa) => new()
    {
        KhoaNut = $"hoc-vien-{nhanh}", Loai = "Học viện", Ten = "Học viện An ninh nhân dân", Ma = "C500", MoTa = moTa
    };

    private static NutToChuc ThemNhom(NutToChuc cha, string ma, string ten, string moTa)
    {
        var nhom = new NutToChuc { KhoaNut = $"{cha.KhoaNut}-{ma}", Loai = "Nhóm", Ten = ten, MoTa = moTa };
        cha.Con.Add(nhom);
        return nhom;
    }

    private static string LayTrangThai(bool coHoatDong) => coHoatDong ? "Đang hoạt động" : "Ngừng hoạt động";
}
