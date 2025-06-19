using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Loan_application_service.Models
{
    public class LoanProduct
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ProductId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string ProductName { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string ProductType { get; set; }
        
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
        public string EligibilityCriteria { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation Properties
        public virtual ICollection<LoanApplication> LoanApplications { get; set; } = new List<LoanApplication>();
    }
}
