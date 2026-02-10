using elbanna.Data;
using Microsoft.EntityFrameworkCore;
using YourProject.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.CommandTimeout(120)
    )
);

// Repository
builder.Services.AddScoped<ItemPurchaseRepository>();

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 🔐 تفعيل الحماية العامة (مهم جدًا)
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<elbanna.Helpers.AuthorizeUserAttribute>();
});



var app = builder.Build();

Rotativa.AspNetCore.RotativaConfiguration.Setup(
    @"C:\Program Files\wkhtmltopdf\bin"
);

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
