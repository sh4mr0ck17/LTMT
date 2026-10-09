using QuanLyDaoTao.Models;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Giao diện dịch vụ nghiệp vụ quản lý Hồ sơ Nhân sự / Giảng viên.
/// </summary>
public interface INhanSuDichVu
{
    /// <summary>
    /// Lấy danh sách nhân sự có hỗ trợ lọc theo khoa, bộ môn, chức vụ, trạng thái công tác và từ khóa tìm kiếm.
    /// </summary>
    /// <param name="khoaId">Lọc theo khoa trực thuộc.</param>
    /// <param name="boMonId">Lọc theo bộ môn trực thuộc.</param>
    /// <param name="chucVuId">Lọc theo chức vụ công tác.</param>
    /// <param name="trangThai">Lọc theo trạng thái công tác (Đang làm việc, Tạm nghỉ, Đã nghỉ việc).</param>
    /// <param name="tuKhoa">Từ khóa tìm kiếm theo mã, họ tên, email, sđt.</param>
    /// <param name="coHoatDong">Lọc theo trạng thái hoạt động trong hệ thống.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>Danh sách nhân sự.</returns>
    Task<List<NhanSu>> LayDanhSachAsync(
        int? khoaId = null,
        int? boMonId = null,
        int? chucVuId = null,
        string? trangThai = null,
        string? tuKhoa = null,
        bool? coHoatDong = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy thông tin chi tiết đầy đủ của một nhân sự theo ID.
    /// </summary>
    /// <param name="id">Mã định danh nhân sự.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>Đối tượng Nhân sự hoặc null nếu không tìm thấy.</returns>
    Task<NhanSu?> LayTheoIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy thông tin nhân sự theo mã nhân sự duy nhất.
    /// </summary>
    /// <param name="maNhanSu">Mã nhân sự nghiệp vụ.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>Đối tượng Nhân sự hoặc null nếu không tìm thấy.</returns>
    Task<NhanSu?> LayTheoMaAsync(string maNhanSu, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm mới một hồ sơ nhân sự vào hệ thống.
    /// </summary>
    /// <param name="nhanSu">Đối tượng Nhân sự cần thêm mới.</param>
    /// <param name="nguoiThucHien">Tài khoản thực hiện thao tác.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>Đối tượng Nhân sự sau khi thêm thành công.</returns>
    Task<NhanSu> ThemNhanSuAsync(NhanSu nhanSu, string? nguoiThucHien = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật thông tin hồ sơ nhân sự hiện có.
    /// </summary>
    /// <param name="nhanSu">Đối tượng Nhân sự chứa dữ liệu cập nhật.</param>
    /// <param name="nguoiThucHien">Tài khoản thực hiện cập nhật.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>Đối tượng Nhân sự sau khi cập nhật.</returns>
    Task<NhanSu> CapNhatNhanSuAsync(NhanSu nhanSu, string? nguoiThucHien = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa một hồ sơ nhân sự nếu không còn phân công giảng dạy.
    /// </summary>
    /// <param name="id">Mã định danh nhân sự cần xóa.</param>
    /// <param name="nguoiThucHien">Tài khoản thực hiện xóa.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>True nếu xóa thành công.</returns>
    Task<bool> XoaNhanSuAsync(int id, string? nguoiThucHien = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra xem mã nhân sự đã tồn tại trong hệ thống hay chưa.
    /// </summary>
    /// <param name="maNhanSu">Mã nhân sự cần kiểm tra.</param>
    /// <param name="boQuaId">Id nhân sự cần bỏ qua (khi cập nhật).</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>True nếu mã đã tồn tại.</returns>
    Task<bool> KiemTraTonTaiMaAsync(string maNhanSu, int? boQuaId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra xem nhân sự có thể xóa được hay không (không có phân công giảng dạy).
    /// </summary>
    /// <param name="id">Mã định danh nhân sự.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>True nếu có thể xóa an toàn.</returns>
    Task<bool> CoTheXoaAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách các trạng thái công tác khả dụng để phục vụ bộ lọc.
    /// </summary>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>Danh sách chuỗi các trạng thái công tác.</returns>
    Task<List<string>> LayDanhSachTrangThaiCongTacAsync(CancellationToken cancellationToken = default);
}
