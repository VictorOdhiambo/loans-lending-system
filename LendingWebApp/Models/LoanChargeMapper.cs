using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Loan_application_service.Models
{
    public class LoanChargeMapper


    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        int id {  get; set; }
        public int LoanProductId { get; set; }

        public required LoanProduct LoanProduct { get; set; } 

       

        public int LoanChargeId { get; set; }

        public required LoanCharge LoanCharge { get; set; }      
    }
}
