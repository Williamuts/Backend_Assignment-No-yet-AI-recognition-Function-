// ---------------------------------
// 导入所有必需的库
// ---------------------------------
using E1.Backend.Api; // <-- 确保它可以找到您的DbContext
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------
// (1) 注册服务 (Services)
// ---------------------------------

// 添加API控制器
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// --- (这是 "Authorize" 按钮的完整配置) ---
builder.Services.AddSwaggerGen(options =>
{
    // 1. 为 Swagger UI 添加 "Authorize" 按钮的定义
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http, // 使用 HTTP 承载
        Scheme = "Bearer", // 方案 (Scheme)
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "请输入 'Bearer' [空格] 然后输入您的 Token. \r\n\r\n 例如: 'Bearer 12345abcdef'"
    });

    // 2. 告诉 Swagger 我们的 API 操作需要这个安全方案
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
// --- (Swagger "Authorize" 按钮配置结束) ---


// --- (E1 & E3 数据库配置) ---
// 1. 添加数据库连接
var connectionString = "Data Source=app.db"; // SQLite 数据库文件
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// 2. 添加 .NET Identity 服务 (E1)
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
// --- (E1 & E3 数据库配置结束) ---


// --- (E1: 配置JWT认证) ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        // 从 appsettings.json 中读取配置
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});
// --- (E1: JWT配置结束) ---


var app = builder.Build();

// ---------------------------------
// (2) 配置HTTP请求管道 (Middleware)
// ---------------------------------

// 在开发环境中启用Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 关键: 启用认证和授权
// 必须在 MapControllers 之前
app.UseAuthentication(); // <-- 启用认证 (检查Token)
app.UseAuthorization(); // <-- 启用授权 (检查 [Authorize] 标签)

app.MapControllers(); // 将请求映射到 AuthController, RecyclingController 等

app.Run();