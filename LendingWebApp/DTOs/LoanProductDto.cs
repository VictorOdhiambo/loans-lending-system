
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Loan_application_service.DTOs;
public class loanproductDto

{
   
    public int ProductId { get; set; }

    [Required]
    [MaxLength(100)]
    public required string ProductName { get; set; }

    [Required]
    [MaxLength(50)]
    public required string ProductType { get; set; }

    [Column(TypeName = "decimal(15,2)")]
    public decimal MinAmount { get; set; }

    [Column(TypeName = "decimal(15,2)")]
    public decimal MaxAmount { get; set; }

    [Column(TypeName = "decimal(5,4)")]
    public decimal InterestRate { get; set; }

    public int MinTermMonths { get; set; }
    public int MaxTermMonths { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal ProcessingFee { get; set; }

    [MaxLength(500)]
    public required string EligibilityCriteria { get; set; }


    

}

