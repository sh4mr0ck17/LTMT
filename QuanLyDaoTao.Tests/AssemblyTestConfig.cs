using Xunit;

// Vô hiệu hóa chạy song song các collection kiểm thử để tránh xung đột ghi đồng thời trên file CSDL SQLite dùng chung trong WebApplicationFactory
[assembly: CollectionBehavior(DisableTestParallelization = true)]
