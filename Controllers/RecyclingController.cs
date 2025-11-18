using E1.Backend.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // 导入 EF Core

namespace E1.Backend.Api.Controllers
{
    [Authorize] // <-- 1. 保护此控制器，必须登录 (E1)
    [ApiController]
    [Route("api/[controller]")]
    public class RecyclingController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // 数据库上下文

        // 2. 注入数据库服务
        public RecyclingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- 接口 1: 添加一个新的回收站 (用于测试) ---
        // POST /api/recycling/add
        [HttpPost("add")]
        public async Task<IActionResult> AddSite([FromBody] RecyclingSite site)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _context.RecyclingSites.AddAsync(site);
            await _context.SaveChangesAsync();

            // 返回 "201 Created" 状态码和新创建的回收站
            return CreatedAtAction(nameof(GetSiteById), new { id = site.Id }, site);
        }

        // --- 接口 2: 获取所有回收站 (E3 核心) ---
        // GET /api/recycling/all
        [HttpGet("all")]
        public async Task<IActionResult> GetAllSites()
        {
            // 从数据库中获取所有回收站列表
            var sites = await _context.RecyclingSites.ToListAsync();

            // 返回 "200 OK" 和列表数据
            return Ok(sites);
        }

        // --- 接口 3: (可选) 按ID获取单个回收站 ---
        // GET /api/recycling/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSiteById(int id)
        {
            var site = await _context.RecyclingSites.FindAsync(id);
            if (site == null)
            {
                return NotFound();
            }
            return Ok(site);
        }

        // (未来: 我们可以添加一个 'find_nearby' 接口来接收GPS坐标并计算距离,
        // 但 'GetAllSites' 已足够前端地图显示。)
    }
}