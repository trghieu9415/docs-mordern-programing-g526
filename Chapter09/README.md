# CHƯƠNG 9: TÍCH HỢP DỊCH VỤ BÊN THỨ 3

Dự án minh họa việc xây dựng một hệ thống `Event Ticketing System` theo kiến trúc phân tầng gồm `Presentation`, `Application`, `Infrastructure` và `Domain`. Trọng tâm của chương là tích hợp dịch vụ bên thứ 3 theo hướng tách biệt nghiệp vụ với hạ tầng, giúp hệ thống dễ thay thế nhà cung cấp, dễ mở rộng và dễ kiểm thử.

## 1. Yêu cầu về Kiến trúc Tích hợp
- Thiết kế kiến trúc tuân thủ nguyên lý `Dependency Inversion`.
- Định nghĩa các interface trừu tượng tại tầng nghiệp vụ như `IStorageService`, `IPaymentService`, `IEmailService`, `IEventRepository`, `ITicketOrderRepository`.
- Triển khai `Adapter Pattern` cho cổng thanh toán với ít nhất hai implementation của `IPaymentService`, gồm `StripePaymentService` và `PayPalPaymentService`.
- Sử dụng `Factory Pattern` thông qua `IPaymentServiceFactory` để chọn cổng thanh toán tương ứng mà không phải sửa controller hay use case.

## 2. Yêu cầu về Lưu trữ Dữ liệu
- Tích hợp `Object Storage` theo chuẩn S3-compatible, phù hợp để chạy với `MinIO` hoặc `AWS S3`.
- Xây dựng API upload ảnh poster cho sự kiện.
- Ảnh được nhận từ request, upload lên bucket và lưu `PosterUrl` vào cơ sở dữ liệu của thực thể sự kiện.

## 3. Yêu cầu về Dịch vụ Email
- Tích hợp dịch vụ gửi email bằng `MailKit`.
- Sau khi thanh toán thành công, hệ thống gửi email chứa thông tin vé điện tử cho khách hàng, gồm mã đơn, tên sự kiện, số lượng vé và mã vé điện tử.

## 4. Yêu cầu về Cổng thanh toán
- Tích hợp hoặc mô phỏng sandbox của cổng thanh toán thực tế.
- Hệ thống hỗ trợ hai cổng thanh toán là `Stripe` và `PayPal`.
- Luồng thanh toán bao gồm:
  - Khởi tạo URL thanh toán an toàn.
  - Xử lý `Return URL` để đối soát trạng thái giao dịch.
  - Xử lý `Webhook` để đồng bộ trạng thái từ phía cổng thanh toán.
  - Áp dụng `Idempotency` để tránh xử lý trùng lặp đơn hàng đã chốt thành công.

## 5. Yêu cầu về Cơ chế Chịu lỗi
- Dùng `Polly` để bọc các `HttpClient` gọi ra dịch vụ bên ngoài.
- Áp dụng `Retry Pattern` tối thiểu 3 lần với chiến lược `Exponential Backoff`.
- Áp dụng `Circuit Breaker Pattern` mở mạch trong 30 giây khi lỗi liên tục vượt ngưỡng cho phép.

## 6. Triển khai hệ thống

### 6.a. Triển khai kiến trúc tích hợp
Phần kiến trúc được triển khai theo hướng tách biệt rõ ràng giữa nghiệp vụ và hạ tầng. Tầng `Application` chỉ làm việc với các interface như `IStorageService`, `IPaymentService`, `IEmailService`, `IEventRepository`, `ITicketOrderRepository`. Tầng `Infrastructure` chịu trách nhiệm cài đặt cụ thể các interface này.

Trong phần thanh toán, hệ thống có hai adapter là `StripePaymentService` và `PayPalPaymentService`, cùng triển khai `IPaymentService`. Việc lựa chọn cổng thanh toán được thực hiện qua `PaymentServiceFactory`, cho phép use case xử lý đơn vé hoạt động thống nhất dù người dùng chọn Stripe hay PayPal.

Kết quả đạt được:
- Controller không phụ thuộc trực tiếp vào SDK hay API của bên thứ 3.
- Use case thanh toán chỉ làm việc với abstraction.
- Có thể mở rộng thêm cổng thanh toán mới mà không phải sửa luồng nghiệp vụ hiện tại.

Ghi chú minh họa nên chụp:
- Ảnh cấu trúc solution thể hiện rõ 4 tầng `Domain`, `Application`, `Infrastructure`, `Presentation`.
- Ảnh code interface `IPaymentService` và hai lớp `StripePaymentService`, `PayPalPaymentService`.
- Ảnh code `PaymentServiceFactory` hoặc cấu hình DI trong `InfrastructureConfiguration`.

