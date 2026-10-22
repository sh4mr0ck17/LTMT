using QuanLyDaoTao.Models;

namespace QuanLyDaoTao.Models.QuanTri;

/// <summary>
/// Thông tin tổng quan của một bảng / nhóm quản lý dữ liệu trong hệ thống.
/// </summary>
public class ThongTinBangHeThong
{
    /// <summary>
    /// Tên định danh bảng (Khoa, BoMon, LopHoc, HocVien, NhanSu, MonHoc, PhanCongGiangDay, ChucVu, TrinhDoChuyenMon, TaiKhoan, NhatKyHeThong).
    /// </summary>
    public string TenBang { get; set; } = string.Empty;

    /// <summary>
    /// Tên hiển thị Tiếng Việt của nhóm quản lý.
    /// </summary>
    public string TenHienThi { get; set; } = string.Empty;

    /// <summary>
    /// Biểu tượng Bootstrap Icon đại diện.
    /// </summary>
    public string BieuTuong { get; set; } = "bi-table";

    /// <summary>
    /// Tổng số bản ghi hiện có trong bảng.
    /// </summary>
    public int SoBanGhi { get; set; }

    /// <summary>
    /// Số lượng trường (cột) tùy chỉnh đã được cấu hình thêm cho bảng.
    /// </summary>
    public int SoTruongTuyChinh { get; set; }
}

/// <summary>
/// Đại diện cho một mục lựa chọn trong hộp thả xuống (Khóa ngoại hoặc Danh sách chọn).
/// </summary>
public class LuaChonKhoaNgoai
{
    /// <summary>
    /// Giá trị lưu trữ (ID hoặc chuỗi giá trị).
    /// </summary>
    public string GiaTri { get; set; } = string.Empty;

    /// <summary>
    /// Nhãn văn bản hiển thị cho người dùng.
    /// </summary>
    public string NhanHienThi { get; set; } = string.Empty;
}

/// <summary>
/// Siêu dữ liệu mô tả một cột (trường) dữ liệu của bảng để render giao diện CRUD trực tiếp.
/// </summary>
public class MoTaCotDuLieu
{
    /// <summary>
    /// Mã kỹ thuật của cột (ví dụ: MaKhoa, TenKhoa, hoặc Custom_SoQuyetDinh).
    /// </summary>
    public string MaCot { get; set; } = string.Empty;

    /// <summary>
    /// Tên hiển thị của cột trên giao diện.
    /// </summary>
    public string TenHienThi { get; set; } = string.Empty;

    /// <summary>
    /// Kiểu dữ liệu (VanBan, SoNguyen, NgayThang, Logic, KhoaNgoai, DanhSach).
    /// </summary>
    public string KieuDuLieu { get; set; } = "VanBan";

    /// <summary>
    /// Xác định đây có phải là cột khóa chính kỹ thuật hay không.
    /// </summary>
    public bool LaKhoaChinh { get; set; }

    /// <summary>
    /// Xác định trường này có bắt buộc nhập hay không.
    /// </summary>
    public bool LaBatBuoc { get; set; }

    /// <summary>
    /// Xác định đây có phải là trường tùy chỉnh do Admin thêm vào hay không.
    /// </summary>
    public bool LaTruongTuyChinh { get; set; }

    /// <summary>
    /// ID của trường tùy chỉnh nếu LaTruongTuyChinh = true.
    /// </summary>
    public int? TruongTuyChinhId { get; set; }

    /// <summary>
    /// Danh sách các lựa chọn nếu cột là KhoaNgoai hoặc DanhSach.
    /// </summary>
    public List<LuaChonKhoaNgoai> DanhSachLuaChon { get; set; } = new();
}

/// <summary>
/// Gói dữ liệu hoàn chỉnh của một bảng phục vụ hiển thị và chỉnh sửa CRUD trực tiếp trên web.
/// </summary>
public class DuLieuBangTrucTuyen
{
    /// <summary>
    /// Tên kỹ thuật của bảng.
    /// </summary>
    public string TenBang { get; set; } = string.Empty;

    /// <summary>
    /// Tên hiển thị Tiếng Việt của bảng.
    /// </summary>
    public string TenHienThi { get; set; } = string.Empty;

    /// <summary>
    /// Tên cột khóa chính của bảng (ví dụ: KhoaId, BoMonId, Id).
    /// </summary>
    public string TenKhoaChinh { get; set; } = string.Empty;

    /// <summary>
    /// Danh sách toàn bộ các cột (gồm cả cột gốc của DB và cột tùy chỉnh do Admin thêm).
    /// </summary>
    public List<MoTaCotDuLieu> DanhSachCot { get; set; } = new();

    /// <summary>
    /// Danh sách các định nghĩa trường tùy chỉnh của bảng này.
    /// </summary>
    public List<TruongTuyChinh> DanhSachTruongTuyChinh { get; set; } = new();

    /// <summary>
    /// Danh sách các dòng dữ liệu (mỗi dòng là bản đồ từ MaCot sang Giá trị chuỗi).
    /// </summary>
    public List<Dictionary<string, string?>> DanhSachDong { get; set; } = new();
}
