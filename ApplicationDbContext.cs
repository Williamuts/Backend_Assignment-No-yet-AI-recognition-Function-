using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using E1.Backend.Api.Models; // <-- 1. 导入您的 "Models" 命名空间

// 命名空间应匹配您的新项目名称，例如 "E1.Backend.Api"
namespace E1.Backend.Api
{
    // 您的类必须继承自 IdentityDbContext
    public class ApplicationDbContext : IdentityDbContext
    {
        // --- E3 的表 (已添加) ---
        public DbSet<RecyclingSite> RecyclingSites { get; set; }

        // --- E4 的新表 (已添加) ---
        public DbSet<IncidentReport> IncidentReports { get; set; }
        // -------------------------

        // --- 关键: 添加 E6 的新表 ---
        public DbSet<UserDevice> UserDevices { get; set; }
        // -------------------------

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}