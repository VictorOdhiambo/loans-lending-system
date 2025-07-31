using System.ComponentModel.DataAnnotations;

namespace LoanApplicationService.Service.DTOs.LoanModule
{
    public class LoanChargeDto
    {
        public int LoanChargeId { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public decimal Amount { get; set; }
        public bool IsPenalty { get; set; }
        public bool IsUpfront { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}
