namespace QuanLyDaoTao.ViewModels.ThongKe;

/// <summary>
/// Thống kê số lượng học viên và lớp học theo từng khoa đào tạo.
/// </summary>
public class ThongKeKhoaItem
{
    /// <summary>
    /// Tên khoa đào tạo.
    /// </summary>
    public string TenKhoa { get; set; } = string.Empty;

    /// <summary>
    /// Số lượng lớp học thuộc khoa.
    /// </summary>
    public int SoLop { get; set; }

    /// <summary>
    /// Tổng số lượng học viên thuộc khoa.
    /// </summary>
    public int SoHocVien { get; set; }

    /// <summary>
    /// Tổng số lượng cán bộ/giảng viên thuộc khoa.
    /// </summary>
    public int SoNhanSu { get; set; }
}

/// <summary>
/// Thống kê tỷ lệ nhân sự theo trình độ chuyên môn.
/// </summary>
public class ThongKeTrinhDoItem
{
    /// <summary>
    /// Tên trình độ chuyên môn hoặc học vị.
    /// </summary>
    public string TenTrinhDo { get; set; } = string.Empty;

    /// <summary>
    /// Số lượng nhân sự đạt trình độ này.
    /// </summary>
    public int SoLuong { get; set; }
}

/// <summary>
/// Thống kê số lượng nhân sự theo chức vụ công tác.
/// </summary>
public class ThongKeChucVuItem
{
    /// <summary>
    /// Tên chức vụ.
    /// </summary>
    public string TenChucVu { get; set; } = string.Empty;

    /// <summary>
    /// Số lượng nhân sự giữ chức vụ này.
    /// </summary>
    public int SoLuong { get; set; }
}

/// <summary>
/// Thống kê tỷ lệ lấp đầy sĩ số của từng lớp học.
/// </summary>
public class ThongKeLopItem
{
    /// <summary>
    /// Tên lớp học.
    /// </summary>
    public string TenLop { get; set; } = string.Empty;

    /// <summary>
    /// Số lượng học viên hiện tại.
    /// </summary>
    public int SoHocVien { get; set; }

    /// <summary>
    /// Sĩ số tối đa được giao.
    /// </summary>
    public int SoLuongToiDa { get; set; }
}

/// <summary>
/// Gói dữ liệu hoàn chỉnh phục vụ vẽ các biểu đồ thống kê (Diagrams Panel).
/// </summary>
public class BaoCaoDiagramViewModel
{
    /// <summary>
    /// Tổng số khoa đào tạo.
    /// </summary>
    public int TongSoKhoa { get; set; }

    /// <summary>
    /// Tổng số bộ môn trực thuộc.
    /// </summary>
    public int TongSoBoMon { get; set; }

    /// <summary>
    /// Tổng số lớp học đào tạo.
    /// </summary>
    public int TongSoLop { get; set; }

    /// <summary>
    /// Tổng số học viên.
    /// </summary>
    public int TongSoHocVien { get; set; }

    /// <summary>
    /// Tổng số cán bộ / giảng viên.
    /// </summary>
    public int TongSoNhanSu { get; set; }

    /// <summary>
    /// Tổng số môn học.
    /// </summary>
    public int TongSoMonHoc { get; set; }

    /// <summary>
    /// Tổng số phân công giảng dạy.
    /// </summary>
    public int TongSoPhanCong { get; set; }

    /// <summary>
    /// Danh sách thống kê theo khoa.
    /// </summary>
    public List<ThongKeKhoaItem> DanhSachKhoa { get; set; } = new();

    /// <summary>
    /// Danh sách thống kê theo trình độ chuyên môn.
    /// </summary>
    public List<ThongKeTrinhDoItem> DanhSachTrinhDo { get; set; } = new();

    /// <summary>
    /// Danh sách thống kê theo chức vụ.
    /// </summary>
    public List<ThongKeChucVuItem> DanhSachChucVu { get; set; } = new();

    /// <summary>
    /// Danh sách thống kê theo lớp học.
    /// </summary>
    public List<ThongKeLopItem> DanhSachLop { get; set; } = new();
}
