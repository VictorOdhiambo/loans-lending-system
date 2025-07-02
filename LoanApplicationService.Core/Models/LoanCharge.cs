using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanApplicationService.Core.Models
{
    public class LoanCharge
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }
        public int ProcessingFee { get; set; }
        public decimal PrepaymentPenalty { get; set; }
        public decimal LatePaymentPenalty { get; set; }
        public int LoanProductId { get; set; }
        public required virtual LoanProduct LoanProduct { get; set; }
    }
}

