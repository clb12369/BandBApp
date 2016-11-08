using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;

public interface IAuthService {
    Task<bool> Login(string email, string pass);
    Task Logout();
    Task<bool> Register(string email, string pass);
    Task<bool> ResetPassword(string email, Func<Object, string> getCallbackUrl);
    // Task<IdentityUser> GetUser();
}

public class AuthService : IAuthService {
    private UserManager<IdentityUser> u;
    private SignInManager<IdentityUser> s;
    private IEmail emailer;

    public AuthService(UserManager<IdentityUser> u, SignInManager<IdentityUser> s, IEmail emailer){
        this.u = u;
        this.s = s;
        this.emailer = emailer;
    }

    public async Task<bool> Login(string email, string pass){
        return (await s.PasswordSignInAsync(email, pass, true, lockoutOnFailure: false)).Succeeded;
    }

    public async Task<bool> Register(string email, string pass){
        var user = new IdentityUser { UserName = email, Email = email };
        if((await u.CreateAsync(user, pass)).Succeeded){
            await s.SignInAsync(user, isPersistent: true);
            return true;
        }
        return false;
    }

    public async Task Logout() => await s.SignOutAsync();

    public async Task<bool> ResetPassword(string email, Func<Object, string> getCallbackUrl){
        var user = await u.FindByNameAsync(email);
        
        if (user != null || (await u.IsEmailConfirmedAsync(user)))
            return false;
        
        var code = await u.GeneratePasswordResetTokenAsync(user);
        var url = getCallbackUrl(new { userId = user.Id, code = code });

        await emailer.SendEmailAsync(
            email,
            "Reset Password",
            $"Please reset your password by clicking here: <a href='{url}'>link</a>");
        
        return true;
    }

    // public Task<IdentityUser> GetUser(HttpContext) => await u.GetUserAsync(HttpContext.User);
}
