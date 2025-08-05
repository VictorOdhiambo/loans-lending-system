using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace LoanApplicationService.Core.Models
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

        public Guid? ApprovedBy { get; set; }

        public Guid? RejectedBy { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal RequestedAmount { get; set; }

        public Guid createdBy { get; set; }
        public int TermMonths { get; set; }

        public int PaymentFrequency { get; set; }

        [MaxLength(200)]
        public string? Purpose { get; set; }

        public int Status { get; set; } 

        [Column(TypeName = "decimal(15,2)")]
        public decimal? ApprovedAmount { get; set; }

        [Column(TypeName = "decimal(5,4)")]
        public decimal InterestRate { get; set; }

        public DateTimeOffset ApplicationDate { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset DecisionDate { get; set; }

        [MaxLength(500)]
        public string? DecisionNotes { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation Properties
        [ForeignKey("CustomerId")]
        public required virtual Customer Customer { get; set; }

        [ForeignKey("ProductId")]
        public required virtual LoanProduct LoanProduct { get; set; }

        [ForeignKey("ProcessedBy")]
        public virtual ApplicationUser? ProcessedByUser { get; set; }

        public virtual Account? Account { get; set; }

        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<AuditTrail> AuditTrails { get; set; } = new List<AuditTrail>();
        
    }
}
