using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanApplicationService.Service.DTOs.LoanPayment
{
    public class LoanPaymentDto
    {
        public int Id { get; set; }
        public int AccountId { get; set; }

        public decimal Amount { get; set; }
        public DateTimeOffset PaymentDate { get; set; }
        public int PaymentMethod { get; set; } 
        
    }
}
