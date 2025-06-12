using MediAppointment.API.Extensions;
using MediAppointment.Application.Extensions;
using MediAppointment.Infrastructure.Data;
using MediAppointment.Infrastructure.Persistence;
using MediAppointment.Infrastructure.Persistence.Seeder; // Thêm dòng này
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApiServices(builder.Configuration);

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Tự động migrate database và seed dữ liệu trong cùng một scope
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();

    await DbSeeder.SeedAsync(scope.ServiceProvider);
}



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
