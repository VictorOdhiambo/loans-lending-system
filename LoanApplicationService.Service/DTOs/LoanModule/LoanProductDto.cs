
using System.ComponentModel.DataAnnotations;
using LoanApplicationService.CrossCutting.Utils;

namespace LoanApplicationService.Service.DTOs.LoanModule;
public class LoanProductDto
{
    public int ProductId { get; set; }

    [Required]
    [MaxLength(100)]
    [Display(Name = "Product Name")]
    public string ProductName { get; set; }

    [Required(ErrorMessage = "Please select a loan product type.")]
    public int? LoanProductType { get; set; }  

    public string LoanProductTypeDescription =>
        LoanProductType.HasValue && Enum.IsDefined(typeof(LoanProductType), LoanProductType.Value)
        ? EnumHelper.GetDescription((LoanProductType)LoanProductType.Value)
        : string.Empty;

    [Required(ErrorMessage = "Please select a payment frequency.")]
    public int? PaymentFrequency { get; set; }  

    public string PaymentFrequencyDescription =>
        PaymentFrequency.HasValue && Enum.IsDefined(typeof(PaymentFrequency), PaymentFrequency.Value)
        ? EnumHelper.GetDescription((PaymentFrequency)PaymentFrequency.Value)
        : string.Empty;

    [Required]
    public decimal MinAmount { get; set; }

    [Required]
    public decimal MaxAmount { get; set; }

    [Required]
    public decimal InterestRate { get; set; }

    [Required]
    public int MinTermMonths { get; set; }

    [Required]
    public int MaxTermMonths { get; set; }

    [Required]
    public decimal ProcessingFee { get; set; }

    public bool IsActive { get; set; } = true;

    [Required]
    [MaxLength(500)]
    public string EligibilityCriteria { get; set; }

    
}


