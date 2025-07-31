using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanApplicationService.Service.DTOs.LoanDisbursement
{
    public class LoanWithdawalDto
    {
        public int AccountId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending";
        public int PaymentMethod { get; set; }
    }
}
