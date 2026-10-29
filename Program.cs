using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QLKTX.Data;
using QLKTX.Data.Entities;
using QLKTX.Services;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Dormitory");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Cần cấu hình ConnectionStrings:Dormitory bằng User Secrets hoặc biến môi trường.");
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<QlKtxContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddScoped<IPasswordHasher<user>, PasswordHasher<user>>();
builder.Services.AddScoped<DemoDataSeeder>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Denied";
        options.Cookie.Name = "qlktx.session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromHours(10);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<QlKtxContext>();
    if (await db.Database.CanConnectAsync())
        await scope.ServiceProvider.GetRequiredService<DemoDataSeeder>().SeedAsync();
}

app.MapControllerRoute("default", "{controller=Dormitory}/{action=Index}/{id?}");
app.Run();
