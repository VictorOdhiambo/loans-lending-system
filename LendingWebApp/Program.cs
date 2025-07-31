using LoanApplicationService.Core.Models;
using LoanApplicationService.Core.Repository;
=======
﻿using LoanApplicationService.Core.Repository;
>>>>>>> Stashed changes
using LoanApplicationService.Service.Mapper.LoanModuleMapper;
using LoanApplicationService.Service.Services;
using LoanApplicationService.Web.Helpers;
using LoanManagementApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using LoanApplicationService.Core.Models;
using LendingWebApp;
using LoanApplicationService.Web.Helpers;
using LoanManagementApp.Models;

// ✅ Avoid ambiguous Role reference
using AppRole = LoanApplicationService.CrossCutting.Utils.Role;

var builder = WebApplication.CreateBuilder(args);

// Add MVC
builder.Services.AddControllersWithViews();

// AutoMapper
builder.Services.AddAutoMapper(typeof(LoansProfile).Assembly);

// EF Core DbContext
builder.Services.AddDbContext<LoanApplicationServiceDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection"));
});

// Register application services
builder.Services.AddScoped<IUserService, UserServiceImpl>();
builder.Services.AddScoped<ICustomerService, CustomerServiceImpl>();
builder.Services.AddScoped<ILoanProductService, LoanProductServiceImpl>();
builder.Services.AddScoped<ILoanChargeService, LoanChargeServiceImpl>();
builder.Services.AddScoped<INotificationTemplateService, NotificationTemplateService>();
builder.Services.AddScoped<INotificationSenderService, NotificationSenderService>();
builder.Services.AddScoped<ILoanApplicationService, LoanApplicationServiceImpl>();
builder.Services.AddScoped<IAccountService, AccountServiceImpl>();
builder.Services.AddScoped<ILoanPaymentService, LoanPaymentImpl>();
builder.Services.AddScoped<IRepaymentScheduleService, LoanRepaymentScheduleService>();
builder.Services.AddScoped<ILoanWithdrawalService, LoanWithdrawalServiceImpl>();

// Email Service registration, 
builder.Services.AddScoped<IEmailService, EmailService>();
// Register EmailSettings for DI
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
// Add others here
=======
>>>>>>> Stashed changes

// Email service
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Swagger (Dev only)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<LoanMonitoringService>();


//  Enable Session
=======
// Session
>>>>>>> Stashed changes
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Identity + Password Hashing
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<LoanApplicationServiceDbContext>()
.AddDefaultTokenProviders();

// Configure authentication with cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Home/Index";
    options.LogoutPath = "/Home/Logout";
    options.AccessDeniedPath = "/Home/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
});

builder.Services.AddScoped<IPasswordHasher<ApplicationUser>, BCryptPasswordHasher>();

// builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation(); // Uncomment if using runtime view compilation

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Default route (Login page)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ✅ Seed roles and SuperAdmin
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

    // Seed Roles
    foreach (var roleName in Enum.GetNames(typeof(AppRole)))
    {
<<<<<<< Updated upstream
<<<<<<< HEAD
        var superAdmin = new LoanApplicationService.Core.Models.Users
=======
        var admin = new Users
>>>>>>> main
=======
          if (!roleManager.Roles.Any(r => r.Name == roleName))
>>>>>>> Stashed changes
        {
            roleManager.CreateAsync(new ApplicationRole { Name = roleName }).Wait();
        }
    }

    // Seed SuperAdmin
    var superAdminRole = roleManager.Roles.FirstOrDefault(r => r.Name == AppRole.SuperAdmin.ToString());

    if (superAdminRole != null && !userManager.Users.Any(u => u.Email == "superadmin@pesasure.com"))
    {
        var admin = new Users
        {
            UserName = "SuperAdmin",
            Email = "superadmin@pesasure.com",
            IsActive = true
        };

        var result = userManager.CreateAsync(superAdmin, "Super@123").Result;
        if (result.Succeeded)
        {
            userManager.AddToRoleAsync(superAdmin, superAdminRole.Name ?? "SuperAdmin").Wait();
        }
    }
}

app.Run();
