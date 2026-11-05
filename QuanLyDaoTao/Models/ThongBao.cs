using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDaoTao.Models;

/// <summary>
/// Thông báo nội bộ hiển thị trên Bảng tin trang chủ, gắn phạm vi đối tượng nhận (toàn hệ thống, khoa, hệ, khóa, lớp).
/// </summary>
public class ThongBao
{
    /// <summary>
    /// Số giờ một thông báo được coi là "Mới".
    /// </summary>
    public const int SoGioHienNhanMoi = 24;

    /// <summary>
    /// Khóa chính.
    /// </summary>
    public int ThongBaoId { get; set; }

    /// <summary>
    /// Tiêu đề thông báo.
    /// </summary>
    [Required(ErrorMessage = "Vui lòng nhập tiêu đề.")]
    [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự.")]
    public string TieuDe { get; set; } = string.Empty;

    /// <summary>
    /// Nội dung thông báo (văn bản thuần, xuống dòng được giữ nguyên khi hiển thị).
    /// </summary>
    [Required(ErrorMessage = "Vui lòng nhập nội dung.")]
    [StringLength(20000, ErrorMessage = "Nội dung không được vượt quá 20.000 ký tự.")]
    public string NoiDung { get; set; } = string.Empty;

    /// <summary>
    /// Danh sách phạm vi nhận, phân tách bằng dấu chấm phẩy. Ví dụ: "*", "khoa:1", "he:Chính quy", "khoahoc:D57", "lop:5".
    /// </summary>
    [StringLength(1000)]
    public string PhamVi { get; set; } = "*";

    /// <summary>
    /// Ghim lên đầu bảng tin.
    /// </summary>
    public bool DaGhim { get; set; }

    /// <summary>
    /// Mức độ: Thường, Quan trọng, Khẩn.
    /// </summary>
    [StringLength(20)]
    public string MucDo { get; set; } = MucDoThongBao.Thuong;

    /// <summary>
    /// Hết hạn hiển thị (UTC). Null = không hết hạn. Thông báo hết hạn chỉ người đăng / quản trị còn thấy khi bật bộ lọc.
    /// </summary>
    public DateTime? HetHanLuc { get; set; }

    /// <summary>
    /// Ảnh đính kèm dạng data URI (PNG, JPEG, WebP; tối đa 3 MB, đã kiểm tra chữ ký tệp phía máy chủ).
    /// </summary>
    public string? AnhDinhKem { get; set; }

    /// <summary>
    /// Loại văn bản (chuyên mục): Thông báo, Kế hoạch, Quyết định, Công văn, Hướng dẫn, Lịch công tác.
    /// </summary>
    [StringLength(30)]
    public string LoaiVanBan { get; set; } = LoaiVanBanThongBao.ThongBao;

    /// <summary>
    /// Số / ký hiệu văn bản (ví dụ 125/TB-C500). Không bắt buộc.
    /// </summary>
    [StringLength(50)]
    public string? SoKyHieu { get; set; }

    /// <summary>
    /// Cơ quan / đơn vị ban hành.
    /// </summary>
    [StringLength(150)]
    public string? CoQuanBanHanh { get; set; }

    /// <summary>
    /// Người ký ban hành.
    /// </summary>
    [StringLength(100)]
    public string? NguoiKy { get; set; }

    /// <summary>
    /// Ngày ban hành văn bản (khác ngày đăng lên hệ thống).
    /// </summary>
    public DateTime? NgayBanHanh { get; set; }

    /// <summary>
    /// Trích yếu (tóm tắt một câu hiển thị ở danh sách).
    /// </summary>
    [StringLength(500)]
    public string? TrichYeu { get; set; }

    /// <summary>
    /// Danh sách lượt đọc.
    /// </summary>
    public virtual ICollection<ThongBaoDaDoc> LuotDoc { get; set; } = new List<ThongBaoDaDoc>();

    /// <summary>
    /// Tệp đính kèm (PDF, Word, Excel, ảnh).
    /// </summary>
    public virtual ICollection<TepDinhKemThongBao> TepDinhKem { get; set; } = new List<TepDinhKemThongBao>();

    /// <summary>
    /// Thông báo đã quá hạn hiển thị.
    /// </summary>
    public bool LaHetHan(DateTime hienTaiUtc) => HetHanLuc.HasValue && HetHanLuc.Value <= hienTaiUtc;

    /// <summary>
    /// Tên đăng nhập người đăng.
    /// </summary>
    [StringLength(100)]
    public string NguoiDang { get; set; } = string.Empty;

    /// <summary>
    /// Thời điểm đăng (UTC).
    /// </summary>
    public DateTime NgayDang { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Thời điểm sửa gần nhất (UTC).
    /// </summary>
    public DateTime? NgaySua { get; set; }

    /// <summary>
    /// Danh sách phạm vi đã tách.
    /// </summary>
    [NotMapped]
    public IReadOnlyList<string> DanhSachPhamVi =>
        PhamVi.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    /// <summary>
    /// Thông báo còn trong khoảng hiển thị nhãn "Mới".
    /// </summary>
    public bool LaMoi(DateTime hienTaiUtc) =>
        hienTaiUtc >= NgayDang && hienTaiUtc - NgayDang < TimeSpan.FromHours(SoGioHienNhanMoi);
}

/// <summary>
/// Lượt đọc thông báo của một tài khoản (mỗi người một dòng).
/// </summary>
public class ThongBaoDaDoc
{
    /// <summary>
    /// Khóa chính.
    /// </summary>
    public int ThongBaoDaDocId { get; set; }

    /// <summary>
    /// Thông báo được đọc.
    /// </summary>
    public int ThongBaoId { get; set; }

    /// <summary>
    /// Điều hướng tới thông báo.
    /// </summary>
    public virtual ThongBao? ThongBao { get; set; }

    /// <summary>
    /// Tên đăng nhập người đọc.
    /// </summary>
    [StringLength(100)]
    public string TenDangNhap { get; set; } = string.Empty;

    /// <summary>
    /// Thời điểm đọc lần đầu (UTC).
    /// </summary>
    public DateTime DocLuc { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Các mức độ ưu tiên của thông báo.
/// </summary>
public static class MucDoThongBao
{
    /// <summary>Thông báo thường.</summary>
    public const string Thuong = "Thường";

    /// <summary>Thông báo quan trọng.</summary>
    public const string QuanTrong = "Quan trọng";

    /// <summary>Thông báo khẩn.</summary>
    public const string Khan = "Khẩn";

    /// <summary>Tất cả mức độ theo thứ tự tăng dần.</summary>
    public static readonly IReadOnlyList<string> TatCa = [Thuong, QuanTrong, Khan];
}

/// <summary>
/// Tệp đính kèm của thông báo, lưu trong CSDL (đã kiểm tra chữ ký tệp phía máy chủ).
/// </summary>
public class TepDinhKemThongBao
{
    /// <summary>Khóa chính.</summary>
    public int TepDinhKemThongBaoId { get; set; }

    /// <summary>Thông báo sở hữu.</summary>
    public int ThongBaoId { get; set; }

    /// <summary>Điều hướng tới thông báo.</summary>
    public virtual ThongBao? ThongBao { get; set; }

    /// <summary>Tên tệp gốc (đã làm sạch).</summary>
    [StringLength(200)]
    public string TenTep { get; set; } = string.Empty;

    /// <summary>Kiểu nội dung (MIME) xác định từ chữ ký tệp.</summary>
    [StringLength(120)]
    public string KieuNoiDung { get; set; } = "application/octet-stream";

    /// <summary>Dung lượng (byte).</summary>
    public long KichThuoc { get; set; }

    /// <summary>Nội dung tệp.</summary>
    public byte[] DuLieu { get; set; } = [];

    /// <summary>Thời điểm tải lên (UTC).</summary>
    public DateTime NgayTaiLen { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Các loại văn bản (chuyên mục) của cổng thông báo.
/// </summary>
public static class LoaiVanBanThongBao
{
    /// <summary>Thông báo.</summary>
    public const string ThongBao = "Thông báo";

    /// <summary>Kế hoạch.</summary>
    public const string KeHoach = "Kế hoạch";

    /// <summary>Quyết định.</summary>
    public const string QuyetDinh = "Quyết định";

    /// <summary>Công văn.</summary>
    public const string CongVan = "Công văn";

    /// <summary>Hướng dẫn.</summary>
    public const string HuongDan = "Hướng dẫn";

    /// <summary>Lịch công tác.</summary>
    public const string LichCongTac = "Lịch công tác";

    /// <summary>Tất cả loại theo thứ tự hiển thị.</summary>
    public static readonly IReadOnlyList<string> TatCa = [ThongBao, KeHoach, QuyetDinh, CongVan, HuongDan, LichCongTac];

    /// <summary>Ký hiệu viết tắt dùng gợi ý số/ký hiệu (TB, KH, QĐ, CV, HD, LCT).</summary>
    public static string LayKyHieu(string loai) => loai switch
    {
        KeHoach => "KH",
        QuyetDinh => "QĐ",
        CongVan => "CV",
        HuongDan => "HD",
        LichCongTac => "LCT",
        _ => "TB"
    };
}
