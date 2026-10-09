using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Data;

namespace QuanLyDaoTao.Tests.Helpers;

/// <summary>
/// Lớp hỗ trợ khởi tạo DbContext phục vụ kiểm thử đơn vị với cơ sở dữ liệu độc lập.
/// </summary>
public static class CoSoDuLieuTestHelper
{
    /// <summary>
    /// Tạo một phiên bản DbContext dùng cơ sở dữ liệu In-Memory độc lập với tên duy nhất.
    /// </summary>
    /// <param name="tenDb">Tên database tùy chọn (mặc định sinh GUID mới).</param>
    /// <returns>Đối tượng QuanLyDaoTaoDbContext đã sẵn sàng.</returns>
    public static QuanLyDaoTaoDbContext TaoDbContextAo(string? tenDb = null)
    {
        var dbName = tenDb ?? Guid.NewGuid().ToString("N");
        var options = new DbContextOptionsBuilder<QuanLyDaoTaoDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new QuanLyDaoTaoDbContext(options);
    }
}
