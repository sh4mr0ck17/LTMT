# ==============================================================================
# HỌC VIỆN AN NINH NHÂN DÂN (C500) - PRODUCTION DOCKERFILE (.NET 8 MULTI-STAGE)
# ==============================================================================

# Giai đoạn 1: Biên dịch mã nguồn trên nền tảng .NET 8 SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy project file và restore cache
COPY ["QuanLyDaoTao/QuanLyDaoTao.csproj", "QuanLyDaoTao/"]
RUN dotnet restore "QuanLyDaoTao/QuanLyDaoTao.csproj"

# Copy toàn bộ mã nguồn và xuất bản release
COPY . .
WORKDIR "/src/QuanLyDaoTao"
RUN dotnet publish "QuanLyDaoTao.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Giai đoạn 2: Môi trường chạy tối giản, bảo mật cao trên nền Alpine Linux
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app

# Cài đặt thư viện cần thiết phục vụ Globalization và Health Check
RUN apk add --no-cache curl icu-libs

# Tạo người dùng an toàn không có đặc quyền root
RUN addgroup -S c500group && adduser -S c500user -G c500group
RUN mkdir -p /app/data && chown -R c500user:c500group /app

COPY --from=build --chown=c500user:c500group /app/publish .

USER c500user

ENV ASPNETCORE_URLS=http://+:5219 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    ConnectionStrings__DefaultConnection="Data Source=/app/data/QuanLyDaoTao.db"

EXPOSE 5219

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD curl --fail http://localhost:5219/healthz || exit 1

ENTRYPOINT ["dotnet", "QuanLyDaoTao.dll"]
