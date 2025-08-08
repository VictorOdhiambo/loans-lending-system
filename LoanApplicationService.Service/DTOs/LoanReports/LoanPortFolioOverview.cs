using LoanApplicationService.CrossCutting.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanApplicationService.Service.DTOs.LoanPortFolioOverview
{
    public class LoanPortFolioOverview
    {

        public decimal TotalOutstanding { get; set; }
        public decimal PrincipalOutstanding { get; set; }
        public decimal InterestOutstanding { get; set; }
        public decimal PenaltyOutstanding { get; set; }
        public decimal FeesOutstanding { get; set; }

        public int OpenLoans { get; set; }
        public int ProcessingLoans { get; set; }
        public int DefaultedLoans { get; set; }
        public int DeniedLoans { get; set; }
        public int FullyPaidLoans { get; set; }
        public int NotTakenUpLoans { get; set; }

        public int NumberOfCustomers { get; set; }



        public decimal Disbursed { get; set; }
        public decimal Paid { get; set; }

        public IEnumerable<MonthlyAmount> MonthlyDisbursements { get; set; } = new List<MonthlyAmount>();
    public IEnumerable<MonthlyAmount> MonthlyPayments { get; set; } = new List<MonthlyAmount>();

    public class MonthlyAmount
    {
        public string Month { get; set; } // e.g., "2025-08"
        public decimal Amount { get; set; }
    }

        public IEnumerable<RiskDistribution> CustomerRiskDistribution { get; set; } = new List<RiskDistribution>();         

        public class RiskDistribution
        {
            public int RiskLevel { get; set; }
            public int Count { get; set; }

            public decimal Percentage { get; set; }

        }
    }
}