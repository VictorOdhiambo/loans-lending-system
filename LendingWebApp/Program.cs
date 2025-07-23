using LoanApplicationService.Core.Repository;
using LoanApplicationService.Service.Mapper.LoanModuleMapper;
using LoanApplicationService.Service.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

//  Add services to the container
builder.Services.AddControllersWithViews();

//  Add AutoMapper
builder.Services.AddAutoMapper(typeof(LoansProfile).Assembly);

//  Register EF Core DbContext
builder.Services.AddDbContext<LoanApplicationServiceDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection"));
});

//  Dependency Injection for services
builder.Services.AddScoped<IUserService, UserServiceImpl>();
builder.Services.AddScoped<ICustomerService, CustomerServiceImpl>();
builder.Services.AddScoped<ILoanProductService, LoanProductServiceImpl>();
builder.Services.AddScoped<ILoanChargeService, LoanChargeServiceImpl>();
builder.Services.AddScoped<INotificationTemplateService, NotificationTemplateService>();
builder.Services.AddScoped<INotificationSenderService, NotificationSenderService>();
builder.Services.AddScoped<ILoanApplicationService, LoanApplicationServiceImpl>();
// Email Service registration
builder.Services.AddScoped<LoanApplicationService.Web.Helpers.IEmailService, LoanApplicationService.Web.Helpers.EmailService>();
// Register EmailSettings for DI
builder.Services.Configure<LoanManagementApp.Models.EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
// Add others here

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//  Enable Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add authentication and cookie authentication middleware to support claims-based login. This is required for SignInAsync to work.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Index";
        options.LogoutPath = "/Home/Logout";
        options.AccessDeniedPath = "/Home/Index";
    });


// builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

var app = builder.Build();

//  Middleware
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

// Enable Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//  Default Route: Home/Index (Login)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


//  Auto-seed SuperAdmin user on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LoanApplicationServiceDbContext>();

    if (!db.Users.Any(u => u.Email == "superadmin@pesasure.com"))
    {
        var superAdmin = new LoanApplicationService.Core.Models.Users
        {
            Id = Guid.NewGuid(),
            Username = "SuperAdmin",
            Email = "superadmin@pesasure.com",
            Role = "SuperAdmin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Super@123"),
            IsActive = true
        };

        db.Users.Add(superAdmin);
        db.SaveChanges();
    }
}

app.Run();
