using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Models;

namespace QuanLyDaoTao.Data;

/// <summary>
/// Ngữ cảnh cơ sở dữ liệu Entity Framework Core cho Hệ thống Quản lý Đào tạo.
/// </summary>
public class QuanLyDaoTaoDbContext : IdentityDbContext<TaiKhoan, VaiTro, string>
{
    /// <summary>
    /// Khởi tạo ngữ cảnh dữ liệu với các tùy chọn cấu hình.
    /// </summary>
    /// <param name="options">Tùy chọn cấu hình DbContext.</param>
    public QuanLyDaoTaoDbContext(DbContextOptions<QuanLyDaoTaoDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Bảng dữ liệu Khoa.
    /// </summary>
    public DbSet<Khoa> Khoas => Set<Khoa>();

    /// <summary>
    /// Bảng dữ liệu Bộ môn.
    /// </summary>
    public DbSet<BoMon> BoMons => Set<BoMon>();

    /// <summary>
    /// Bảng dữ liệu Lớp học.
    /// </summary>
    public DbSet<LopHoc> LopHocs => Set<LopHoc>();

    /// <summary>
    /// Bảng dữ liệu Học viên.
    /// </summary>
    public DbSet<HocVien> HocViens => Set<HocVien>();

    /// <summary>
    /// Bảng dữ liệu Nhân sự / Giảng viên.
    /// </summary>
    public DbSet<NhanSu> NhanSus => Set<NhanSu>();

    /// <summary>
    /// Bảng dữ liệu Chức vụ.
    /// </summary>
    public DbSet<ChucVu> ChucVus => Set<ChucVu>();

    /// <summary>
    /// Bảng dữ liệu Trình độ chuyên môn.
    /// </summary>
    public DbSet<TrinhDoChuyenMon> TrinhDoChuyenMons => Set<TrinhDoChuyenMon>();

    /// <summary>
    /// Bảng dữ liệu Môn học.
    /// </summary>
    public DbSet<MonHoc> MonHocs => Set<MonHoc>();

    /// <summary>
    /// Bảng dữ liệu Phân công giảng dạy.
    /// </summary>
    public DbSet<PhanCongGiangDay> PhanCongGiangDays => Set<PhanCongGiangDay>();

    /// <summary>
    /// Bảng dữ liệu Nhật ký hệ thống / Kiểm toán.
    /// </summary>
    public DbSet<NhatKyHeThong> NhatKyHeThongs => Set<NhatKyHeThong>();

    /// <summary>
    /// Bảng cấu hình các trường (cột) tùy chỉnh của từng nhóm quản lý.
    /// </summary>
    public DbSet<TruongTuyChinh> TruongTuyChinhs => Set<TruongTuyChinh>();

    /// <summary>
    /// Bảng lưu trữ giá trị của các trường tùy chỉnh cho từng bản ghi.
    /// </summary>
    public DbSet<GiaTriTruongTuyChinh> GiaTriTruongTuyChinhs => Set<GiaTriTruongTuyChinh>();

    /// <summary>
    /// Cấu hình các ràng buộc, chỉ mục và mối quan hệ giữa các thực thể.
    /// </summary>
    /// <param name="modelBuilder">Đối tượng ModelBuilder để cấu hình lược đồ.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        base.OnModelCreating(modelBuilder);

        // ==========================================
        // CẤU HÌNH KHOA
        // ==========================================
        modelBuilder.Entity<Khoa>(entity =>
        {
            entity.HasKey(k => k.KhoaId);
            entity.HasIndex(k => k.MaKhoa).IsUnique();
            entity.HasIndex(k => k.TenKhoa);
        });

        // ==========================================
        // CẤU HÌNH BỘ MÔN
        // ==========================================
        modelBuilder.Entity<BoMon>(entity =>
        {
            entity.HasKey(bm => bm.BoMonId);
            entity.HasIndex(bm => bm.MaBoMon).IsUnique();
            entity.HasIndex(bm => bm.TenBoMon);

            entity.HasOne(bm => bm.Khoa)
                  .WithMany(k => k.BoMons)
                  .HasForeignKey(bm => bm.KhoaId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ==========================================
        // CẤU HÌNH LỚP HỌC
        // ==========================================
        modelBuilder.Entity<LopHoc>(entity =>
        {
            entity.HasKey(l => l.LopHocId);
            entity.HasIndex(l => l.MaLop).IsUnique();
            entity.HasIndex(l => l.TenLop);
            entity.HasIndex(l => l.NienKhoa);

            entity.HasOne(l => l.Khoa)
                  .WithMany(k => k.LopHocs)
                  .HasForeignKey(l => l.KhoaId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ==========================================
        // CẤU HÌNH HỌC VIÊN
        // ==========================================
        modelBuilder.Entity<HocVien>(entity =>
        {
            entity.HasKey(hv => hv.HocVienId);
            entity.HasIndex(hv => hv.MaHocVien).IsUnique();
            entity.HasIndex(hv => hv.HoVaTen);
            entity.HasIndex(hv => hv.LopHocId);

            entity.HasOne(hv => hv.LopHoc)
                  .WithMany(l => l.HocViens)
                  .HasForeignKey(hv => hv.LopHocId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ==========================================
        // CẤU HÌNH CHỨC VỤ
        // ==========================================
        modelBuilder.Entity<ChucVu>(entity =>
        {
            entity.HasKey(cv => cv.ChucVuId);
            entity.HasIndex(cv => cv.MaChucVu).IsUnique();
        });

        // ==========================================
        // CẤU HÌNH TRÌNH ĐỘ CHUYÊN MÔN
        // ==========================================
        modelBuilder.Entity<TrinhDoChuyenMon>(entity =>
        {
            entity.HasKey(td => td.TrinhDoChuyenMonId);
            entity.HasIndex(td => td.MaTrinhDo).IsUnique();
        });

        // ==========================================
        // CẤU HÌNH MÔN HỌC
        // ==========================================
        modelBuilder.Entity<MonHoc>(entity =>
        {
            entity.HasKey(mh => mh.MonHocId);
            entity.HasIndex(mh => mh.MaMonHoc).IsUnique();
            entity.HasIndex(mh => mh.TenMonHoc);

            entity.HasOne(mh => mh.BoMon)
                  .WithMany(bm => bm.MonHocs)
                  .HasForeignKey(mh => mh.BoMonId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ==========================================
        // CẤU HÌNH NHÂN SỰ
        // ==========================================
        modelBuilder.Entity<NhanSu>(entity =>
        {
            entity.HasKey(ns => ns.NhanSuId);
            entity.HasIndex(ns => ns.MaNhanSu).IsUnique();
            entity.HasIndex(ns => ns.HoVaTen);
            entity.HasIndex(ns => ns.EmailCongViec);
            entity.HasIndex(ns => ns.KhoaId);
            entity.HasIndex(ns => ns.BoMonId);
            entity.HasIndex(ns => ns.ChucVuId);

            entity.HasOne(ns => ns.Khoa)
                  .WithMany(k => k.NhanSus)
                  .HasForeignKey(ns => ns.KhoaId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ns => ns.BoMon)
                  .WithMany(bm => bm.NhanSus)
                  .HasForeignKey(ns => ns.BoMonId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ns => ns.ChucVu)
                  .WithMany(cv => cv.NhanSus)
                  .HasForeignKey(ns => ns.ChucVuId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ns => ns.TrinhDoChuyenMon)
                  .WithMany(td => td.NhanSus)
                  .HasForeignKey(ns => ns.TrinhDoChuyenMonId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ==========================================
        // CẤU HÌNH PHÂN CÔNG GIẢNG DẠY
        // ==========================================
        modelBuilder.Entity<PhanCongGiangDay>(entity =>
        {
            entity.HasKey(pc => pc.PhanCongId);
            entity.HasIndex(pc => pc.MaPhanCong).IsUnique();
            entity.HasIndex(pc => new { pc.NhanSuId, pc.MonHocId, pc.LopHocId, pc.HocKy, pc.NamHoc });

            entity.HasOne(pc => pc.NhanSu)
                  .WithMany(ns => ns.PhanCongGiangDays)
                  .HasForeignKey(pc => pc.NhanSuId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(pc => pc.MonHoc)
                  .WithMany(mh => mh.PhanCongGiangDays)
                  .HasForeignKey(pc => pc.MonHocId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(pc => pc.LopHoc)
                  .WithMany(l => l.PhanCongGiangDays)
                  .HasForeignKey(pc => pc.LopHocId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ==========================================
        // CẤU HÌNH NHẬT KÝ HỆ THỐNG
        // ==========================================
        modelBuilder.Entity<NhatKyHeThong>(entity =>
        {
            entity.HasKey(nk => nk.NhatKyId);
            entity.HasIndex(nk => nk.ThoiDiem);
            entity.HasIndex(nk => nk.NguoiThucHien);
            entity.HasIndex(nk => nk.BangDuLieu);
        });

        // ==========================================
        // CẤU HÌNH TRƯỜNG TÙY CHỈNH & GIÁ TRỊ
        // ==========================================
        modelBuilder.Entity<TruongTuyChinh>(entity =>
        {
            entity.HasKey(t => t.TruongTuyChinhId);
            entity.HasIndex(t => new { t.TenBang, t.MaTruong }).IsUnique();
            entity.HasIndex(t => t.TenBang);
        });

        modelBuilder.Entity<GiaTriTruongTuyChinh>(entity =>
        {
            entity.HasKey(g => g.GiaTriId);
            entity.HasIndex(g => new { g.TruongTuyChinhId, g.KhoaBanGhi }).IsUnique();
            entity.HasIndex(g => new { g.TenBang, g.KhoaBanGhi });

            entity.HasOne(g => g.TruongTuyChinh)
                  .WithMany(t => t.GiaTris)
                  .HasForeignKey(g => g.TruongTuyChinhId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
