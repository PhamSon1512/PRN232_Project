using Hangfire;
using MediAppointment.Application.DTOs.GeminiDTOs;
using MediAppointment.Application.Interfaces;
using MediAppointment.Infrastructure.Data;
using MediAppointment.Infrastructure.Identity;
using MediAppointment.Infrastructure.Persistence;
using MediAppointment.Infrastructure.Persistence.Configurations;
using MediAppointment.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MediAppointment.Infrastructure.Extensions
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Đăng ký các service liên quan đến database (Persistence)
            services.AddPersistenceServices(configuration);


            //Đăng ký Identity cho UserIdentity và IdentityRole<Guid>
            services.AddIdentity<UserIdentity, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
                };
            });

            // Đăng ký các service hạ tầng khác
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IVnPayService, VnPayService>();
            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<ITokenService, TokenService>();
            services.Configure<EmailConfig>(configuration.GetSection("EmailConfig"));
            // InfrastructureServiceCollectionExtensions.cs
            services.AddScoped<IEmailService, EmailService>();
            services.Configure<GeminiSettings>(configuration.GetSection("GeminiSettings"));
            services.AddHttpClient<IGeminiChatService, GeminiChatService>();
            services.AddSingleton<IEmailSender<UserIdentity>, EmailSender>();

            services.AddScoped<IJobService, JobService>();
            services.AddScoped<IManagerService, ManagerService>();
            services.AddScoped<IAppointmentService, AppointmentService>();
            services.AddScoped<IDoctorScheduleService, DoctorScheduleService>();
            services.AddScoped<IRoomTimeSlotService, RoomTimeSlotService>();
            services.AddScoped<IMedicalRecordService, MedicalRecordService>();
            services.AddScoped<IPatientService, PatientService>();
            services.AddScoped<IAppointmentBookingDoctor, BookingDoctorService>();
            services.AddScoped<IAdminService, Application.Services.AdminService>();
            services.AddHangfire(config =>
                config.UseSqlServerStorage(configuration.GetConnectionString("HangfireConnection")));
            services.AddHangfireServer();
            

            return services;
        }
    }
}
