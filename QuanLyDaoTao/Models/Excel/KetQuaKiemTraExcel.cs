namespace QuanLyDaoTao.Models.Excel;

/// <summary>
/// Đại diện cho một dòng dữ liệu được đọc từ Excel để phục vụ xem trước (Preview).
/// </summary>
public class DongDuLieuPreviewExcel
{
    /// <summary>
    /// Số dòng Excel thực tế (tính từ 1).
    /// </summary>
    public int SoDong { get; set; }

    /// <summary>
    /// Xác định xem dòng dữ liệu này có hợp lệ hoàn toàn hay không.
    /// </summary>
    public bool LaHopLe { get; set; } = true;

    /// <summary>
    /// Bản đồ chứa tên cột và giá trị chuỗi tương ứng của dòng đó.
    /// </summary>
    public Dictionary<string, string> DuLieuCot { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Danh sách các thông điệp lỗi thuộc về dòng này nếu có.
    /// </summary>
    public List<string> DanhSachLoi { get; set; } = new();
}

/// <summary>
/// Đại diện cho kết quả kiểm tra toàn bộ tệp Excel trước khi ghi vào cơ sở dữ liệu.
/// </summary>
public class KetQuaKiemTraExcel
{
    /// <summary>
    /// Tên tệp Excel tải lên.
    /// </summary>
    public string TenTep { get; set; } = string.Empty;

    /// <summary>
    /// Tên trang tính (sheet) được xử lý.
    /// </summary>
    public string TenSheet { get; set; } = string.Empty;

    /// <summary>
    /// Tên module nghiệp vụ đang nhập liệu (Khoa, LopHoc, NhanSu, ...).
    /// </summary>
    public string TenModule { get; set; } = string.Empty;

    /// <summary>
    /// Tổng số dòng dữ liệu đọc được từ Excel (không tính dòng tiêu đề).
    /// </summary>
    public int TongSoDong { get; set; }

    /// <summary>
    /// Số lượng dòng hoàn toàn hợp lệ và sẵn sàng nhập.
    /// </summary>
    public int SoDongHopLe { get; set; }

    /// <summary>
    /// Số lượng dòng có lỗi không thể nhập.
    /// </summary>
    public int SoDongLoi { get; set; }

    /// <summary>
    /// Xác định xem toàn bộ tệp có hợp lệ hay không (không có bất kỳ dòng nào lỗi).
    /// </summary>
    public bool LaHopLeToanBo => SoDongLoi == 0 && TongSoDong > 0 && DanhSachLoi.Count == 0;

    /// <summary>
    /// Danh sách toàn bộ các lỗi chi tiết theo từng ô / trường / dòng.
    /// </summary>
    public List<LoiChiTietExcel> DanhSachLoi { get; set; } = new();

    /// <summary>
    /// Danh sách các dòng dữ liệu phục vụ hiển thị xem trước (Preview).
    /// </summary>
    public List<DongDuLieuPreviewExcel> DanhSachDongDuLieu { get; set; } = new();

    /// <summary>
    /// Danh sách các tên cột theo đúng thứ tự tiêu đề.
    /// </summary>
    public List<string> DanhSachCot { get; set; } = new();
}
