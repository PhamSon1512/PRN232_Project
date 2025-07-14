using MediAppointment.Client.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add shared CookieContainer for maintaining session across API calls
builder.Services.AddSingleton<System.Net.CookieContainer>();

// Add HTTP Client for API calls with shared cookie support
builder.Services.AddHttpClient("ApiClient", (serviceProvider, client) =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7230");
})
.ConfigurePrimaryHttpMessageHandler(serviceProvider => new HttpClientHandler
{
    UseCookies = true,
    CookieContainer = serviceProvider.GetRequiredService<System.Net.CookieContainer>()
});

// Add HttpContextAccessor for session management
builder.Services.AddHttpContextAccessor();

// Add Session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add Authentication and JWT support
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
        ValidIssuer = "MediAppointmentApi", // Khớp với server-side
        ValidAudience = "MediAppointmentUsers", // Khớp với server-side
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MediAppointmentSecretKey@@@@@@@@@@@")), // Khớp với server-side
        NameClaimType = ClaimTypes.NameIdentifier // Ánh xạ "nameid" thành NameIdentifier
    };
});

builder.Services.AddAuthorization();

// Register application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IDoctorAppointmentService, DoctorAppointmentService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<ITimeSlotService, TimeSlotService>();
builder.Services.AddScoped<IManagerService, ManagerService>();

// Add API base URL configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Add Session middleware
app.UseSession();

app.MapControllerRoute(
    name: "appointment_book",
    pattern: "Appointment/Book",
    defaults: new { controller = "Appointment", action = "Book" });

app.MapControllerRoute(
    name: "doctor_schedule",
    pattern: "DoctorSchedule/Manage",
    defaults: new { controller = "DoctorSchedule", action = "Manage" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();