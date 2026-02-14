# MIGRATION & DATABASE SETUP GUIDE

## Các bước cài đặt Database

### 1. Cài đặt Entity Framework Core Tools

```bash
dotnet tool install --global dotnet-ef
```

Hoặc cập nhật nếu đã cài:

```bash
dotnet tool update --global dotnet-ef
```

### 2. Kiểm tra version

```bash
dotnet ef --version
```

### 3. Tạo Migration

Từ thư mục `Chapter06/MvPresentation`:

```bash
dotnet ef migrations add InitialCreate -p ../MvInfrastructure -s . -o Persistence/Migrations
```

**Giải thích các tham số:**

- `-p ../MvInfrastructure`: Project chứa DbContext
- `-s .`: Startup project (MvPresentation)
- `-o Persistence/Migrations`: Thư mục output cho migration files

### 4. Review Migration

Migration files sẽ được tạo trong `MvInfrastructure/Persistence/Migrations/`:

- `{timestamp}_InitialCreate.cs`
- `{timestamp}_InitialCreate.Designer.cs`
- `AppDbContextModelSnapshot.cs`

### 5. Apply Migration (Tạo Database)

```bash
dotnet ef database update -p ../MvInfrastructure -s .
```

Database sẽ được tạo với:

- Bảng `Users` (từ AppUser)
- Bảng `AspNetUserClaims`
- Bảng `AspNetUserLogins`
- Bảng `AspNetUserTokens`
- Default admin user (username: admin, password: Admin@123)

### 6. Kiểm tra Database

#### Với SQL Server LocalDB:

```bash
sqllocaldb info
```

#### Connect với SQL Server Management Studio (SSMS):

- Server: `(localdb)\mssqllocaldb`
- Database: `Chapter06IdentityDb_Dev` (Development) hoặc `Chapter06IdentityDb` (Production)

### 7. View Data

```sql
-- Xem tất cả users
SELECT * FROM Users;

-- Xem admin user (seeded data)
SELECT Id, UserName, Email, Role, CreatedAt FROM Users WHERE Role = 'Admin';
```

---

## Các Commands Hữu Ích

### Drop Database và Recreate

```bash
# Drop database
dotnet ef database drop -p ../MvInfrastructure -s .

# Recreate
dotnet ef database update -p ../MvInfrastructure -s .
```

### Xem SQL Script sẽ được execute

```bash
dotnet ef migrations script -p ../MvInfrastructure -s .
```

### Remove Migration (chưa apply)

```bash
dotnet ef migrations remove -p ../MvInfrastructure -s .
```

### List tất cả Migrations

```bash
dotnet ef migrations list -p ../MvInfrastructure -s .
```

### Revert về migration cụ thể

```bash
dotnet ef database update {MigrationName} -p ../MvInfrastructure -s .
```

---

## Connection Strings

### Development (LocalDB)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=Chapter06IdentityDb_Dev;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### Production (SQL Server)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server.database.windows.net;Database=Chapter06IdentityDb;User Id=your-username;Password=your-password;Encrypt=true;TrustServerCertificate=false"
  }
}
```

### Docker SQL Server

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=Chapter06IdentityDb;User Id=sa;Password=YourStrong@Passw0rd;Encrypt=false"
  }
}
```

---

## Seeded Data

Default admin user được seed khi tạo database:

```
Username: admin
Email: admin@example.com
Password: Admin@123
Role: Admin
```

**Lưu ý**: Trong production, nên đổi password này ngay sau khi deploy!

---

## Troubleshooting

### Lỗi: "No executable found matching command 'dotnet-ef'"

**Giải pháp**: Cài đặt EF Core tools:

```bash
dotnet tool install --global dotnet-ef
```

### Lỗi: "Your startup project 'MvInfrastructure' doesn't reference Microsoft.EntityFrameworkCore.Design"

**Giải pháp**: Đảm bảo bạn đang ở thư mục MvPresentation và sử dụng tham số `-s .`

