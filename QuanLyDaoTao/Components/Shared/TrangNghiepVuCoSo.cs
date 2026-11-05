using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Services;
using Radzen;

namespace QuanLyDaoTao.Components.Shared;

/// <summary>
/// Lớp cơ sở cho trang danh sách nghiệp vụ: nạp quyền người dùng, tự làm mới khi bảng dữ liệu
/// thay đổi ở tab/người dùng khác, và quy chuẩn thông báo kết quả thao tác.
/// </summary>
public abstract class TrangNghiepVuCoSo : ComponentBase, IDisposable
{
    /// <summary>
    /// Thời gian hiển thị thông báo (mili giây).
    /// </summary>
    protected const int ThoiGianThongBao = 5000;

    /// <summary>
    /// Kênh thông báo thay đổi dữ liệu.
    /// </summary>
    [Inject]
    protected IBusThayDoiDuLieu Bus { get; set; } = default!;

    /// <summary>
    /// Dịch vụ phân quyền.
    /// </summary>
    [Inject]
    protected IPhanQuyenDichVu PhanQuyen { get; set; } = default!;

    /// <summary>
    /// Dịch vụ hộp thoại Radzen.
    /// </summary>
    [Inject]
    protected DialogService HopThoai { get; set; } = default!;

    /// <summary>
    /// Dịch vụ thông báo Radzen.
    /// </summary>
    [Inject]
    protected NotificationService ThongBao { get; set; } = default!;

    /// <summary>
    /// Người dùng hiện tại.
    /// </summary>
    protected NguoiDungHienTai NguoiDung { get; private set; } = NguoiDungHienTai.AnDanh;

    /// <summary>
    /// Đang nạp dữ liệu lần đầu.
    /// </summary>
    protected bool DangTai { get; private set; } = true;

    /// <summary>
    /// Tên các bảng mà trang theo dõi để tự làm mới.
    /// </summary>
    protected abstract IReadOnlyCollection<string> BangTheoDoi { get; }

    /// <summary>
    /// Nạp (hoặc nạp lại) dữ liệu của trang.
    /// </summary>
    protected abstract Task TaiDuLieuAsync();

    // DbContext dùng chung trong một mạch Blazor: tuần tự hóa mọi lần nạp để tránh truy vấn song song.
    private readonly SemaphoreSlim _khoaNap = new(1, 1);
    private bool _daHuy;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        NguoiDung = await PhanQuyen.LayNguoiDungAsync();
        await NapLaiAsync();
        DangTai = false;
        Bus.DaThayDoi += KhiDuLieuThayDoi;
    }

    /// <summary>
    /// Nạp lại dữ liệu một cách tuần tự (an toàn khi nhiều sự kiện cùng yêu cầu làm mới).
    /// </summary>
    protected async Task NapLaiAsync()
    {
        await _khoaNap.WaitAsync();
        try
        {
            await TaiDuLieuAsync();
        }
        finally
        {
            _khoaNap.Release();
        }
    }

    /// <summary>
    /// Thực hiện thao tác ghi, hiển thị thông báo thành công hoặc lỗi nghiệp vụ. Trả về true nếu thành công.
    /// </summary>
    protected async Task<bool> ThucHienAsync(Func<Task> thaoTac, string thongDiepThanhCong)
    {
        ArgumentNullException.ThrowIfNull(thaoTac);
        var loi = await ChayVaBatLoiAsync(thaoTac);
        if (loi is null)
        {
            BaoThanhCong(thongDiepThanhCong);
            await NapLaiAsync();
            return true;
        }

        BaoLoi(loi);
        return false;
    }

    /// <summary>
    /// Chạy thao tác và chuyển ngoại lệ nghiệp vụ thành thông điệp tiếng Việt (null nếu thành công).
    /// </summary>
    public static async Task<string?> ChayVaBatLoiAsync(Func<Task> thaoTac)
    {
        ArgumentNullException.ThrowIfNull(thaoTac);
        try
        {
            await thaoTac();
            return null;
        }
        catch (UnauthorizedAccessException ex)
        {
            return ex.Message;
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message;
        }
        catch (KeyNotFoundException ex)
        {
            return ex.Message;
        }
        catch (DbUpdateException)
        {
            return "Không lưu được do ràng buộc dữ liệu (trùng mã hoặc còn bản ghi liên kết). Vui lòng tải lại và kiểm tra.";
        }
    }

    /// <summary>
    /// Hiển thị thông báo thành công.
    /// </summary>
    protected void BaoThanhCong(string thongDiep) => ThongBao.Notify(new NotificationMessage
    {
        Severity = NotificationSeverity.Success,
        Summary = "Hoàn tất",
        Detail = thongDiep,
        Duration = ThoiGianThongBao
    });

    /// <summary>
    /// Hiển thị thông báo lỗi nghiệp vụ.
    /// </summary>
    protected void BaoLoi(string thongDiep) => ThongBao.Notify(new NotificationMessage
    {
        Severity = NotificationSeverity.Warning,
        Summary = "Không thể thực hiện",
        Detail = thongDiep,
        Duration = ThoiGianThongBao
    });

    /// <summary>
    /// Hỏi xác nhận trước khi xóa.
    /// </summary>
    protected async Task<bool> XacNhanXoaAsync(string moTa)
    {
        var dongY = await HopThoai.Confirm($"Xóa {moTa}? Thao tác được ghi nhật ký và không thể hoàn tác.", "Xác nhận xóa",
            new ConfirmOptions { OkButtonText = "Xóa", CancelButtonText = "Hủy" });
        return dongY == true;
    }

    private void KhiDuLieuThayDoi(string tenBang)
    {
        if (!BangTheoDoi.Contains(tenBang, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        _ = InvokeAsync(async () =>
        {
            // Nhường lượt để thao tác ghi đang chạy trong cùng mạch hoàn tất trước khi nạp lại.
            await Task.Yield();
            if (_daHuy)
            {
                return;
            }

            await NapLaiAsync();
            StateHasChanged();
        });
    }

    /// <summary>
    /// Hủy đăng ký kênh thay đổi khi rời trang.
    /// </summary>
    public void Dispose()
    {
        GiaiPhong();
        _daHuy = true;
        Bus.DaThayDoi -= KhiDuLieuThayDoi;
        _khoaNap.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>Điểm mở rộng để trang con giải phóng tài nguyên riêng (bộ hẹn giờ...) khi rời trang.</summary>
    protected virtual void GiaiPhong()
    {
    }
}
