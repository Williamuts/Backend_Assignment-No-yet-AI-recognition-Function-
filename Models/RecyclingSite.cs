using System.ComponentModel.DataAnnotations; // 导入数据验证工具

// 我们将把所有的数据模型放在 "Models" 命名空间下
namespace E1.Backend.Api.Models
{
    public class RecyclingSite
    {
        [Key] // 告诉数据库这是主键 (Primary Key)
        public int Id { get; set; }

        [Required] // 设为必填项
        public string Name { get; set; } = string.Empty; // 回收站名称

        [Required]
        public string Type { get; set; } = string.Empty; // 回收类型 (例如: "Electronics", "Battery", "General")

        public string? Address { get; set; } // 详细地址 (可选)

        [Required]
        public double Latitude { get; set; } // 纬度 (用于地图)

        [Required]
        public double Longitude { get; set; } // 经度 (用于地图)
    }
}