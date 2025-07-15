using LoanApplicationService.Core.Repository;
using LoanApplicationService.Service.Mapper.LoanModuleMapper;
using LoanApplicationService.Service.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<LoanApplicationServiceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection") ?? throw new InvalidOperationException("Connection string 'DbConnection' not found.")));

builder.Services.AddMvc();

builder.Services.AddScoped<ILoanProductService, LoanProductServiceImpl>();
builder.Services.AddScoped<ILoanChargeService, LoanChargeServiceImpl>();
builder.Services.AddScoped<INotificationSenderService, NotificationSenderService>();
builder.Services.AddScoped<INotificationTemplateService, NotificationTemplateService>();
builder.Services.AddScoped<ILoanApplicationService, LoanApplicationServiceImpl>();
builder.Services.AddScoped<ILoanApplicationService, LoanApplicationServiceImpl>();
builder.Services.AddScoped<INotificationSenderService, NotificationSenderService>();
builder.Services.AddScoped<INotificationTemplateService, NotificationTemplateService>();


// Add services to the container.
builder.Services.AddRazorPages();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAutoMapper(typeof(Program));



builder.Services.AddAutoMapper(typeof(LoansProfile));





builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add auto mapper
var app = builder.Build();




if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
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
