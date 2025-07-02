using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanApplicationService.Core.Models;

public class Notification
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int NotificationId { get; set; }

    public int? CustomerId { get; set; }

    public int? UserId { get; set; }

    public int? ApplicationId { get; set; }

    public int? AccountId { get; set; }

    [Required]
    [MaxLength(50)]
    public int NotificationType { get; set; }

    [Required]
    [MaxLength(200)]
    public string? Title { get; set; }

    [Required]
    [MaxLength(1000)]
    public string? Message { get; set; }

    [Required]
    [MaxLength(20)]
    public string Channel { get; set; } = "Email";

    public bool IsRead { get; set; } = false;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReadAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("CustomerId")]
    public required virtual Customer Customer { get; set; }

    [ForeignKey("UserId")]
    public required virtual Users User { get; set; }

    [ForeignKey("ApplicationId")]
    public required virtual LoanApplication LoanApplication { get; set; }

    [ForeignKey("AccountId")]
    public required virtual Account Account { get; set; }
}