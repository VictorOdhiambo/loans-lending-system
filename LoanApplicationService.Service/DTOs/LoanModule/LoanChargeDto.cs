namespace LoanApplicationService.Service.DTOs.LoanModule
{
    public class LoanChargeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsPenalty { get; set; }
        public bool IsUpfront { get; set; }
        public decimal Amount { get; set; }
    }
}
