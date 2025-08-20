using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanApplicationService.Core.Models;

public class AuditTrail
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int AuditId { get; set; }

    public Guid? UserId { get; set; }

    public int? CustomerId { get; set; }

    public int? ApplicationId { get; set; }

    public int? AccountId { get; set; }

    [Required]
    [MaxLength(50)]
    public string EntityType { get; set; }

    public int EntityId { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Action { get; set; }

    [MaxLength(2000)]
    public string OldValues { get; set; }

    [MaxLength(2000)]
    public string NewValues { get; set; }

    [MaxLength(45)]
    public string IpAddress { get; set; }

    [MaxLength(500)]
    public string UserAgent { get; set; }

    public DateTime CreatedAt { get; set; }
    public Customer Customer { get; set; }
    public LoanApplication LoanApplication { get; set; }
    public Account Account { get; set; }
}