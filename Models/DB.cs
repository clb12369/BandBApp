using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;

public class DbFactory : IDbContextFactory<DB> {
    public DB Create(DbContextFactoryOptions options)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Path.GetFullPath("."))
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables(prefix: "ASPNETCORE_")
            .Build();        

        var builder = new DbContextOptionsBuilder<DB>();
        // builder.UseNpgsql(config.GetConnectionString("Postgres:Dev"));
        // builder.UseSqlite(config.GetConnectionString("Sqlite"));
        return new DB(builder.Options);
    }
}

public partial class DB : DbContext {
    public DB(DbContextOptions<DB> options): base(options){}

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}