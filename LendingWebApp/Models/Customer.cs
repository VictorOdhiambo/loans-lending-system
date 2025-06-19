namespace Loan_application_service.Models;

public class Customer
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int CustomerId { get; set; }
        
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; }
        
    [Required]
    [MaxLength(50)]
    public string LastName { get; set; }
        
    [Required]
    [MaxLength(100)]
    [EmailAddress]
    public string Email { get; set; }
        
    [MaxLength(20)]
    public string Phone { get; set; }
        
    [MaxLength(200)]
    public string Address { get; set; }
        
    public DateTime DateOfBirth { get; set; }
        
    [MaxLength(20)]
    public string NationalId { get; set; }
        
    [Column(TypeName = "decimal(5,2)")]
    public decimal? CreditScore { get; set; }
        
    [MaxLength(50)]
    public string EmploymentStatus { get; set; }
        
    [Column(TypeName = "decimal(15,2)")]
    public decimal? AnnualIncome { get; set; }
        
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
    // Navigation Properties
    public virtual ICollection<LoanApplication> LoanApplications { get; set; } = new List<LoanApplication>();
    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public virtual ICollection<AuditTrail> AuditTrails { get; set; } = new List<AuditTrail>();
}