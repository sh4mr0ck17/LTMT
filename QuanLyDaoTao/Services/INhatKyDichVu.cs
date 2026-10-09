using QuanLyDaoTao.Models;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Giao diện dịch vụ ghi nhận và tra cứu nhật ký kiểm toán hệ thống.
/// </summary>
public interface INhatKyDichVu
{
    /// <summary>
    /// Ghi nhận một sự kiện thao tác vào nhật ký hệ thống.
    /// </summary>
    /// <param name="nguoiThucHien">Tên tài khoản người thực hiện.</param>
    /// <param name="hanhDong">Hành động thực hiện (Thêm mới, Cập nhật, Xóa, Nhập Excel, Xuất Excel).</param>
    /// <param name="bangDuLieu">Tên bảng hoặc thực thể bị tác động.</param>
    /// <param name="khoaBanGhi">Khóa bản ghi hoặc mã đối tượng.</param>
    /// <param name="giaTriCu">Dữ liệu trước khi sửa đổi.</param>
    /// <param name="giaTriMoi">Dữ liệu sau khi sửa đổi.</param>
    /// <param name="ketQua">Kết quả thực hiện (Thành công, Thất bại).</param>
    /// <param name="diaChiIp">Địa chỉ IP của người gửi yêu cầu.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    Task GhiNhatKyAsync(
        string nguoiThucHien,
        string hanhDong,
        string bangDuLieu,
        string? khoaBanGhi = null,
        string? giaTriCu = null,
        string? giaTriMoi = null,
        string ketQua = "Thành công",
        string? diaChiIp = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách lịch sử nhật ký hệ thống có lọc và phân trang.
    /// </summary>
    /// <param name="tuKhoa">Từ khóa tìm kiếm theo người thực hiện hoặc khóa bản ghi.</param>
    /// <param name="bangDuLieu">Lọc theo tên bảng cụ thể.</param>
    /// <param name="soLuong">Số lượng bản ghi tối đa lấy về.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>Danh sách nhật ký hệ thống.</returns>
    Task<List<NhatKyHeThong>> LayDanhSachNhatKyAsync(
        string? tuKhoa = null,
        string? bangDuLieu = null,
        int soLuong = 100,
        CancellationToken cancellationToken = default);
}
