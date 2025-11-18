using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using E1.Backend.Api.Models;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations; // <-- 5. 关键: 添加这一行来修复 CS0246 错误

namespace E1.Backend.Api.Controllers
{
    [Authorize] // 1. 保护此控制器，必须登录 (E1)
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // 2. 注入数据库服务
        public NotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- E6 核心接口: 注册设备令牌 ---
        // POST /api/notifications/register-device
        [HttpPost("register-device")]
        public async Task<IActionResult> RegisterDevice([FromBody] DeviceRegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 3. 获取当前登录用户的 ID (来自 E1 的 Token)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            // 4. 检查这个 Token 是否已经为这个用户注册过了？
            bool deviceExists = await _context.UserDevices
                .AnyAsync(d => d.UserId == userId && d.DeviceToken == model.DeviceToken);

            if (!deviceExists)
            {
                // 5. 如果不存在, 就创建并保存
                var newUserDevice = new UserDevice
                {
                    UserId = userId,
                    DeviceToken = model.DeviceToken,
                    RegisteredAt = DateTime.UtcNow
                };

                await _context.UserDevices.AddAsync(newUserDevice);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Device registered successfully" });
            }

            return Ok(new { Message = "Device was already registered" });
        }
    }

    // --- 用于接收前端数据的模型 (DTO) ---
    public class DeviceRegisterModel
    {
        [Required] // (这一行现在可以被正确识别了)
        public string DeviceToken { get; set; }
    }
}