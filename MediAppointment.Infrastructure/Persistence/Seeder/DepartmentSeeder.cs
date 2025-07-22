using MediAppointment.Domain.Entities;
using MediAppointment.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Infrastructure.Persistence.Seeder
{
    public static class DepartmentSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

            if (!dbContext.Departments.Any())
            {
                var departments = new List<Department>
    {
                    new Department
                    {
                        Id = Guid.NewGuid(),    
                        DepartmentName = "Nội tổng quát"
                    },
                    new Department
                    {
                        Id = Guid.NewGuid(),
                        DepartmentName = "Ngoại tổng quát"
                    },
                    new Department
                    {
                        Id = Guid.NewGuid(),
                        DepartmentName = "Nhi"
                    },
                    new Department
                    {
                        Id = Guid.NewGuid(),
                        DepartmentName = "Sản - Phụ khoa"
                    },
                    new Department
                    {
                        Id = Guid.NewGuid(),
                        DepartmentName = "Tai - Mũi - Họng"
                    },
                    new Department
                    {
                        Id = Guid.NewGuid(),
                        DepartmentName = "Mắt"
                    },
                    new Department
                    {
                        Id = Guid.NewGuid(),
                        DepartmentName = "Răng - Hàm - Mặt"
                    },
                    new Department
                    {
                        Id = Guid.NewGuid(),
                        DepartmentName = "Da liễu"
                    },
                    new Department
                    {
                        Id = Guid.NewGuid(),
                        DepartmentName = "Xét nghiệm"
                    },
                    new Department
                    {
                        Id = Guid.NewGuid(),
                        DepartmentName = "Chẩn đoán hình ảnh"
                    },
                    new Department
                    {
                        Id = Guid.NewGuid(),
                        DepartmentName = "Y học cổ truyền"
                    },
                    new Department
                    {
                        Id = Guid.NewGuid(),
                        DepartmentName = "Dinh dưỡng"
                    },
                    new Department
                    {
                        Id = Guid.NewGuid(),
                        DepartmentName = "Vật lý trị liệu - Phục hồi chức năng"
                    },
                    new Department
                    {
                        Id = Guid.NewGuid(),
                        DepartmentName = "Tâm lý"
                    },
                    new Department
                    {
                        Id = Guid.NewGuid(),
                        DepartmentName = "Nam học"
                    }
                };

                dbContext.Departments.AddRange(departments);
                await dbContext.SaveChangesAsync();
            }

        }
    }
}
