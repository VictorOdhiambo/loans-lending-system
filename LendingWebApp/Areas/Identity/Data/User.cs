using Loan_application_service.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Loan_application_service.Areas.Identity.Data;

// Add profile data for application users by adding properties to the User class
public class User : IdentityUser
{

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime LastLogin {  get; set; }

    




}

