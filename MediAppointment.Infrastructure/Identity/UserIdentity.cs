using MediAppointment.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace MediAppointment.Infrastructure.Identity
{
    public class UserIdentity : IdentityUser<Guid> 
    {
    }

}
