using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediAppointment.Domain.Entities.Abstractions;

namespace MediAppointment.Domain.Entities
{
    public class WalletTransaction : Entity
    {
        public Guid WalletId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } = default!; // "Deposit", "Payment"
        public string? Description { get; set; }
    }
}
