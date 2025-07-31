
using System.ComponentModel.DataAnnotations;
using LoanApplicationService.CrossCutting.Utils;

namespace LoanApplicationService.Service.DTOs.LoanModule;
public class LoanProductDto
{
    public int ProductId { get; set; }

    public required string ProductName { get; set; }

    public int LoanProductType { get; set; }
    public string? LoanProductTypeDescription { get; set; }

    public int PaymentFrequency { get; set; }
    public string? PaymentFrequencyDescription { get; set; }

    public decimal MinAmount { get; set; }

    public decimal MaxAmount { get; set; }

    public decimal InterestRate { get; set; }

    public int MinTermMonths { get; set; }

    public int MaxTermMonths { get; set; }

    public decimal ProcessingFee { get; set; }

    public bool IsActive { get; set; }

    
    [MaxLength(500)]
    public string? EligibilityCriteria { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }

    public LoanRiskLevel RiskLevel { get; set; }
    public string? RiskLevelDescription { get; set; }
}


