using System.Globalization;
using System.Text;

namespace QuanLyDaoTao.Models.ToChuc;

/// <summary>
/// Một vị trí trong cây tổ chức; mỗi hồ sơ chỉ xuất hiện ở một nhánh.
/// </summary>
public class NutToChuc
{
    /// <summary>Mã định danh duy nhất trong cây.</summary>
    public string KhoaNut { get; set; } = string.Empty;
    /// <summary>Loại đơn vị hoặc hồ sơ.</summary>
    public string Loai { get; set; } = string.Empty;
    /// <summary>Tên hiển thị.</summary>
    public string Ten { get; set; } = string.Empty;
    /// <summary>Mã nghiệp vụ.</summary>
    public string Ma { get; set; } = string.Empty;
    /// <summary>Thông tin bổ sung: địa điểm, chức vụ hoặc niên khóa.</summary>
    public string MoTa { get; set; } = string.Empty;
    /// <summary>Trạng thái của bản ghi, để trống với nhóm điều hướng.</summary>
    public string TrangThai { get; set; } = string.Empty;
    /// <summary>Cờ hoạt động; nhóm điều hướng không có trạng thái.</summary>
    public bool? CoHoatDong { get; set; }
    /// <summary>Vấn đề cần người quản lý kiểm tra.</summary>
    public string? CanhBao { get; set; }
    /// <summary>Đường dẫn đến hồ sơ gốc.</summary>
    public string? DuongDan { get; set; }
    /// <summary>Số học viên đang học trong lớp, không tính hồ sơ đã tốt nghiệp hoặc thôi học.</summary>
    public int SiSo { get; set; }
    /// <summary>Sức chứa của lớp.</summary>
    public int? SucChua { get; set; }
    /// <summary>Các đơn vị và hồ sơ trực thuộc.</summary>
    public List<NutToChuc> Con { get; set; } = new();
    /// <summary>Tổng khoa trong phạm vi nhánh.</summary>
    public int SoKhoa => (Loai == "Khoa" ? 1 : 0) + Con.Sum(n => n.SoKhoa);
    /// <summary>Tổng bộ môn trong phạm vi nhánh.</summary>
    public int SoBoMon => (Loai == "Bộ môn" ? 1 : 0) + Con.Sum(n => n.SoBoMon);
    /// <summary>Tổng lớp trong phạm vi nhánh.</summary>
    public int SoLop => (Loai == "Lớp học" ? 1 : 0) + Con.Sum(n => n.SoLop);
    /// <summary>Tổng nhân sự không trùng lặp trong nhánh.</summary>
    public int SoNhanSu => (Loai == "Nhân sự" ? 1 : 0) + Con.Sum(n => n.SoNhanSu);
    /// <summary>Tổng học viên đang học của các lớp trong nhánh.</summary>
    public int SoHocVien => SiSo + Con.Sum(n => n.SoHocVien);
    /// <summary>Số bản ghi có cảnh báo trong nhánh.</summary>
    public int SoCanRaSoat => (CanhBao != null ? 1 : 0) + Con.Sum(n => n.SoCanRaSoat);
    /// <summary>Biểu tượng theo loại bản ghi.</summary>
    public string BieuTuong => Loai switch
    {
        "Học viện" => "bi-buildings",
        "Khoa" => "bi-building",
        "Bộ môn" => "bi-diagram-2",
        "Lớp học" => "bi-mortarboard",
        "Nhân sự" => "bi-person",
        "Học viên" => "bi-person-badge",
        "Hệ đào tạo" => "bi-collection",
        "Khóa" => "bi-calendar3",
        _ => "bi-folder2"
    };

    /// <summary>Tìm tên và mã bằng tiếng Việt có dấu hoặc không dấu.</summary>
    public bool KhopTuKhoa(string tuKhoa)
    {
        var noiDung = ChuanHoaTimKiem($"{Ten} {Ma} {MoTa}");
        return noiDung.Contains(ChuanHoaTimKiem(tuKhoa.Trim()), StringComparison.OrdinalIgnoreCase);
    }

    private static string ChuanHoaTimKiem(string noiDung)
    {
        var cacKyTu = noiDung.Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
        return new string(cacKyTu).Replace('đ', 'd').Replace('Đ', 'D').Normalize(NormalizationForm.FormC);
    }
}
