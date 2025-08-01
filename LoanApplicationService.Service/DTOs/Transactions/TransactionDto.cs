﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanApplicationService.Service.DTOs.Transactions
{
    public class TransactionDto
    {
        public int TransactionId { get; set; }

        public int AccountId { get; set; }

        public decimal Amount { get; set; }

        public DateTime TransactionDate { get; set; }
        public int TransactionType { get; set; }

        public int PaymentMethod { get; set; }
        public decimal PrincipalAmount { get; set; }

        public decimal InterestAmount { get; set; }
    }
}