### 6.b. Triển khai lưu trữ dữ liệu và upload poster sự kiện
Phần lưu trữ poster được triển khai qua interface `IStorageService`, còn phần cài đặt cụ thể dùng `S3StorageService`. Nhờ sử dụng chuẩn S3-compatible, hệ thống có thể chạy với `MinIO` khi demo local hoặc chuyển sang `AWS S3` khi cần.

Luồng xử lý upload poster:
1. Người dùng gọi API upload poster cho một sự kiện.
2. API nhận file từ request.
3. `UploadPosterHandler` gọi `IStorageService.UploadAsync(...)`.
4. File được upload lên bucket `event-posters`.
5. Hệ thống lấy URL công khai của file và cập nhật vào trường `PosterUrl` trong bảng `Events`.

Kết quả đạt được:
- Poster được lưu trên object storage thay vì lưu trực tiếp trong database.
- URL ảnh được lưu trong SQLite để hiển thị lại khi truy vấn sự kiện.
- Tầng nghiệp vụ không phụ thuộc vào MinIO hay AWS SDK cụ thể.

Ghi chú ảnh chụp bắt buộc:
- Ảnh giao diện `MinIO` hiển thị file poster vừa upload thành công trong bucket.
- Ảnh kết quả database bảng `Events` có cột `PosterUrl` đã được cập nhật.
- Ảnh Swagger hoặc Postman khi gọi API `POST /api/events/{eventId}/poster`.

### 6.c. Triển khai dịch vụ email bằng MailKit
Phần gửi email được triển khai thông qua interface `IEmailService`, còn lớp `SmtpEmailService` dùng `MailKit` để thực hiện kết nối SMTP và gửi nội dung vé điện tử cho người dùng.

Luồng xử lý email:
1. Người dùng hoàn tất thanh toán.
2. Hệ thống xác nhận giao dịch ở `Return URL` hoặc `Webhook`.
3. Use case tạo mã vé điện tử cho đơn hàng.
4. `IEmailService.SendTicketIssuedAsync(...)` được gọi để gửi email xác nhận.

Nội dung email gồm:
- Tên sự kiện.
- Mã đơn hàng.
- Số lượng vé.
- Mã vé điện tử dùng để check-in.

Kết quả đạt được:
- Email được gửi tự động sau khi đơn vé được chốt thành công.
- Việc thay đổi nhà cung cấp email trong tương lai không làm ảnh hưởng tới use case nghiệp vụ.

Ghi chú ảnh chụp bắt buộc:
- Ảnh inbox nhận được email xác nhận vé sau khi thanh toán hoàn tất.
- Ảnh nội dung email thể hiện rõ mã vé điện tử hoặc thông tin vé.
- Ảnh log console hoặc Swagger chứng minh endpoint thanh toán đã xử lý xong trước khi email được gửi.

### 6.d. Triển khai cổng thanh toán Stripe và PayPal
Hệ thống hỗ trợ hai cổng thanh toán:
- `Stripe`: tích hợp thông qua thư viện `Stripe.net`.
- `PayPal`: tích hợp theo hướng gọi `HttpClient` tới API sandbox.

Luồng thanh toán được triển khai như sau:
1. Người dùng gọi API `POST /api/ticket-orders/checkout`.
2. Hệ thống tạo đơn vé ở trạng thái `Pending`.
3. `PaymentServiceFactory` chọn adapter tương ứng với cổng thanh toán.
4. Adapter tạo `PaymentUrl` chứa mã đơn hàng và thông tin thanh toán.
5. Người dùng được chuyển sang trang thanh toán của Stripe hoặc PayPal.
6. Sau khi thanh toán xong, cổng thanh toán redirect về `Return URL`.
7. Hệ thống gọi trực tiếp tới cổng thanh toán để đối soát lại trạng thái giao dịch trước khi xác nhận thành công.

Về `Idempotency`:
- Nếu đơn hàng đã ở trạng thái `Paid`, các tín hiệu `Return URL` hoặc `Webhook` đến sau sẽ không xử lý lại.
- Điều này giúp tránh trường hợp một đơn hàng bị xác nhận thành công hai lần hoặc phát hành vé hai lần.

Kết quả đạt được:
- Hỗ trợ song song hai cổng thanh toán với cùng một luồng nghiệp vụ.
- Tách biệt rõ logic nghiệp vụ và logic tích hợp.
- Đáp ứng đủ vòng đời giao dịch gồm tạo checkout, return và webhook.

