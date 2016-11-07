using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;

public interface IAuthService {
    Task<bool> Login(string email, string pass, bool rememberme);
    Task Logout();
    Task<bool> Register(string email, string pass);
    Task<bool> ResetPassword(string email, Func<Object, string> getCallbackUrl);
    // Task<User> GetUser();
}

public class AuthService : IAuthService {
    private UserManager<User> u;
    private SignInManager<User> s;
    private IEmail emailer;

    public AuthService(UserManager<User> u, SignInManager<User> s, IEmail emailer){
        this.u = u;
        this.s = s;
        this.emailer = emailer;
    }

    public async Task<bool> Login(string email, string pass, bool rememberme = true){
        return (await s.PasswordSignInAsync(email, pass, rememberme, lockoutOnFailure: false)).Succeeded;
    }

    public async Task<bool> Register(string email, string pass){
        var user = new User { UserName = email, Email = email };
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

    // public Task<User> GetUser(HttpContext) => await u.GetUserAsync(HttpContext.User);
    
}