namespace QuanLyDaoTao.Models;

/// <summary>
/// Bốn vai trò của hệ thống (hợp nhất v4). Quyền ghi của <see cref="QuanLy"/> còn bị giới hạn
/// bởi vùng quản lý (<see cref="VungQuanLy"/>) và khoa phụ trách gắn trên tài khoản.
/// </summary>
public static class VaiTroMacDinh
{
    /// <summary>
    /// Siêu quản trị viên: toàn quyền, độc quyền Live CRUD Database Studio.
    /// </summary>
    public const string SuperAdmin = "Superadmin";

    /// <summary>
    /// Quản trị viên: toàn quyền nghiệp vụ, cấp phát và phân quyền tài khoản.
    /// </summary>
    public const string QuanTriVien = "Admin";

    /// <summary>
    /// Quản lý nghiệp vụ: chỉ được ghi trong vùng (Tổ chức - Cán bộ hoặc Đào tạo) và khoa được giao.
    /// </summary>
    public const string QuanLy = "Manager";

    /// <summary>
    /// Người xem: chỉ tra cứu, thông tin cá nhân (CCCD, SĐT, email, địa chỉ) bị che.
    /// </summary>
    public const string NguoiXem = "Viewer";

    /// <summary>
    /// Chuỗi vai trò được phép ghi dữ liệu nghiệp vụ (dùng cho thuộc tính [Authorize]).
    /// Phạm vi cụ thể của Manager được kiểm tra lại tại tầng dịch vụ.
    /// </summary>
    public const string NhomCoQuyenGhi = SuperAdmin + "," + QuanTriVien + "," + QuanLy;

    /// <summary>
    /// Chuỗi vai trò quản trị hệ thống (dùng cho thuộc tính [Authorize]).
    /// </summary>
    public const string NhomQuanTri = SuperAdmin + "," + QuanTriVien;

    /// <summary>
    /// Danh sách đầy đủ các vai trò hợp lệ.
    /// </summary>
    public static readonly IReadOnlyList<string> TatCa = [SuperAdmin, QuanTriVien, QuanLy, NguoiXem];

    /// <summary>
    /// Ánh xạ vai trò cũ (trước v4) sang vai trò mới và vùng quản lý tương ứng.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, (string VaiTroMoi, string? Vung)> AnhXaVaiTroCu =
        new Dictionary<string, (string, string?)>(StringComparer.OrdinalIgnoreCase)
        {
            ["DataManager"] = (QuanLy, VungQuanLy.DaoTao),
            ["CanBo"] = (QuanLy, VungQuanLy.ToChuc),
            ["GiaoVien"] = (NguoiXem, null),
            ["User"] = (NguoiXem, null)
        };

    /// <summary>
    /// Nhãn tiếng Việt của vai trò để hiển thị.
    /// </summary>
    public static string LayNhan(string? vaiTro) => vaiTro switch
    {
        SuperAdmin => "Siêu quản trị",
        QuanTriVien => "Quản trị viên",
        QuanLy => "Quản lý",
        NguoiXem => "Người xem",
        _ => "Chưa xác định"
    };
}

/// <summary>
/// Hai vùng dữ liệu tách biệt: tổ chức - cán bộ (Khoa, Bộ môn, Cán bộ) và đào tạo (Lớp, Học viên, Môn học, Phân công).
/// </summary>
public static class VungQuanLy
{
    /// <summary>
    /// Vùng Tổ chức - Cán bộ: Khoa, Bộ môn (tổ chuyên môn), Cán bộ giảng viên, Chức vụ, Trình độ.
    /// </summary>
    public const string ToChuc = "ToChuc";

    /// <summary>
    /// Vùng Đào tạo: Lớp, Học viên, Môn học, Phân công giảng dạy.
    /// </summary>
    public const string DaoTao = "DaoTao";

    /// <summary>
    /// Kiểm tra mã vùng có hợp lệ hay không.
    /// </summary>
    public static bool LaHopLe(string? vung) => vung is ToChuc or DaoTao;

    /// <summary>
    /// Nhãn tiếng Việt của vùng.
    /// </summary>
    public static string LayNhan(string? vung) => vung switch
    {
        ToChuc => "Tổ chức - Cán bộ",
        DaoTao => "Đào tạo",
        _ => "Toàn hệ thống"
    };
}

/// <summary>
/// Tên các claim tùy chỉnh gắn trên phiên đăng nhập.
/// </summary>
public static class LoaiClaimTuyChinh
{
    /// <summary>
    /// Claim vùng quản lý của tài khoản Manager.
    /// </summary>
    public const string VungQuanLy = "qldt:vung";

    /// <summary>
    /// Claim mã khoa phụ trách (KhoaId) của tài khoản Manager.
    /// </summary>
    public const string KhoaPhuTrach = "qldt:khoa";

    /// <summary>
    /// Claim họ tên hiển thị.
    /// </summary>
    public const string HoVaTen = "qldt:hoten";
}
