using LoanApplicationService.Core.Repository;
using LoanApplicationService.Service.Mapper.LoanModuleMapper;
using LoanApplicationService.Service.Services;
using Microsoft.EntityFrameworkCore;

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
builder.Services.AddScoped<RepaymentServiceImpl>();
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


//  Auto-seed admin user on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LoanApplicationServiceDbContext>();

    if (!db.Users.Any(u => u.Email == "admin@lms.com"))
    {
        var admin = new LoanApplicationService.Core.Models.Users
        {
            Id = Guid.NewGuid(),
            Username = "SuperAdmin",
            Email = "admin@lms.com",
            Role = "Admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            IsDeleted = false
        };

        db.Users.Add(admin);
        db.SaveChanges();
    }
}

app.Run();
