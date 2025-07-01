using Microsoft.EntityFrameworkCore;
using Loan_application_service.Data;
using AutoMapper;
using Loan_application_service.Models;
using Loan_application_service.DTOs;
using Loan_application_service.Repository;
using Microsoft.AspNetCore.Identity;
using Loan_application_service.Areas.Identity.Data;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<LoanApplicationServiceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Loan_application_serviceContext") ?? throw new InvalidOperationException("Connection string 'Loan_application_serviceContext' not found.")));

//identity data context
builder.Services.AddDbContext<Loan_application_serviceContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Loan_application_serviceContext") ?? throw new InvalidOperationException("Connection string 'Loan_application_serviceContext' not found.")));


builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<Loan_application_serviceContext>();

//identity validation configurations
builder.Services.Configure<IdentityOptions>(options => {
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;


});

builder.Services.ConfigureApplicationCookie(options =>
{ //options.LoginPath = ""
  //options.Cookie.HttpOnly = true;
  //options.AccessDeniedPath = "/";
  // options.ExpireTimeSpan = TimeSpan.Zero;

});


// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddScoped<ILoanProductRepository, LoanProductRepository>();

builder.Services.AddRazorPages();




// Add auto mapper
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();
app.Run();
