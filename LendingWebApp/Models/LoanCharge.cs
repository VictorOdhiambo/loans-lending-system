using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Loan_application_service.Models
{
    public class LoanCharge
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        public decimal Amount { get; set; } 
       
        public Boolean IsUpfront { get; set; }  

        public string? description { get; set; } 

        public Boolean IsPenalty {  get; set; } 

        public int LoanProductId { get; set; }

        public required ICollection<LoanChargeMapper> LoanChargeMap { get; set; }

    }
}

