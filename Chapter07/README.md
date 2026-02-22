# CHƯƠNG 7: TỐI ƯU HIỆU NĂNG VÀ XỬ LÝ TRANH CHẤP VỚI HỆ THỐNG PHÂN TÁN

Dự án này trình bày các giải pháp tối ưu hóa hiệu năng và quản lý truy cập đồng thời trong môi trường phân tán, sử dụng Redis làm thành phần trung tâm.

## Các thành phần chính

### 1. Hệ thống phân tán và Redis
- Giới thiệu kiến trúc hệ thống phân tán và vai trò của bộ nhớ đệm ngoài.
- Cách tích hợp và cấu hình Redis trong ứng dụng .NET (StackExchange.Redis).

### 2. Chiến lược Caching
- Triển khai In-memory Cache và Distributed Cache.
- Các chiến lược lưu trữ dữ liệu đệm: Cache-Aside, Write-Through và Read-Through.
- Quản lý vòng đời dữ liệu cache (Expiration, Eviction policies).

### 3. Xử lý tranh chấp với Distributed Lock
- Khái niệm và nhu cầu sử dụng khóa phân tán khi triển khai ứng dụng trên nhiều Node/Instance.
- Triển khai Distributed Lock với Redis để ngăn chặn tình trạng Race Condition.
- Đảm bảo tính nhất quán dữ liệu cho các tác vụ quan trọng trong hệ thống có tải trọng lớn.

---

## Triển khai trong project này

### Redis Distributed Cache (Cache-Aside)
- **GET /api/products**: Danh sách sản phẩm phân trang — cache key `products:paged:{page}:{pageSize}`, TTL 2 phút.
- **GET /api/products/{id}**: Chi tiết sản phẩm — cache key `product:{id}`, TTL 5 phút.
- Khi **Create/Update/Remove** sản phẩm: xóa cache tương ứng để đảm bảo dữ liệu nhất quán.

### Distributed Lock (tránh Race Condition)
- **PUT /api/products/{id}** (UpdateProduct): Command implement `ILockable` với `LockKey = locks:product:{id}`.
- Khi một request đang cập nhật sản phẩm X, request khác cập nhật cùng sản phẩm X sẽ bị chặn (429) cho đến khi lock được giải phóng (tối đa chờ 5 giây).

### Cách chạy
1. Cài và chạy Redis (Docker: `docker run -d -p 6379:6379 redis` hoặc Redis bản cài sẵn).
2. Trong `appsettings.json`, mục `Redis:Configuration` trỏ tới Redis (mặc định `localhost:6379`).
3. `dotnet run --project MvPresentation`.

### Minh chứng Response Time (trước / sau Cache)
1. **Trước cache**: Tắt Redis hoặc xóa key cache, gọi `GET /api/products?page=1&pageSize=20` — ghi lại Response Time (Postman/Browser DevTools).
2. **Sau cache**: Bật Redis, gọi cùng endpoint lần 2 (sau khi đã có cache) — Response Time giảm rõ rệt (hit cache).
3. Chụp ảnh so sánh hai lần đo làm minh chứng.