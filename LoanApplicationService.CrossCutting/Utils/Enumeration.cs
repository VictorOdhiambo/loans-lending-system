using System;
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
        [Description("Daily")]
        Daily = 0,
        [Description("Weekly")]
        Weekly =1,
        [Description("FortNightly")]
        FortNightly = 2,
        [Description("Monthly")]
        Monthly =3
        

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

    public static class RiskScoringUtil
    {
        public static LoanRiskLevel GetRiskLevel(int age, bool isEmployed, decimal income)
        {
            int score = 0;
            // Age points
            if (age >= 18 && age <= 25) score += 1;
            else if (age >= 26 && age <= 35) score += 2;
            else if (age >= 36 && age <= 45) score += 3;
            else if (age >= 46 && age <= 55) score += 4;
            else if (age >= 56) score += 2;
            // Employment status points
            score += isEmployed ? 2 : 0;
            // Income points
            if (income >= 0 && income <= 250000) score += 1;
            else if (income <= 500000) score += 2;
            else if (income <= 1000000) score += 3;
            else if (income <= 1500000) score += 4;
            // Risk level mapping
            if (score >= 2 && score <= 4) return LoanRiskLevel.VeryHigh;
            if (score >= 5 && score <= 6) return LoanRiskLevel.High;
            if (score >= 7 && score <= 8) return LoanRiskLevel.Medium;
            if (score >= 9 && score <= 10) return LoanRiskLevel.Low;
            return LoanRiskLevel.VeryLow;
        }
    }

    public enum PaymentMethods
    {
       
        [Description("Bank Transfer")]
        BankTransfer = 0,
        [Description("Mobile Money")]
        MobileMoney = 1
        
    }
}
