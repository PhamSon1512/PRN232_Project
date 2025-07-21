using ExceptionHandleNH;
using Hangfire;
using MediAppointment.API.Extensions;
using MediAppointment.Application.Extensions;
using MediAppointment.Application.Interfaces;
using MediAppointment.Infrastructure.Data;
using MediAppointment.Infrastructure.Identity;
using MediAppointment.Infrastructure.Persistence;
using MediAppointment.Infrastructure.Persistence.Seeder; // Thêm dòng này
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApiServices(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionNH>(); 
builder.Services.AddProblemDetails();
var app = builder.Build();
app.UseExceptionHandler();
// Tự động migrate database và seed dữ liệu trong cùng một scope
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
    await DbSeeder.SeedAsync(scope.ServiceProvider);
    var jobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    jobManager.AddOrUpdate<IJobService>(
        "JobCreateTimeSlot",
        job => job.JobCreateRoomTimeSlot(),
        "* * * * *"
    );
}



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS before Authorization
app.UseCors("AllowClient");

// Add Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
