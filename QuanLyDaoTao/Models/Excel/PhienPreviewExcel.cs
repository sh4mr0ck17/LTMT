namespace QuanLyDaoTao.Models.Excel;

/// <summary>
/// Đại diện cho phiên xem trước (Preview Session) được lưu giữ an toàn phía máy chủ.
/// </summary>
public class PhienPreviewExcel
{
    /// <summary>
    /// Mã phiên duy nhất (GUID).
    /// </summary>
    public string MaPhien { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Tên module nghiệp vụ (Khoa, BoMon, LopHoc, HocVien, NhanSu, MonHoc, PhanCongGiangDay).
    /// </summary>
    public string TenModule { get; set; } = string.Empty;

    /// <summary>
    /// Tài khoản người dùng đã tải tệp lên.
    /// </summary>
    public string NguoiTao { get; set; } = string.Empty;

    /// <summary>
    /// Thời điểm khởi tạo phiên xem trước.
    /// </summary>
    public DateTime ThoiGianTao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Thời điểm hết hạn hiệu lực của phiên xem trước (mặc định sau 30 phút).
    /// </summary>
    public DateTime ThoiGianHetHan { get; set; } = DateTime.UtcNow.AddMinutes(30);

    /// <summary>
    /// Kết quả kiểm tra tệp Excel chi tiết.
    /// </summary>
    public KetQuaKiemTraExcel KetQuaKiemTra { get; set; } = new();

    /// <summary>
    /// Kiểm tra xem phiên còn hiệu lực hay không.
    /// </summary>
    public bool ConHieuLuc => DateTime.UtcNow <= ThoiGianHetHan;
}

/// <summary>
/// Đại diện cho kết quả sau khi thực hiện xác nhận ghi (Commit) dữ liệu vào database.
/// </summary>
public class KetQuaCommitExcel
{
    /// <summary>
    /// Xác định thao tác commit có thành công hay không.
    /// </summary>
    public bool ThanhCong { get; set; }

    /// <summary>
    /// Tổng số bản ghi đã được thêm mới thành công vào cơ sở dữ liệu.
    /// </summary>
    public int SoBanGhiDaThem { get; set; }

    /// <summary>
    /// Thông điệp kết quả hiển thị cho người dùng.
    /// </summary>
    public string ThongDiep { get; set; } = string.Empty;

    /// <summary>
    /// Danh sách các lỗi phát sinh trong quá trình lưu dữ liệu nếu có.
    /// </summary>
    public List<string> DanhSachLoi { get; set; } = new();
}
