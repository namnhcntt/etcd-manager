# [claudecode] Security Remediation Summary — etcd-manager

**Ngày thực hiện:** 2026-05-31
**Dựa trên báo cáo:** `docs/claudecode-security-scan-report-2026-05-31.md`

## Đã vá

| Vuln | Severity | Mô tả | Fix |
|------|----------|-------|-----|
| Vuln 1 | High | Hardcoded JWT Key | Xóa key khỏi appsettings.json, inject qua env var `Jwt__Key` |
| Vuln 2 | High | Unsalted SHA256 password | Thay bằng BCrypt workFactor 12 |
| Vuln 3 | High | Default credential root/root | Bắt buộc env var `ROOT_ACCOUNT_PASSWORD`, throw nếu thiếu |
| Vuln 4 | High | etcd password lộ trong API response | Xóa `Password` field khỏi GET connection DTO |
| Vuln 6 | High | CORS AllowAnyOrigin | Restrict về `AllowedOrigins` từ config |
| Vuln 7 | Medium | JWT ValidateAudience=false | Bật `ValidateAudience=true` |
| Vuln 8 | Medium | JWT encoding ASCII vs UTF8 | Đồng nhất về `Encoding.UTF8` |
| Vuln 9 | Medium | Log tiết lộ default password | Xóa log message chứa password |

## Chưa vá (cần sprint riêng)

| Vuln | Severity | Lý do chưa vá |
|------|----------|---------------|
| Vuln 5 | High | Cần DB migration để encrypt etcd credentials — scope lớn |
| Vuln 10 | Medium | Implement PermissionUsers enforcement — thay đổi logic nghiệp vụ |

## Hướng dẫn deploy

Các env var bắt buộc khi chạy production:
- `ROOT_ACCOUNT_PASSWORD` — password cho tài khoản root (bắt buộc)
- `Jwt__Key` — JWT signing key, tối thiểu 256-bit (bắt buộc)

Ví dụ:
```bash
export ROOT_ACCOUNT_PASSWORD="your-strong-password-here"
export Jwt__Key="your-256-bit-random-key-here"
dotnet run
```

## Tests

11 unit tests được tạo mới để bảo vệ các fix:
- `PasswordHashTests` (3 tests) — BCrypt hash/verify
- `LoginCommandTests` (2 tests) — Login flow
- `SeedDataTests` (2 tests) — ROOT_ACCOUNT_PASSWORD enforcement
- `GetConnectionTests` (2 tests) — Password không lộ trong response
- `TokenServiceTests` (2 tests) — JWT generate/refresh với UTF8 encoding
