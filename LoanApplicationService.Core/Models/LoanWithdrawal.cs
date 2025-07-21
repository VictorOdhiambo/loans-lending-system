using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanApplicationService.Core.Models
{
    public class LoanWithdrawal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int WithdrawalId { get; set; }
        public int AccountId { get; set; }
        public decimal Amount { get; set; }
        public DateTime WithdrawalDate { get; set; }
        public string Status { get; set; } = "Pending"; 
        public int PaymentMethodId { get; set; } 
        public virtual Account Account { get; set; }
    }
}
