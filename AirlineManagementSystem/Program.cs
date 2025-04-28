using System.Text;
using AirlineManagementSystem.Data;
using AirlineManagementSystem.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NLog.Web;
using System.Reflection;
using AirlineManagementSystem.Profiles;
using AirlineManagementSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Logging.ClearProviders();
builder.Host.UseNLog();

// MVC & Razor
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// EF Core + Connection String
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<AppDbContext1>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection1")));

builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);  // Specify the assembly that contains MappingProfile


// Dependency Injection
builder.Services.AddScoped<IFlightService, FlightService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBookingService, BookingService>();
// Add this after your HttpClient registration
builder.Services.AddScoped<UserApiService>();

// Session Support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Cookie Policy (for safety)
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
    options.Secure = CookieSecurePolicy.Always;
});

// Antiforgery settings for secure cookie
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// In Program.cs or Startup.cs of your MVC project
builder.Services.AddHttpClient("AirlineAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7099/"); // Replace PORT with your API port
});

// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"];

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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

// Authorization Roles
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ManagerOnly", policy => policy.RequireRole("Manager"));
    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
});

var app = builder.Build();

// Error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // Enforce HTTPS
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCookiePolicy();      // Ensure secure cookie behavior
app.UseSession();           // Enable session tracking

// Inject JWT token from cookie into Authorization header
app.Use(async (context, next) =>
{
    var jwt = context.Request.Cookies["jwt"];
    if (!string.IsNullOrEmpty(jwt))
    {
        context.Request.Headers.Authorization = $"Bearer {jwt}";
    }
    await next();
});

app.UseAuthentication();    // Apply JWT auth
app.UseAuthorization();     // Role-based authorization

// Default routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
