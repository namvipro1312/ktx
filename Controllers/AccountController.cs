using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKTX.Data;
using QLKTX.Data.Entities;

namespace QLKTX.Controllers;

public sealed class AccountController(QlKtxContext db, IPasswordHasher<user> hasher, IWebHostEnvironment environment) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["ShowDemoCredentials"] = environment.IsDevelopment();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
    {
        username = (username ?? "").Trim();
        if (username.Length > 100) username = username[..100];
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var lockout = await db.login_attempts.CountAsync(x => x.username == username && !x.succeeded && x.attempted_at >= DateTime.Now.AddMinutes(-15));
        if (lockout >= 8)
        {
            db.login_attempts.Add(new login_attempt { username = username, ip_address = ip, succeeded = false, attempted_at = DateTime.Now });
            await db.SaveChangesAsync();
            ModelState.AddModelError("", "Tài khoản tạm khóa đăng nhập do có nhiều lần thử sai. Hãy đợi 15 phút rồi thử lại.");
            return View();
        }
        var user = await db.users.Include(x => x.user_roles).ThenInclude(x => x.role)
            .SingleOrDefaultAsync(x => x.username == username && x.is_active);
        var valid = user is not null && hasher.VerifyHashedPassword(user, user.password_hash, password ?? "") != PasswordVerificationResult.Failed;
        db.login_attempts.Add(new login_attempt { username = username, ip_address = ip, succeeded = valid, attempted_at = DateTime.Now });
        if (!valid)
        {
            await db.SaveChangesAsync();
            ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu chưa chính xác.");
            ViewData["ShowDemoCredentials"] = environment.IsDevelopment();
            return View();
        }

        user!.last_login_at = DateTime.Now;
        await db.SaveChangesAsync();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.id.ToString()),
            new(ClaimTypes.Name, user.full_name),
            new("username", user.username)
        };
        claims.AddRange(user.user_roles.Select(x => new Claim(ClaimTypes.Role, x.role.code)));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
            new AuthenticationProperties { IsPersistent = false, AllowRefresh = true });
        return Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl!) : RedirectToAction("Index", "Dormitory");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    public IActionResult Denied() => View();
}
