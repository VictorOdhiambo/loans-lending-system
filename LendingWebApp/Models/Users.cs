using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Loan_application_service.Models
{
    public class Users
    {
        [Key]
        public int? Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }

        public string? Role { get; set; }
        public Boolean IsActive { get; set; } 
        public string? Password { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime createdAt { get; set; }
    }
}
