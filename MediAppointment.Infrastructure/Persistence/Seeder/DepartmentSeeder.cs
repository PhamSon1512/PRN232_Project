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
                        Id = Guid.Parse("0A438320-EEFE-4DB7-9DCA-51A17AB9AEE5"),
                        DepartmentName = "Nội tổng quát"
                    },
                    new Department
                    {
                        Id = Guid.Parse("4D484610-3623-465D-82C2-55C0337A5A11"),
                        DepartmentName = "Ngoại tổng quát"
                    },
                    new Department
                    {
                        Id = Guid.Parse("E053F9BF-3195-4A42-904A-E99A615BE701"),
                        DepartmentName = "Nhi"
                    },
                    new Department
                    {
                        Id = Guid.Parse("9D32F2F7-BDB9-4B72-BA46-973C58A3C7AD"),
                        DepartmentName = "Sản - Phụ khoa"
                    },
                    new Department
                    {
                        Id = Guid.Parse("C0F3E347-7E84-4A1F-9D75-B36FC25E5B62"),
                        DepartmentName = "Tai - Mũi - Họng"
                    },
                    new Department
                    {
                        Id = Guid.Parse("F5E3D3B9-B215-4D96-9397-DC707E2028DA"),
                        DepartmentName = "Mắt"
                    },
                    new Department
                    {
                        Id = Guid.Parse("A19E28D7-C028-47A3-87B2-37C6E84BC9B6"),
                        DepartmentName = "Răng - Hàm - Mặt"
                    },
                    new Department
                    {
                        Id = Guid.Parse("2B8BB890-4AD4-4FA8-B12B-0D3C312D3871"),
                        DepartmentName = "Da liễu"
                    },
                    new Department
                    {
                        Id = Guid.Parse("A8E3C452-677B-4FB3-9D8C-0A6F10F9E3F6"),
                        DepartmentName = "Xét nghiệm"
                    },
                    new Department
                    {
                        Id = Guid.Parse("C6E10936-36A4-4E76-A59F-0DE79C30C5F0"),
                        DepartmentName = "Chẩn đoán hình ảnh"
                    },
                    new Department
                    {
                        Id = Guid.Parse("9B4907D2-08A9-4F65-867C-5EAB0F13D336"),
                        DepartmentName = "Y học cổ truyền"
                    },
                    new Department
                    {
                        Id = Guid.Parse("BD2E270D-6772-4A58-AF5E-CE9D6AF2C517"),
                        DepartmentName = "Dinh dưỡng"
                    },
                    new Department
                    {
                        Id = Guid.Parse("80A2EB1C-41F0-4C5C-94C7-157AE4D01C4A"),
                        DepartmentName = "Vật lý trị liệu - Phục hồi chức năng"
                    },
                    new Department
                    {
                        Id = Guid.Parse("6FA89E39-1576-4E9A-B1B4-010C2676A72A"),
                        DepartmentName = "Tâm lý"
                    },
                    new Department
                    {
                        Id = Guid.Parse("E38A5D1F-62AA-4A11-8C37-CAEC08E9D946"),
                        DepartmentName = "Nam học"
                    }
                };

                dbContext.Departments.AddRange(departments);
                await dbContext.SaveChangesAsync();
            }

        }
    }
}
