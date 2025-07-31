using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanApplicationService.Service.DTOs.Account
{
    public class AccountDto
    {
    public int AccountId { get; set; }

    public int CustomerId { get; set; }

    public int ApplicationId { get; set; }

    public string AccountNumber { get; set; } = string.Empty;

    public string AccountType { get; set; } = string.Empty;

    public decimal PrincipalAmount { get; set; }

    public decimal OutstandingBalance { get; set; }

    public decimal DisbursedAmount { get; set; }

        public decimal AvailableBalance { get; set; }

        public int WithdrawalAmount { get; set; }

    public decimal InterestRate { get; set; }
    public int TermMonths { get; set; }

    public decimal MonthlyPayment { get; set; }

    public DateTime NextPaymentDate { get; set; }

    public int PaymentFrequency { get; set; }

        public int Status { get; set; } 

    public DateTime? DisbursementDate { get; set; }

    public DateTime? MaturityDate { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }


    }
}
