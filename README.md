# Tài liệu & Mã nguồn: Backend Architecture & Concurrency Handling

Repository này chứa mã nguồn minh họa và tài liệu kỹ thuật từ **Chương 4 đến Chương 10**. Dự án tập trung vào việc xây dựng hệ thống Backend hiện đại, xử lý các bài toán về **Hiệu năng**, **Bảo mật** và đặc biệt là **Xử lý tranh chấp dữ liệu (Race Conditions)** trong môi trường thực tế.

## Cấu trúc Chương & Kỹ thuật

| Chương | Chủ đề chính | Kỹ thuật & Công nghệ | Điểm nhấn kỹ thuật                                                  |
| :------------ | :------------------- |:---------------------|:-----------------------------------------------------|
| **[Chương 04](./Chapter04/README.md)** | **Nền tảng Backend** | .NET API             | Xây dựng chuẩn RESTful API.                          |
| **[Chương 05](./Chapter05/README.md)** | **Quản trị Dữ liệu** | EF Core Code-First   | Xử lý xung đột bằng Optimistic Concurrency.          |
| **[Chương 06](./Chapter06/README.md)** | **Bảo mật** | JWT, Identity                 | Quản lý phiên và thu hồi Token an toàn.              |
| **[Chương 07](./Chapter07/README.md)** | **Tối ưu hiệu năng** | Redis                | Sử dụng Distributed Lock để khóa tài nguyên.         |
| **[Chương 08](./Chapter08/README.md)** | **Thời gian thực** | SignalR                | Đồng bộ trạng thái ghế tức thì tới Client.           |
| **[Chương 09](./Chapter09/README.md)** | **Dịch vụ thứ 3** | Adapter Pattern         | Đảm bảo tính nhất quán (Idempotency) khi thanh toán. |
| **[Chương 10](./Chapter10/README.md)** | **Tác vụ nền** | Quartz, MassTransit        | Xử lý tác vụ ngầm, chống trùng lặp.                  |

## Yêu cầu hệ thống

* **.NET SDK**: 9.0
* **Docker & Docker Compose**: Để chạy SQL Server, Redis, RabbitMQ.
* **Database**: SQL Server (hoặc PostgreSQL).