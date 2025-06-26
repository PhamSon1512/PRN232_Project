using MediAppointment.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace MediAppointment.Infrastructure.Identity
{
    public class UserIdentity : IdentityUser<Guid> 
    {
        public string? FullName  { get; set; }
        public string RefreshToken { get; internal set; }
        public DateTime RefreshTokenExpiryTime { get; internal set; }
    }

}
