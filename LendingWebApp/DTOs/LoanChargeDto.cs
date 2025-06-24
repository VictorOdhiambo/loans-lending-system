using Loan_application_service.Models;
using Loan_application_service.DTOs;

namespace Loan_application_service.DTOs
{
    public class LoanChargeDto
    {
        

        public int ProcessingFee { get; set; }

        public decimal PrepaymentPenalty { get; set; }

        public decimal LatePaymentPenalty { get; set; }

        public int LoanProductId{ get; set; }
    }
}
