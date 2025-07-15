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

        public LoanProduct LoanProduct { get; set; }



        public int LoanChargeId { get; set; }

        public  LoanCharge LoanCharge { get; set; }
    }
}
