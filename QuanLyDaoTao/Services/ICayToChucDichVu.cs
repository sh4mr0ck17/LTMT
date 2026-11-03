using QuanLyDaoTao.Models.ToChuc;

namespace QuanLyDaoTao.Services;

/// <summary>Cung cấp cấu trúc tổ chức và số liệu theo quan hệ thực tế.</summary>
public interface ICayToChucDichVu
{
    /// <summary>Lấy cây đầy đủ, bao gồm nhân sự chưa được phân đơn vị.</summary>
    Task<NutToChuc> LayCayAsync(CancellationToken cancellationToken = default);
}
