using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // 引入 ForeignKey
using Microsoft.AspNetCore.Identity; // 引入 IdentityUser
using System; // 引入 DateTime

namespace E1.Backend.Api.Models
{
    public class IncidentReport
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        // --- 以下是新添加的字段 (修复 CS0117 错误) ---
        [Required]
        public string Status { get; set; } // (例如: "Submitted", "In Progress")

        public DateTime ReportedAt { get; set; } // (控制器需要这个)

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public string? PhotoUrl { get; set; }

        // --- 用于关联 E1 的用户 ID (修复 CS0117 错误) ---
        [Required]
        public string UserId { get; set; } // 提交报告的用户ID

        [ForeignKey("UserId")]
        public virtual IdentityUser User { get; set; } // 导航属性 (可选，但推荐)
    }
}