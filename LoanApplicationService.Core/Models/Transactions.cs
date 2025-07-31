using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanApplicationService.Core.Models
{
    public class Transactions
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionId { get; set; } 

        public int AccountId { get; set; }

        public decimal Amount { get; set; }

        public DateTimeOffset TransactionDate { get; set; }
        public int PaymentMethod { get; set; }
        public int TransactionType { get; set; } 

        public virtual Account Account { get; set; } 

        public decimal PrincipalAmount { get; set; } 

        public decimal InterestAmount { get; set; }

        public decimal PenaltyAmount { get; set; }


    }
}
