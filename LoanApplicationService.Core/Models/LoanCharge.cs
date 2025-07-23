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
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsPenalty { get; set; }
        public bool IsUpfront { get; set; }
        public decimal Amount { get; set; }

        public bool IsDeleted { get; set; } = false;

        public bool IsPercentage { get; set; }


        public required ICollection<LoanChargeMapper> LoanChargeMap { get; set; }

    }
}

