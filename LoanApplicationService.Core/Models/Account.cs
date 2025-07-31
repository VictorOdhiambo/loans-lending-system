using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanApplicationService.Core.Models;

public class Account
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int AccountId { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [Required]
    public int ApplicationId { get; set; }

    [Required]
    [MaxLength(20)]
    public string? AccountNumber { get; set; }

    [Required]
    [MaxLength(20)]
    public string? AccountType { get; set; }

    [Column(TypeName = "decimal(15,2)")]
    public decimal PrincipalAmount { get; set; }

    [Column(TypeName = "decimal(15,2)")]
    public decimal OutstandingBalance { get; set; }

    [Column(TypeName = "decimal(5,4)")]
    public decimal InterestRate { get; set; }

    public int TermMonths { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal MonthlyPayment { get; set; }

    public DateTime NextPaymentDate { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Active";

    public DateTime? DisbursementDate { get; set; }
    public DateTime? MaturityDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("CustomerId")]
    public required virtual Customer Customer { get; set; }

    [ForeignKey("ApplicationId")]
    public required virtual LoanApplication LoanApplication { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public virtual ICollection<AuditTrail> AuditTrails { get; set; } = new List<AuditTrail>();
}