using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using RimDev.Stuntman.Core;

public class NormalRole : IdentityRole<int>
{
    public NormalRole() {}
    public NormalRole(string name): this()
    {
        this.Name = name;
    }
    // public string Test { get; set; } // can add properties
}

// Add profile data for application users by adding properties to the User class
public class User : IdentityUser<int>
{
    // public string Test { get; set; } // can add properties
}

public partial class Handler {
    public static readonly StuntmanOptions StuntmanOptions = new StuntmanOptions();

    public void CreateStuntUsers(){
        StuntmanOptions
        .AddUser(
            new StuntmanUser("user-1", "User 1")
                .AddClaim("name", "John Doe"));
    }
}