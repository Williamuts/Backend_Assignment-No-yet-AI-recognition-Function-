using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations; // 1. 导入 (用于 [Required])

// 确保将 "E1.Backend.Api" 替换为您的后端项目的实际命名空间
namespace E1.Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        // ... (依赖注入和构造函数保持不变) ...
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _configuration;
        public AuthController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }


        // 接口 1: 注册
        // POST /api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model) // (使用 RegisterModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // --- 2. 关键修改: 使用 model.Username ---
            var user = new IdentityUser
            {
                UserName = model.Username, // (使用 Username)
                Email = model.Email        // (Email 仍然是 Email)
            };
            // ------------------------------------

            // _userManager 会自动处理密码哈希和存储
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                // (如果注册失败，例如邮箱或用户名已存在)
                return BadRequest(result.Errors);
            }

            return Ok(new { Message = "User registered successfully" });
        }

        // 接口 2: 登录 (保持不变!)
        // POST /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model) // (使用 LoginModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 1. 登录时我们仍然通过 Email 查找用户 (和以前一样)
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid email or password" });
            }

            // 2. 验证密码 (和以前一样)
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                return Unauthorized(new { Message = "Invalid email or password" });
            }

            // 3. 登录成功, 生成JWT Token (和以前一样)
            string token = GenerateJwtToken(user);
            return Ok(new { Token = token });
        }


        // ... (GenerateJwtToken 辅助方法保持不变) ...
        private string GenerateJwtToken(IdentityUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName), // (我们可以把 Username 也放进 Token)
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    // --- 3. 关键修改: 更新 RegisterModel ---
    public class RegisterModel
    {
        [Required]
        public string Username { get; set; } // (添加这一行)

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
    // ------------------------------------

    // --- LoginModel (保持不变!) ---
    public class LoginModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}