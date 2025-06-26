using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Loan_application_service.Data
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal InterestRate { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal MaxAmount { get; set; }

        [MaxLength(50)]
        public string Duration { get; set; }  // e.g. "12 months"
    }
}
