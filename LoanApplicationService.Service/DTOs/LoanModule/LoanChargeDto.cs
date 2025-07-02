namespace LoanApplicationService.Service.DTOs.LoanModule
{
    public class LoanChargeDto
    {
        public int ProcessingFee { get; set; }

        public decimal PrepaymentPenalty { get; set; }

        public decimal LatePaymentPenalty { get; set; }

        public int LoanProductId { get; set; }
    }
}
