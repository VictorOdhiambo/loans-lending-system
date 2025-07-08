using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanApplicationService.Core.Models
{
    public class LoanChargeMapper
    {
        public int LoanProductId { get; set; }

        public required LoanProduct LoanProduct { get; set; }



        public int LoanChargeId { get; set; }

        public required LoanCharge LoanCharge { get; set; }
    }
}
