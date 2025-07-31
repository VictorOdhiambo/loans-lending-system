using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanApplicationService.Core.Models
{
    public class LoanRepaymentSchedule
    {
        [Key]
        public int ScheduleId { get; set; }
        public int AccountId { get; set; }
        public int InstallmentNumber { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestAmount { get; set; }  
        public decimal ScheduledAmount { get; set; } 

        public decimal PaidPrincipal { get; set; } 
        public decimal PaidInterest { get; set; }  

        public DateTime DueDate { get; set; } 
        public DateTime? PaidDate { get; set; } 
        public bool IsPaid { get; set; }

        // NEW PROPERTIES
        public DateTime StartDate { get; set; } 
        public DateTime EndDate { get; set; }   

        // Navigation property
        public virtual Account Account { get; set; }
    }
}


