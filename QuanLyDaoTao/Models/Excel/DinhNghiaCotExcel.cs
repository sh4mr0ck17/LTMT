namespace QuanLyDaoTao.Models.Excel;

/// <summary>
/// Các kiểu dữ liệu mong đợi cho từng cột trong tệp Excel.
/// </summary>
public enum KieuDuLieuExcel
{
    /// <summary>
    /// Chuỗi ký tự văn bản thông thường.
    /// </summary>
    VanBan,

    /// <summary>
    /// Số nguyên.
    /// </summary>
    SoNguyen,

    /// <summary>
    /// Định dạng ngày tháng năm (dd/MM/yyyy).
    /// </summary>
    NgayThang,

    /// <summary>
    /// Địa chỉ thư điện tử.
    /// </summary>
    Email,

    /// <summary>
    /// Số điện thoại liên hệ.
    /// </summary>
    SoDienThoai,

    /// <summary>
    /// Giá trị logic (Có/Không, Hoạt động/Đã khóa, 1/0).
    /// </summary>
    Logic,

    /// <summary>
    /// Mã đối tượng khóa ngoại tham chiếu đến bảng khác.
    /// </summary>
    KhoaNgoai
}

/// <summary>
/// Định nghĩa cấu hình cho một cột dữ liệu trong lược đồ Excel của module.
/// </summary>
public class DinhNghiaCotExcel
{
    /// <summary>
    /// Tên cột chuẩn hiển thị trên dòng tiêu đề của tệp mẫu.
    /// </summary>
    public string TenCot { get; set; } = string.Empty;

    /// <summary>
    /// Các tên gọi tương đương (alias) được phép nhận diện.
    /// </summary>
    public string[] CacTenGoiKhac { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Xác định cột này có bắt buộc phải có dữ liệu trong từng dòng hay không.
    /// </summary>
    public bool LaBatBuoc { get; set; }

    /// <summary>
    /// Kiểu dữ liệu mong đợi của giá trị trong ô.
    /// </summary>
    public KieuDuLieuExcel KieuDuLieu { get; set; } = KieuDuLieuExcel.VanBan;

    /// <summary>
    /// Độ dài tối đa cho phép của văn bản.
    /// </summary>
    public int DoDaiToiDa { get; set; } = 255;

    /// <summary>
    /// Mô tả ý nghĩa của cột phục vụ hướng dẫn trong template.
    /// </summary>
    public string MoTa { get; set; } = string.Empty;

    /// <summary>
    /// Giá trị mẫu ví dụ điền sẵn trong tệp template tải về.
    /// </summary>
    public string ViDu { get; set; } = string.Empty;
}
