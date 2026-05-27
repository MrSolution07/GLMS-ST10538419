using Microsoft.EntityFrameworkCore;
using GLMS.Web.Data;
using GLMS.Web.Services;
using GLMS.Web.Services.Factories;
using GLMS.Web.Services.Observers;
using GLMS.Web.Services.Strategies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// EF Core — SQL Server (connection string in appsettings.json)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Factory Method pattern — all three factories registered; resolver picks the right one
builder.Services.AddTransient<IContractFactory, StandardContractFactory>();
builder.Services.AddTransient<IContractFactory, PremiumContractFactory>();
builder.Services.AddTransient<IContractFactory, EnterpriseContractFactory>();
builder.Services.AddTransient<ContractFactoryResolver>();

// Observer pattern — all observers registered; controllers iterate over them
builder.Services.AddTransient<IContractObserver, AuditLogObserver>();
builder.Services.AddTransient<IContractObserver, EmailNotificationObserver>();

// Strategy pattern — validation strategies
builder.Services.AddTransient<ServiceRequestValidationStrategy>();
builder.Services.AddTransient<FileValidationStrategy>();

// Application services
builder.Services.AddTransient<IFileService, FileService>();
builder.Services.AddHttpClient<ICurrencyService, CurrencyService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Auto-apply migrations on startup (development convenience)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.Run();

// Partial class declaration to allow test projects to reference the entry point
public partial class Program { }
