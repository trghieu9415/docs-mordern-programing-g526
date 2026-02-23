using Microsoft.EntityFrameworkCore;
using MvDomain.Entities;

namespace MvInfrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ProductDetail> ProductDetails => Set<ProductDetail>();
    public DbSet<Tag> Tags => Set<Tag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Seed dữ liệu mẫu — chạy một lần qua Migration
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // ── 1. Categories ────────────────────────────────────────────────────
        modelBuilder.Entity<Category>().HasData(
            new { Id = 1, Name = "Laptop & Máy tính" },
            new { Id = 2, Name = "Phụ kiện bàn phím & Chuột" },
            new { Id = 3, Name = "Màn hình" },
            new { Id = 4, Name = "Âm thanh" }
        );

        // ── 2. Tags ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Tag>().HasData(
            new { Id = 1, Name = "Gaming" },
            new { Id = 2, Name = "Cơ học" },
            new { Id = 3, Name = "Không dây" },
            new { Id = 4, Name = "4K" },
            new { Id = 5, Name = "Chống ồn" },
            new { Id = 6, Name = "RGB" }
        );

        // ── 3. Products (với CategoryId) ────────────────────────────────────
        // QUAN TRỌNG: Phải dùng Guid cố định — KHÔNG seed cột RowVersion
        var p1 = Guid.Parse("11111111-0000-0000-0000-000000000001");
        var p2 = Guid.Parse("11111111-0000-0000-0000-000000000002");
        var p3 = Guid.Parse("11111111-0000-0000-0000-000000000003");
        var p4 = Guid.Parse("11111111-0000-0000-0000-000000000004");
        var p5 = Guid.Parse("11111111-0000-0000-0000-000000000005");

        modelBuilder.Entity<Product>().HasData(
            new { Id = p1, Name = "Laptop Gaming ASUS ROG",      Price = 35_000_000m, Stock = 12, ImageUrl = (string?)"https://placehold.co/600x400", CategoryId = (int?)1 },
            new { Id = p2, Name = "Bàn phím cơ Akko v3",         Price =  1_500_000m, Stock = 50, ImageUrl = (string?)"https://placehold.co/600x400", CategoryId = (int?)2 },
            new { Id = p3, Name = "Chuột Logitech G502",          Price =  1_200_000m, Stock = 35, ImageUrl = (string?)"https://placehold.co/600x400", CategoryId = (int?)2 },
            new { Id = p4, Name = "Màn hình Dell UltraSharp 27\"",Price =  8_000_000m, Stock = 20, ImageUrl = (string?)"https://placehold.co/600x400", CategoryId = (int?)3 },
            new { Id = p5, Name = "Tai nghe Sony WH-1000XM5",     Price =  9_000_000m, Stock = 25, ImageUrl = (string?)"https://placehold.co/600x400", CategoryId = (int?)4 }
        );

        // ── 4. ProductDetails (quan hệ 1-1) ─────────────────────────────────
        modelBuilder.Entity<ProductDetail>().HasData(
            new {
                Id = Guid.Parse("22222222-0000-0000-0000-000000000001"),
                ProductId = p1,
                Description = "Laptop gaming cao cấp với CPU AMD Ryzen 9, GPU RTX 4070, RAM 32GB DDR5.",
                Specification = (string?)"CPU: AMD Ryzen 9 7945HX | GPU: RTX 4070 8GB | RAM: 32GB DDR5 | SSD: 1TB NVMe"
            },
            new {
                Id = Guid.Parse("22222222-0000-0000-0000-000000000002"),
                ProductId = p2,
                Description = "Bàn phím cơ 75% layout với switch Akko CS Jelly Pink, hotswap, LED RGB.",
                Specification = (string?)"Layout: 75% | Switch: Akko CS Jelly Pink | LED: RGB | Kết nối: USB-C"
            },
            new {
                Id = Guid.Parse("22222222-0000-0000-0000-000000000003"),
                ProductId = p3,
                Description = "Chuột gaming có dây với sensor HERO 25K, 11 nút lập trình, trọng lượng điều chỉnh.",
                Specification = (string?)"Sensor: HERO 25K | DPI: 100–25600 | Nút: 11 | Trọng lượng: điều chỉnh được"
            },
            new {
                Id = Guid.Parse("22222222-0000-0000-0000-000000000004"),
                ProductId = p4,
                Description = "Màn hình IPS 27 inch 4K UHD, độ phủ màu 99% sRGB, cổng USB-C 90W.",
                Specification = (string?)"Kích thước: 27\" | Độ phân giải: 4K UHD | Tấm nền: IPS | Refresh: 60Hz | USB-C: 90W"
            },
            new {
                Id = Guid.Parse("22222222-0000-0000-0000-000000000005"),
                ProductId = p5,
                Description = "Tai nghe over-ear chống ồn chủ động hàng đầu thế giới, pin 30 giờ, kết nối đa điểm.",
                Specification = (string?)"Driver: 30mm | Chống ồn: ANC | Pin: 30h | Kết nối: Bluetooth 5.2 + 3.5mm"
            }
        );

        // ── 5. ProductTags (quan hệ n-n — seed bảng join) ───────────────────
        // EF Core seed bảng join bằng anonymous type với đúng tên FK
        modelBuilder.Entity("ProductTag").HasData(
            new { ProductsId = p1, TagsId = 1 },  // Laptop → Gaming
            new { ProductsId = p1, TagsId = 6 },  // Laptop → RGB
            new { ProductsId = p2, TagsId = 2 },  // Bàn phím → Cơ học
            new { ProductsId = p2, TagsId = 6 },  // Bàn phím → RGB
            new { ProductsId = p3, TagsId = 1 },  // Chuột → Gaming
            new { ProductsId = p3, TagsId = 6 },  // Chuột → RGB
            new { ProductsId = p4, TagsId = 4 },  // Màn hình → 4K
            new { ProductsId = p5, TagsId = 3 },  // Tai nghe → Không dây
            new { ProductsId = p5, TagsId = 5 }   // Tai nghe → Chống ồn
        );
    }
}
