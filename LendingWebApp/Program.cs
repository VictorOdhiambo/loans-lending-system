using LoanApplicationService.Core.Models;
using LoanApplicationService.Core.Repository;
using LoanApplicationService.Service.Mapper.LoanModuleMapper;
using LoanApplicationService.Service.Services;
using LoanApplicationService.Web.Helpers;
using LoanManagementApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity; 
using LendingWebApp;
using AutoMapper;


// ✅ Avoid ambiguous Role reference
using AppRole = LoanApplicationService.CrossCutting.Utils.Role;
using LoanApplicationService.Service.Mapper.TransactionsModuleMapper;
using LoanApplicationService.Service.Mapper.RepaymentScheduleMapper;

var builder = WebApplication.CreateBuilder(args);

// Add MVC
builder.Services.AddControllersWithViews();

// AutoMapper
builder.Services.AddAutoMapper(typeof(LoansProfile).Assembly);

builder.Services.AddAutoMapper(typeof(TransactionsProfile).Assembly);
builder.Services.AddAutoMapper(typeof(RepaymentScheduleProfile).Assembly);

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
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IReportService, ReportServiceImpl>();    


// Email service
builder.Services.AddTransient<LoanApplicationService.Web.Services.EmailService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Swagger (Dev only)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<LoanMonitoringService>();


// Session
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

builder.Services.AddScoped<IPasswordHasher<ApplicationUser>, LendingWebApp.Helpers.BCryptPasswordHasher>();

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
    var dbContext = scope.ServiceProvider.GetRequiredService<LoanApplicationServiceDbContext>();

    // Seed Roles
    foreach (var roleName in Enum.GetNames(typeof(AppRole)))
    {
        if (!roleManager.Roles.Any(r => r.Name == roleName))
        {
            roleManager.CreateAsync(new ApplicationRole { Name = roleName }).Wait();
        }
    }

    // Seed SuperAdmin
    var superAdminRole = roleManager.Roles.FirstOrDefault(r => r.Name == AppRole.SuperAdmin.ToString());

    if (superAdminRole != null && !userManager.Users.Any(u => u.Email == "superadmin@pesasure.com"))
    {
        var superAdmin = new ApplicationUser
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

    // Seed Password Reset Email Template
    if (!dbContext.NotificationTemplates.Any(t => t.NotificationHeader == "Password Reset" && t.Channel == "email"))
    {
        var passwordResetTemplate = new NotificationTemplate
        {
            NotificationHeader = "Password Reset",
            Channel = "email",
            Subject = "Password Reset Request - Loan Management System",
            BodyText = @"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #333;'>Password Reset Request</h2>
                    <p>Hello {{UserName}},</p>
                    <p>We received a request to reset your password for your Loan Management System account.</p>
                    <p>Click the button below to reset your password:</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{{ResetLink}}' style='background-color: #007bff; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block;'>Reset Password</a>
                    </div>
                    <p>If the button doesn't work, copy and paste this link into your browser:</p>
                    <p style='word-break: break-all; color: #666;'>{{ResetLink}}</p>
                    <p><strong>Important:</strong></p>
                    <ul>
                        <li>This link will expire in 24 hours</li>
                        <li>If you didn't request this password reset, please ignore this email</li>
                        <li>For security reasons, this link can only be used once</li>
                    </ul>
                    <p>If you have any questions, please contact our support team.</p>
                    <p>Best regards,<br>Loan Management System Team</p>
                </div>"
        };

        dbContext.NotificationTemplates.Add(passwordResetTemplate);
        dbContext.SaveChanges();
    }
}

app.Run();
