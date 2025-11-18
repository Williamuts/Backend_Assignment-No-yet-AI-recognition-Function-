using E1.Backend.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http; // 需要 IFormFile
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting; // 需要 IWebHostEnvironment
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace E1.Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 1. 确保只有登录的用户才能提交
    public class IncidentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment; // 用于获取 wwwroot 路径

        public IncidentController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // POST /api/incident/submit
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitIncident([FromForm] IncidentReportDto reportDto)
        {
            if (reportDto.Photo == null || reportDto.Photo.Length == 0)
            {
                return BadRequest("没有提供照片 (Photo is required)");
            }

            // --- 1. 保存照片到服务器 ---
            string webRootPath = _webHostEnvironment.WebRootPath;

            // ******** 这是新的“防崩溃”代码 ********
            if (string.IsNullOrEmpty(webRootPath))
            {
                // 如果 webRootPath 是 null, 手动将其设置为 wwwroot 文件夹
                // 这假设您的 wwwroot 文件夹在项目的根目录下
                webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }
            // ******** 修正结束 ********

            // (这是您之前崩溃的第42行，现在它 100% 不会崩溃了)
            string uploadFolder = Path.Combine(webRootPath, "uploads");

            // 确保 "uploads" 文件夹存在
            Directory.CreateDirectory(uploadFolder);

            // 创建一个唯一的文件名 (例如: 8a1c...e1.jpg)
            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(reportDto.Photo.FileName);
            string filePath = Path.Combine(uploadFolder, uniqueFileName);

            // 将文件流复制到服务器上的新文件中
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await reportDto.Photo.CopyToAsync(fileStream);
            }

            // --- 2. 保存报告详情到数据库 ---

            // 获取当前登录用户的ID (来自E1的Token)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized(new { Message = "User ID not found in token." });
            }

            // 创建数据库模型
            var incident = new IncidentReport
            {
                Description = reportDto.Description,
                Latitude = reportDto.Latitude,
                Longitude = reportDto.Longitude,
                Status = "Submitted", // 默认状态
                ReportedAt = DateTime.UtcNow,
                UserId = userId, // 关联用户
                // **重要**: 我们只在数据库中存储照片的URL路径
                PhotoUrl = "/uploads/" + uniqueFileName
            };

            await _context.IncidentReports.AddAsync(incident);
            await _context.SaveChangesAsync();

            // 返回 201 Created，并包含新创建的数据
            return CreatedAtAction(nameof(GetIncidentById), new { id = incident.Id }, incident);
        }

        // 辅助接口，用于 "CreatedAtAction"
        [HttpGet("{id}")]
        public async Task<IActionResult> GetIncidentById(int id)
        {
            var incident = await _context.IncidentReports.FindAsync(id);
            if (incident == null)
            {
                return NotFound();
            }
            return Ok(incident);
        }
    }

    // --- 数据传输对象 (DTO) ---
    // 用于接收来自表单(form)的数据
    public class IncidentReportDto
    {
        [Required]
        public string Description { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        [Required]
        public IFormFile Photo { get; set; } // 这是上传的文件
    }
}