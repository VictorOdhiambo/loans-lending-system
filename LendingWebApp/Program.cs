using Microsoft.EntityFrameworkCore;
using Loan_application_service.Data;
using AutoMapper;
using Loan_application_service.Models;
using Loan_application_service.DTOs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<LoanApplicationServiceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Loan_application_serviceContext") ?? throw new InvalidOperationException("Connection string 'Loan_application_serviceContext' not found.")));


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
app.UseAuthorization();
app.UseHttpsRedirection();
app.UseRouting();



app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=LoanProductController}/{action=GetAl}/{id?}")
    .WithStaticAssets();

app.Run();
