using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Application.DTOs.Payments
{
    public class DepositRequest
    {
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public string? TransactionId { get; set; }
    }
}
