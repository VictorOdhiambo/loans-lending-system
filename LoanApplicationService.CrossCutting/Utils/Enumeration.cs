﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanApplicationService.CrossCutting.Utils
{
    public enum LoanProductType
    {
        [Description("Digital")]
        Digital = 0,
        [Description("LogBook")]
        LogBook = 1 
    }

    public enum NotificationHeader
    {
        [Description("Email")]
        Email = 0,
        [Description("SMS")]
        SMS = 1,
        [Description("Push Notification")]
        PushNotification = 2,
        [Description("Loan Approved")]
        LoanApproved = 3
    }

    public enum PaymentFrequency
    {
        
        [Description("Weekly")]
        Weekly =0,
        [Description("Biweekly")]
        Biweekly = 1,
        [Description("Monthly")]
        Monthly =2,
        [Description("Quarterly")]
        Quarterly = 3,



    }

    public enum LoanStatus
    {
        [Description("Pending")]
        Pending = 0,
        [Description("Approved")]
        Approved = 1,
        [Description("Rejected")]
        Rejected = 2,
        [Description("Disbursed")]
        Disbursed = 3,
        [Description("Customer Rejected")]   
        CustomerRejected = 4,
        [Description("Closed")]
        Closed = 5
         
    }       

    public enum LoanRiskLevel
    {
        [Description("Very Low")]
        VeryLow = 0,
        [Description("Low")]
        Low = 1,
        [Description("Medium")]
        Medium = 2,
        [Description("High")]
        High = 3,
        [Description("Very High")]
        VeryHigh = 4
    }

    public enum Role
    {
        SuperAdmin = 0,
        Admin = 1,
        Customer = 2
    }

    public static class RiskScoringUtil
    {
        public static LoanRiskLevel GetRiskLevel(int age, string employmentStatus, decimal income)
        {
            int score = 0;

            // Age points
            if (age >= 18 && age <= 25) score += 1;
            else if (age >= 26 && age <= 35) score += 2;
            else if (age >= 36 && age <= 50) score += 3;
            else if (age >= 51 && age <= 60) score += 2;
            else if (age >= 61) score += 1;

            // Employment status points
            if (!string.IsNullOrWhiteSpace(employmentStatus))
            {
                var status = employmentStatus.Trim().ToLower();
                if (status == "employed") score += 3;
                else if (status == "self-employed") score += 2;
                // else unemployed/other: 0 points
            }

            // Income points
            if (income < 250000) score += 1;
            else if (income < 500000) score += 2;
            else if (income < 1000000) score += 3;
            else score += 4;

            // Risk level mapping
            if (score >= 8) return LoanRiskLevel.VeryLow;
            if (score >= 6) return LoanRiskLevel.Low;
            if (score >= 4) return LoanRiskLevel.Medium;
            if (score >= 2) return LoanRiskLevel.High;
            return LoanRiskLevel.VeryHigh;
        }
    }

    public enum PaymentMethods
    {
       
        [Description("Bank Transfer")]
        BankTransfer = 0,
        [Description("Mobile Money")]
        MobileMoney = 1
        
    }

    public enum AccountStatus
    {
        [Description("Active")]
        Active = 0,
        [Description("Inactive")]
        Inactive = 1,
        [Description("Closed")]
        Closed = 2,
        [Description("Frozen")]
        Frozen = 3,
        [Description("Defaulted")]
        Defaulted = 4
    }

    public enum TransactionType
    {
        
        [Description("Withdrawal")]
        Withdrawal = 0,
        [Description("Payment")]
        Payment = 1,
        
    }
}