### Lỗi: "A connection was successfully established... but then an error occurred"

**Nguyên nhân**: SQL Server chưa chạy
**Giải pháp**:

```bash
# Start LocalDB
sqllocaldb start mssqllocaldb

# Hoặc start SQL Server service
net start MSSQLSERVER
```

### Lỗi: "Cannot open database... CREATE DATABASE is not allowed"

**Nguyên nhân**: User không có quyền tạo database
**Giải pháp**: Chạy với admin privileges hoặc tạo database thủ công trước

### Lỗi Migration conflict

**Giải pháp**: Reset migrations:

```bash
# 1. Drop database
dotnet ef database drop -p ../MvInfrastructure -s . --force

# 2. Xóa thư mục Migrations
rm -rf ../MvInfrastructure/Persistence/Migrations

# 3. Tạo migration mới
dotnet ef migrations add InitialCreate -p ../MvInfrastructure -s . -o Persistence/Migrations

# 4. Apply migration
dotnet ef database update -p ../MvInfrastructure -s .
```

---

## Production Deployment

### 1. Generate SQL Script

```bash
dotnet ef migrations script -p ../MvInfrastructure -s . -o migration.sql
```

### 2. Review Script

Mở file `migration.sql` và review tất cả changes

### 3. Apply to Production Database

- Sử dụng SQL Server Management Studio
- Hoặc Azure Data Studio
- Hoặc execute via command line tools

### 4. Verify

```sql
-- Check tables
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';

-- Check seeded admin
SELECT * FROM Users WHERE Role = 'Admin';
```

---

## Backup & Restore

### Backup Database

```bash
sqlcmd -S (localdb)\mssqllocaldb -Q "BACKUP DATABASE [Chapter06IdentityDb_Dev] TO DISK = N'C:\Backups\Chapter06_backup.bak'"
```

### Restore Database

```bash
sqlcmd -S (localdb)\mssqllocaldb -Q "RESTORE DATABASE [Chapter06IdentityDb_Dev] FROM DISK = N'C:\Backups\Chapter06_backup.bak' WITH REPLACE"
```

---

## Database Schema Overview

### Users Table

| Column               | Type             | Description                     |
| -------------------- | ---------------- | ------------------------------- |
| Id                   | uniqueidentifier | Primary Key                     |
| UserName             | nvarchar(256)    | Username (unique)               |
| NormalizedUserName   | nvarchar(256)    | Uppercase username for searches |
| Email                | nvarchar(256)    | Email (unique)                  |
| NormalizedEmail      | nvarchar(256)    | Uppercase email                 |
| EmailConfirmed       | bit              | Email confirmation status       |
| PasswordHash         | nvarchar(MAX)    | Hashed password                 |
| SecurityStamp        | nvarchar(MAX)    | Used for token invalidation     |
| PhoneNumber          | nvarchar(MAX)    | Phone number                    |
| PhoneNumberConfirmed | bit              | Phone confirmation status       |
| TwoFactorEnabled     | bit              | 2FA status                      |
| LockoutEnd           | datetimeoffset   | Lockout expiry time             |
| LockoutEnabled       | bit              | Lockout feature enabled         |
| AccessFailedCount    | int              | Failed login attempts           |
| FullName             | nvarchar(100)    | Custom: Full name               |
| Role                 | nvarchar(MAX)    | Custom: User role (Admin/User)  |
| CreatedAt            | datetime2        | Custom: Creation timestamp      |
| LastLoginAt          | datetime2        | Custom: Last login timestamp    |

---

## Next Steps

Sau khi setup database thành công:

1. ✅ Chạy ứng dụng: `dotnet run`
2. ✅ Test với Swagger UI
3. ✅ Login với admin account
4. ✅ Tạo user mới
5. ✅ Test authorization với các roles khác nhau

---

## Additional Resources

- [EF Core Migrations](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [SQL Server LocalDB](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)
- [Connection Strings](https://www.connectionstrings.com/sql-server/)
