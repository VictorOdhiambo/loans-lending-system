using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Loan_application_service.Models
{
    public class LoanApplication
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ApplicationId { get; set; }
        
        [Required]
        public int CustomerId { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        public int? ProcessedBy { get; set; }
        
        [Column(TypeName = "decimal(15,2)")]
        public decimal RequestedAmount { get; set; }
        
        public int TermMonths { get; set; }
        
        [MaxLength(200)]
        public string Purpose { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";
        
        [Column(TypeName = "decimal(15,2)")]
        public decimal? ApprovedAmount { get; set; }
        
        [Column(TypeName = "decimal(5,4)")]
        public decimal? InterestRate { get; set; }
        
        public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;
        
        public DateTime? DecisionDate { get; set; }
        
        [MaxLength(500)]
        public string DecisionNotes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation Properties
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }
        
        [ForeignKey("ProductId")]
        public virtual LoanProduct LoanProduct { get; set; }
        
        [ForeignKey("ProcessedBy")]
        public virtual User ProcessedByUser { get; set; }
        
        public virtual Account Account { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<AuditTrail> AuditTrails { get; set; } = new List<AuditTrail>();
    }
}
