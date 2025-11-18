using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E1.Backend.Api.Models
{
    // 这个模型用于存储 Firebase/Google (FCM) 
    // 返回给 APP (前端) 的唯一设备令牌
    public class UserDevice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string DeviceToken { get; set; } // 这是来自 Google 的长令牌

        [Required]
        public string UserId { get; set; } // (E1) 链接到 IdentityUser

        [ForeignKey("UserId")]
        public virtual IdentityUser User { get; set; }

        public DateTime RegisteredAt { get; set; }
    }
}