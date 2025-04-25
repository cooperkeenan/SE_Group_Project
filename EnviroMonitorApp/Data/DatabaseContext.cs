using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using EnviroMonitorApp.Models;


namespace EnviroMonitorApp.Data;

public class DatabaseContext : DbContext
{
    public DatabaseContext()
    { }

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    { }
    
    
}