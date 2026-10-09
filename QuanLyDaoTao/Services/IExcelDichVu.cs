using QuanLyDaoTao.Models.Excel;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Giao diện dịch vụ nghiệp vụ xử lý tệp Excel (Mẫu, Đọc, Kiểm tra nghiêm ngặt, Preview, Commit, Xuất dữ liệu).
/// </summary>
public interface IExcelDichVu
{
    /// <summary>
    /// Tạo tệp Excel mẫu chuẩn (.xlsx) có kèm dòng hướng dẫn và dữ liệu ví dụ cho module được chỉ định.
    /// </summary>
    /// <param name="tenModule">Tên module (Khoa, BoMon, LopHoc, HocVien, NhanSu, MonHoc, PhanCongGiangDay).</param>
    /// <returns>Mảng byte của tệp Excel mẫu.</returns>
    byte[] TaoTepMauExcel(string tenModule);

    /// <summary>
    /// Đọc, kiểm tra tính toàn vẹn (Header, Định dạng dữ liệu, Ràng buộc khóa ngoại, Trùng lặp) và sinh báo cáo xem trước.
    /// </summary>
    /// <param name="tenModule">Tên module dữ liệu.</param>
    /// <param name="luongDuLieu">Luồng dữ liệu tệp Excel tải lên.</param>
    /// <param name="tenTepGoc">Tên gốc của tệp Excel.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>Đối tượng kết quả kiểm tra xem trước chi tiết.</returns>
    Task<KetQuaKiemTraExcel> KiemTraVaDocTepExcelAsync(
        string tenModule,
        Stream luongDuLieu,
        string tenTepGoc,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Thực hiện ghi các dòng hợp lệ vào cơ sở dữ liệu trong một Transaction an toàn (All-or-nothing rollback).
    /// </summary>
    /// <param name="tenModule">Tên module dữ liệu.</param>
    /// <param name="danhSachDongHopLe">Danh sách các dòng dữ liệu đã được kiểm tra hợp lệ.</param>
    /// <param name="nguoiThucHien">Tài khoản thực hiện lưu dữ liệu.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>Kết quả commit vào cơ sở dữ liệu.</returns>
    Task<KetQuaCommitExcel> CommitDuLieuExcelAsync(
        string tenModule,
        List<DongDuLieuPreviewExcel> danhSachDongHopLe,
        string? nguoiThucHien = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Xuất tệp Excel báo cáo lỗi chi tiết dựa trên kết quả kiểm tra để người dùng tải về sửa chữa.
    /// </summary>
    /// <param name="ketQuaKiemTra">Kết quả kiểm tra tệp Excel có chứa danh sách lỗi.</param>
    /// <returns>Mảng byte của tệp Excel báo cáo lỗi.</returns>
    byte[] XuatBaoCaoLoiExcel(KetQuaKiemTraExcel ketQuaKiemTra);

    /// <summary>
    /// Xuất danh sách toàn bộ dữ liệu hiện có của module ra tệp Excel chuẩn, an toàn chống CSV/Formula Injection.
    /// </summary>
    /// <param name="tenModule">Tên module cần xuất dữ liệu.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>Mảng byte của tệp Excel chứa dữ liệu.</returns>
    Task<byte[]> XuatDanhSachDuLieuExcelAsync(string tenModule, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lưu trữ kết quả kiểm tra vào bộ nhớ máy chủ để phục vụ bước xác nhận commit sau xem trước (Preview).
    /// </summary>
    /// <param name="tenModule">Tên module.</param>
    /// <param name="ketQua">Kết quả kiểm tra.</param>
    /// <param name="nguoiTao">Người tạo phiên.</param>
    /// <returns>Đối tượng phiên preview đã lưu.</returns>
    PhienPreviewExcel LuuPhienPreview(string tenModule, KetQuaKiemTraExcel ketQua, string nguoiTao);

    /// <summary>
    /// Lấy lại thông tin phiên xem trước từ bộ nhớ máy chủ theo mã phiên.
    /// </summary>
    /// <param name="maPhien">Mã phiên (GUID).</param>
    /// <returns>Đối tượng phiên preview hoặc null nếu hết hạn/không tồn tại.</returns>
    PhienPreviewExcel? LayPhienPreview(string maPhien);

    /// <summary>
    /// Hủy bỏ và xóa phiên xem trước khỏi bộ nhớ máy chủ.
    /// </summary>
    /// <param name="maPhien">Mã phiên cần xóa.</param>
    void XoaPhienPreview(string maPhien);
}
