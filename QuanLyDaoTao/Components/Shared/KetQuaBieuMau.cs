namespace QuanLyDaoTao.Components.Shared;

/// <summary>
/// Kết quả trả về khi hộp thoại biểu mẫu lưu thành công.
/// </summary>
/// <param name="ThongDiep">Thông điệp hiển thị cho người dùng.</param>
/// <param name="MaBanGhi">Mã nghiệp vụ của bản ghi vừa lưu (để trang lọc và làm nổi bật).</param>
public sealed record KetQuaBieuMau(string ThongDiep, string? MaBanGhi = null);
