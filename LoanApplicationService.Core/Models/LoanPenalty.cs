using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanApplicationService.Core.Models
{
    public class LoanPenalty
    {
        public int Id { get; set; }   
        
        public int AccountId { get; set; }

        public decimal Amount { get; set; }

        public bool IsPaid { get; set; } = false;


        public DateTimeOffset AppliedDate { get; set; }

        public virtual Account Account { get; set; }

    }
}