Ghi chú minh họa nên chụp:
- Ảnh Swagger/Postman khi gọi `POST /api/ticket-orders/checkout`.
- Ảnh trang thanh toán Stripe hoặc PayPal sandbox sau khi mở `PaymentUrl`.
- Ảnh response từ `GET /api/ticket-orders/return`.
- Ảnh database bảng `TicketOrders` thể hiện trạng thái đơn từ `Pending` sang `Paid`.
- Ảnh thử gọi lại `Return URL` hoặc `Webhook` để chứng minh cơ chế idempotency đang hoạt động.

### 6.e. Triển khai cơ chế chịu lỗi với Polly
Phần chịu lỗi được triển khai cho các `HttpClient` gọi ra dịch vụ bên ngoài, cụ thể là `PayPalApi`. Hệ thống sử dụng `Polly` để bọc request và áp dụng hai chính sách chính là `Retry` và `Circuit Breaker`.

`Retry Pattern`:
- Khi dịch vụ bên ngoài trả lỗi mạng, lỗi `5xx`, timeout hoặc `429`, request sẽ tự động thử lại 3 lần.
- Thời gian chờ giữa các lần thử tăng dần theo `2s`, `4s`, `8s`.

`Circuit Breaker Pattern`:
- Nếu số lần lỗi liên tiếp vượt ngưỡng, mạch sẽ chuyển sang trạng thái `Open`.
- Trong 30 giây tiếp theo, các request mới sẽ bị từ chối ngay lập tức thay vì tiếp tục gọi ra dịch vụ lỗi.
- Sau khoảng thời gian này, hệ thống chuyển sang `Half-Open` để kiểm tra lại trạng thái dịch vụ.

Kết quả đạt được:
- Hệ thống tránh bị treo hoặc gọi lặp vô ích khi dịch vụ bên ngoài đang lỗi.
- Console log thể hiện rõ từng lần retry và thời điểm circuit breaker mở mạch.
- Giúp việc quan sát và demo cơ chế chịu lỗi trở nên rõ ràng hơn.

Ghi chú ảnh chụp bắt buộc:
- Cố tình nhập sai `BaseUrl` hoặc endpoint của dịch vụ bên thứ 3 để giả lập lỗi.
- Ảnh console log hiển thị `Retry lan 1`, `Retry lan 2`, `Retry lan 3` trước khi văng exception.
- Ảnh console log hiển thị `Circuit Breaker OPEN` và các request sau đó bị từ chối ngay.

## 7. Kết quả triển khai
Mini project `Event Ticketing System` đã triển khai đầy đủ các yêu cầu chính của chương:
- Tích hợp dịch vụ bên thứ 3 theo nguyên lý `Dependency Inversion`.
- Hỗ trợ object storage để lưu poster sự kiện.
- Hỗ trợ gửi email vé điện tử bằng `MailKit`.
- Hỗ trợ thanh toán qua `Stripe` và `PayPal`.
- Áp dụng `Polly` để tăng khả năng chịu lỗi khi làm việc với dịch vụ bên ngoài.
- Lưu toàn bộ dữ liệu nghiệp vụ bằng `SQLite` để dễ demo và kiểm tra.

## 8. Hướng dẫn chèn ảnh minh họa vào báo cáo
Khi hoàn thiện báo cáo, có thể chèn ảnh theo thứ tự sau:
- Hình 6.a.1: Cấu trúc solution và các interface tích hợp.
- Hình 6.b.1: File poster xuất hiện trong bucket MinIO.
- Hình 6.b.2: Bảng `Events` có `PosterUrl`.
- Hình 6.c.1: Inbox nhận email vé điện tử.
- Hình 6.d.1: Gọi API tạo checkout.
- Hình 6.d.2: Trang thanh toán Stripe hoặc PayPal sandbox.
- Hình 6.d.3: Bảng `TicketOrders` sau khi thanh toán thành công.
- Hình 6.e.1: Console log của Retry.
- Hình 6.e.2: Console log của Circuit Breaker.

## 9. Ghi chú khi demo
- Nếu demo upload poster, cần chạy `MinIO` hoặc dịch vụ S3-compatible trước.
- Nếu demo email, cần cấu hình SMTP hợp lệ trong `appsettings.json`.
- Nếu demo Stripe hoặc PayPal thật, cần cấu hình key sandbox tương ứng.
- Nếu muốn minh họa Polly rõ ràng, nên cố tình sửa sai `BaseUrl` của PayPal trong cấu hình trước khi gọi API.
