using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanApplicationService.Core.Models
{
    public class LoanPayment
    {
        public int Id { get; set; }

        public int AccountId { get; set; }

        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }

        public int PaymentMethod { get; set; }

        public bool IsPrepaymentPenaltyApplied { get; set; } = false;


        public virtual Account Account { get; set; }
    }
}
