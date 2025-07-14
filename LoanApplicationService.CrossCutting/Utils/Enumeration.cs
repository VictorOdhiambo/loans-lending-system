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
        PushNotification = 2
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
}
