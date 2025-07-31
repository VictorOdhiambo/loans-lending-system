using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanApplicationService.Core.Models
{
    public class LoanChargeMapper
    {
        [Key]
        public int LoanChargeId { get; set; }

        [Key]
        public int LoanProductId { get; set; }

        // Navigation Properties
        public required LoanProduct LoanProduct { get; set; }

        public required LoanCharge LoanCharge { get; set; }
    }
}
