using LoanApplicationService.Core.Repository;
using LoanApplicationService.Service.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<LoanApplicationServiceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection") ?? throw new InvalidOperationException("Connection string 'Loan_application_serviceContext' not found.")));

builder.Services.AddMvc();

builder.Services.AddScoped<ILoanProductService, LoanProductServiceImpl>();

// Add services to the container.
builder.Services.AddRazorPages();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAutoMapper(typeof(Program));

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
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();


app.Run();
