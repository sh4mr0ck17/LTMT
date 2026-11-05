namespace QuanLyDaoTao.Models;

/// <summary>
/// Ánh xạ loại đối tượng (nút cây tổ chức) hoặc tên bảng dữ liệu sang lớp CSS màu trong <c>mau-doi-tuong.css</c>,
/// để cùng một đối tượng luôn có cùng một màu trên mọi trang.
/// </summary>
public static class MauDoiTuong
{
    private static readonly Dictionary<string, string> BangMau = new(StringComparer.OrdinalIgnoreCase)
    {
        // Loại nút trên cây tổ chức
        ["Học viện"] = "dt-hoc-vien-goc",
        ["Khoa"] = "dt-khoa",
        ["Bộ môn"] = "dt-bo-mon",
        ["Nhân sự"] = "dt-can-bo",
        ["Hệ đào tạo"] = "dt-he",
        ["Khóa"] = "dt-nien-khoa",
        ["Lớp học"] = "dt-lop",
        ["Học viên"] = "dt-hoc-vien",
        // Tên bảng dữ liệu
        ["BoMon"] = "dt-bo-mon",
        ["NhanSu"] = "dt-can-bo",
        ["LopHoc"] = "dt-lop",
        ["HocVien"] = "dt-hoc-vien",
        ["MonHoc"] = "dt-mon-hoc",
        ["PhanCongGiangDay"] = "dt-phan-cong",
        ["ChucVu"] = "dt-danh-muc",
        ["TrinhDoChuyenMon"] = "dt-danh-muc",
        ["ThongBao"] = "dt-thong-bao",
        ["TaiKhoan"] = "dt-tai-khoan",
        ["NhatKyHeThong"] = "dt-nhat-ky"
    };

    /// <summary>Trả về lớp CSS màu (ví dụ <c>dt-lop</c>); loại chưa khai báo dùng màu trung tính.</summary>
    public static string LayLop(string? loaiHoacBang) =>
        loaiHoacBang is not null && BangMau.TryGetValue(loaiHoacBang, out var lop) ? lop : "dt-trung-tinh";
}
