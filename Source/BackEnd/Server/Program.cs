using AutoMapper;
using FastReport.DataVisualization.Charting;
using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Portable.Licensing;
using ReportServer.Repositories;
using ReportServer.Services;
using reportSystem01.Server;
using reportSystem01.Server.Data;
using reportSystem01.Server.Helpers;
using reportSystem01.Shared;
using Sinco.Server.Middleware;
using Sinco.Server.Repositories;
using Sinco.Server.Repositories.AttachedFile;
using Sinco.Server.Repositories.Auth;
using Sinco.Server.Repositories.BaseRepository;
using Sinco.Server.Repositories.Custom;
using Sinco.Server.Repositories.Dynamic;
using Sinco.Server.Repositories.GetFormData;
using Sinco.Server.Repositories.Lookup;
using Sinco.Server.Repositories.Permission;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ✅ THỰC HIỆN KIỂM TRA LICENSE TRƯỚC KHI BUILD APP
try
{
    var encLicense = File.ReadAllText("license.enc");
    var aesPassword = "l@ihungph@nghan";
    var publicKey = File.ReadAllText("public.key");
    var machineId = MachineIdHelper.GetMachineId();

    var decrypted = AesEncryptionHelper.Decrypt(encLicense, aesPassword);
    var license = Portable.Licensing.License.Load(decrypted);

    if (!license.VerifySignature(publicKey))
        throw new Exception("❌ Sai chữ ký license");

    if (license.Expiration < DateTime.UtcNow)
        throw new Exception("❌ License đã hết hạn");

    if (license.ProductFeatures.Get("MachineId") != machineId)
        throw new Exception("❌ License không khớp với máy");

    Console.WriteLine("✅ License hợp lệ!");
}
catch (Exception ex)
{
    Console.WriteLine($"🚫 License invalid: {ex.Message}");
    return; // ⛔ Dừng app
}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Configure log4net
builder.Logging.ClearProviders(); // Xóa các provider log mặc định
builder.Host.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole(); // Thêm console logging
    // Bạn có thể thêm các provider khác nếu cần
});

// Load cấu hình log4net từ file
var logRepository = LogManager.GetRepository(System.Reflection.Assembly.GetEntryAssembly());
XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<PowerBIService>();

// Register log4net logger
builder.Services.AddSingleton<ILog>(LogManager.GetLogger(typeof(Program)));

builder.Services.AddScoped<IReportRepository>(provider => new ReportRepository(connectionString));

builder.Services.AddDbContext<ReportServerContext>(options =>
{
    options.UseSqlServer(connectionString)
    .LogTo(Console.WriteLine, LogLevel.Information);
});
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserGroupService, UserGroupService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<ILookupRepository, LookupRepository>();

builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IDataService, DataService>();


// Cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .WithExposedHeaders("Content-Disposition", "X-Debug-Filename", "X-Debug-Content-Type");
        });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value!)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<SystemOptionsRepository>();
builder.Services.AddScoped<IDatabaseConnectionRepository, DatabaseConnectionRepository>();
builder.Services.AddScoped<IMenuPermissionService, MenuPermissionService>();
builder.Services.AddScoped<IFormConfigRepository, FormConfigRepository>();
builder.Services.AddScoped<IDynamicRepository, DynamicRepository>();
builder.Services.AddScoped<IAttachedFileRepository, AttachedFileRepository>();
builder.Services.AddScoped<IUnitRepository, UnitRepository>();
builder.Services.AddScoped<CustomQueryRepository>();

builder.Services.AddAuthorization();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Cấu hình Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Sinco API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sinco API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.UseSessionValidation();
app.MapControllers();

app.Run();
