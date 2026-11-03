using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Data;
using QuanLyDaoTao.Models.ToChuc;

namespace QuanLyDaoTao.Services;

/// <summary>Dựng cây đọc riêng, không làm thay đổi quan hệ trong cơ sở dữ liệu.</summary>
public class CayToChucDichVu : ICayToChucDichVu
{
    private readonly QuanLyDaoTaoDbContext _coSoDuLieu;

    /// <summary>Khởi tạo dịch vụ với cơ sở dữ liệu đào tạo.</summary>
    public CayToChucDichVu(QuanLyDaoTaoDbContext coSoDuLieu)
    {
        ArgumentNullException.ThrowIfNull(coSoDuLieu);
        _coSoDuLieu = coSoDuLieu;
    }

    /// <summary>Lấy dữ liệu tối thiểu và đặt mỗi hồ sơ vào đúng một nhánh quản lý.</summary>
    public async Task<NutToChuc> LayCayAsync(CancellationToken cancellationToken = default)
    {
        var goc = new NutToChuc
        {
            KhoaNut = "hoc-vien", Loai = "Học viện", Ten = "Học viện An ninh nhân dân", Ma = "C500",
            MoTa = "Cơ cấu khoa, bộ môn và các lớp do khoa quản lý. Nhân sự được xếp theo bộ môn; hồ sơ chưa có bộ môn nằm trực tiếp dưới khoa."
        };
        var cacKhoa = await _coSoDuLieu.Khoas.AsNoTracking().OrderBy(k => k.MaKhoa)
            .Select(k => new { k.KhoaId, k.MaKhoa, k.TenKhoa, k.DiaDiem, k.CoHoatDong }).ToListAsync(cancellationToken);
        var cacBoMon = await _coSoDuLieu.BoMons.AsNoTracking().OrderBy(b => b.MaBoMon)
            .Select(b => new { b.BoMonId, b.KhoaId, b.MaBoMon, b.TenBoMon, b.CoHoatDong }).ToListAsync(cancellationToken);
        var cacLop = await _coSoDuLieu.LopHocs.AsNoTracking().OrderBy(l => l.MaLop)
            .Select(l => new { l.LopHocId, l.KhoaId, l.MaLop, l.TenLop, l.NienKhoa, l.CoHoatDong,
                l.SoLuongToiDa, SiSo = l.HocViens.Count(h => h.TrangThai == "Đang học") }).ToListAsync(cancellationToken);
        var cacNhanSu = await _coSoDuLieu.NhanSus.AsNoTracking().OrderBy(n => n.HoVaTen).ThenBy(n => n.MaNhanSu)
            .Select(n => new { n.NhanSuId, n.KhoaId, n.BoMonId, n.MaNhanSu, n.HoVaTen, n.TrangThai,
                ChucVu = n.ChucVu == null ? "Chưa cập nhật chức vụ" : n.ChucVu.TenChucVu }).ToListAsync(cancellationToken);

        var nutKhoa = new Dictionary<int, NutToChuc>();
        var nutBoMon = new Dictionary<int, NutToChuc>();
        var nhomBoMon = new Dictionary<int, NutToChuc>();
        var nhomLop = new Dictionary<int, NutToChuc>();
        var nhomNhanSu = new Dictionary<int, NutToChuc>();
        foreach (var khoa in cacKhoa)
        {
            var nut = new NutToChuc
            {
                KhoaNut = $"khoa-{khoa.KhoaId}", Loai = "Khoa", Ten = khoa.TenKhoa, Ma = khoa.MaKhoa,
                MoTa = khoa.DiaDiem ?? "Chưa cập nhật địa điểm", CoHoatDong = khoa.CoHoatDong,
                TrangThai = LayTrangThai(khoa.CoHoatDong), DuongDan = $"Khoa/ChiTiet/{khoa.KhoaId}"
            };
            nutKhoa.Add(khoa.KhoaId, nut);
            goc.Con.Add(nut);
            nhomBoMon.Add(khoa.KhoaId, ThemNhom(nut, "bo-mon", "Bộ môn", "Các đơn vị chuyên môn thuộc khoa."));
            nhomLop.Add(khoa.KhoaId, ThemNhom(nut, "lop", "Lớp học", "Các lớp do khoa quản lý, độc lập với nhánh bộ môn."));
            nhomNhanSu.Add(khoa.KhoaId, ThemNhom(nut, "nhan-su", "Nhân sự trực thuộc khoa", "Nhân sự đã có khoa nhưng chưa được phân vào bộ môn."));
        }
        foreach (var boMon in cacBoMon)
        {
            var nut = new NutToChuc
            {
                KhoaNut = $"bo-mon-{boMon.BoMonId}", Loai = "Bộ môn", Ten = boMon.TenBoMon, Ma = boMon.MaBoMon,
                CoHoatDong = boMon.CoHoatDong, TrangThai = LayTrangThai(boMon.CoHoatDong),
                DuongDan = $"BoMon/ChiTiet/{boMon.BoMonId}"
            };
            nutBoMon.Add(boMon.BoMonId, nut);
            nhomBoMon[boMon.KhoaId].Con.Add(nut);
        }
        foreach (var lop in cacLop)
        {
            nhomLop[lop.KhoaId].Con.Add(new NutToChuc
            {
                KhoaNut = $"lop-{lop.LopHocId}", Loai = "Lớp học", Ten = lop.TenLop, Ma = lop.MaLop,
                MoTa = $"Niên khóa {lop.NienKhoa}", CoHoatDong = lop.CoHoatDong, TrangThai = LayTrangThai(lop.CoHoatDong),
                DuongDan = $"LopHoc/ChiTiet/{lop.LopHocId}", SiSo = lop.SiSo, SucChua = lop.SoLuongToiDa,
                CanhBao = lop.SiSo > lop.SoLuongToiDa ? "Sĩ số vượt sức chứa của lớp." : null
            });
        }
        NutToChuc? chuaPhanDonVi = null;
        var khoaCuaBoMon = cacBoMon.ToDictionary(b => b.BoMonId, b => b.KhoaId);
        foreach (var nhanSu in cacNhanSu)
        {
            var nut = new NutToChuc
            {
                KhoaNut = $"nhan-su-{nhanSu.NhanSuId}", Loai = "Nhân sự", Ten = nhanSu.HoVaTen,
                Ma = nhanSu.MaNhanSu, MoTa = nhanSu.ChucVu, TrangThai = nhanSu.TrangThai,
                CoHoatDong = nhanSu.TrangThai != "Đã khóa" && nhanSu.TrangThai != "Đã nghỉ việc",
                DuongDan = $"NhanSu/ChiTiet/{nhanSu.NhanSuId}"
            };
            if (nhanSu.BoMonId is int boMonId && nutBoMon.TryGetValue(boMonId, out var boMon))
            {
                boMon.Con.Add(nut);
                if (nhanSu.KhoaId.HasValue && nhanSu.KhoaId != khoaCuaBoMon[boMonId])
                {
                    var tenKhoaHoSo = nutKhoa.GetValueOrDefault(nhanSu.KhoaId.Value)?.Ten ?? "không xác định";
                    nut.CanhBao = $"Khoa trong hồ sơ ({tenKhoaHoSo}) khác khoa chủ quản bộ môn. Cây hiển thị theo bộ môn; cần kiểm tra hồ sơ gốc.";
                }
            }
            else if (nhanSu.KhoaId is int khoaId && nhomNhanSu.TryGetValue(khoaId, out var nhom))
            {
                nhom.Con.Add(nut);
                if (nhanSu.BoMonId.HasValue)
                {
                    nut.CanhBao = "Bộ môn trong hồ sơ không tồn tại. Cần kiểm tra lại phân đơn vị.";
                }
            }
            else
            {
                chuaPhanDonVi ??= ThemNhom(goc, "chua-phan", "Nhân sự chưa xác định đơn vị", "Các hồ sơ chưa có khoa hoặc bộ môn hợp lệ, cần được phân đơn vị.");
                nut.CanhBao = "Chưa xác định được khoa hoặc bộ môn trực thuộc.";
                chuaPhanDonVi.Con.Add(nut);
            }
        }
        return goc;
    }

    private static NutToChuc ThemNhom(NutToChuc cha, string ma, string ten, string moTa)
    {
        var nhom = new NutToChuc { KhoaNut = $"{cha.KhoaNut}-{ma}", Loai = "Nhóm", Ten = ten, MoTa = moTa };
        cha.Con.Add(nhom);
        return nhom;
    }

    private static string LayTrangThai(bool coHoatDong) => coHoatDong ? "Đang hoạt động" : "Ngừng hoạt động";
}
