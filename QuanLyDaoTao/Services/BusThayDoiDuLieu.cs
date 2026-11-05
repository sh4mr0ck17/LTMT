namespace QuanLyDaoTao.Services;

/// <summary>
/// Kênh thông báo thay đổi dữ liệu dùng chung trong tiến trình (singleton).
/// Các trang Blazor đăng ký để tự làm mới khi tab/người dùng khác vừa ghi dữ liệu.
/// </summary>
public interface IBusThayDoiDuLieu
{
    /// <summary>
    /// Phát khi một bảng dữ liệu vừa thay đổi; tham số là tên bảng (ví dụ "HocVien").
    /// </summary>
    event Action<string>? DaThayDoi;

    /// <summary>
    /// Thông báo bảng dữ liệu vừa thay đổi.
    /// </summary>
    void PhatThayDoi(string tenBang);
}

/// <summary>
/// Cài đặt mặc định của <see cref="IBusThayDoiDuLieu"/>.
/// </summary>
public class BusThayDoiDuLieu : IBusThayDoiDuLieu
{
    /// <inheritdoc />
    public event Action<string>? DaThayDoi;

    /// <inheritdoc />
    public void PhatThayDoi(string tenBang)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenBang);
        DaThayDoi?.Invoke(tenBang);
    }
}
