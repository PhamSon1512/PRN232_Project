using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediAppointment.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MediAppointment.Infrastructure.Identity
{
    public class EmailSender : IEmailSender<UserIdentity>
    {
        private readonly IServiceProvider _serviceProvider;

        public EmailSender(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task SendConfirmationLinkAsync(UserIdentity user, string email, string confirmationLink)
            => SendScoped(email, "Xác nhận tài khoản", $"Vui lòng xác nhận: {confirmationLink}");

        public Task SendPasswordResetLinkAsync(UserIdentity user, string email, string resetLink)
            => SendScoped(email, "Đặt lại mật khẩu", $"Link đặt lại mật khẩu: {resetLink}");

        public Task SendPasswordResetCodeAsync(UserIdentity user, string email, string resetCode)
            => SendScoped(email, "Mã đặt lại mật khẩu", $"Mã: {resetCode}");

        private Task SendScoped(string to, string subject, string body)
        {
            using var scope = _serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            return emailService.SendAsync(to, subject, body);
        }
    }

}
