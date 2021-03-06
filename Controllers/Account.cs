using System;
using System.Linq;
using Microsoft.AspNetCore.Session;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

[Authorize]
[Route("/account")]
public class AccountController : Controller
{
    private readonly IAuthService auth;
    public AccountController(IAuthService auth){
        this.auth = auth;
    }

    [HttpGet]
    public IActionResult Root()
    {
        // HttpContext.User
        return Ok();
    }

    [HttpGet("register")]
    [AllowAnonymous]
    public IActionResult Register()
    {
        ViewData["Action"] = "Register";
        return View("RegisterOrLogin");
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromForm] UserView user)
    {
        if (!ModelState.IsValid || !(await auth.Register(user.Email, user.Password))){
            ModelState.AddModelError("", "That email/password combination did not work.");
            ViewData["Action"] = "Register";
            return View("RegisterOrLogin", user);
        }
        
        return Redirect("/account");
    }

    [HttpGet("login")]
    [AllowAnonymous]
    public IActionResult Login()
    {
        ViewData["Action"] = "Login";
        return View("RegisterOrLogin");
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromForm] UserView user)
    {
        if (!ModelState.IsValid){
            ModelState.AddModelError("", "Both email/password are required.");
            ViewData["Action"] = "Login";
            return View("RegisterOrLogin", user);
        }

        if(await auth.Login(user.Email, user.Password)){
            return Redirect("/account");
        }

        return BadRequest();
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await auth.Logout();
        return Redirect("/");
    }
}

public class UserView {
    [Required]
    [EmailAddress]
    public string Email {get;set;}
    [Required]
    [DataType(DataType.Password)]
    public string Password {get;set;}
}