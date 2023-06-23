using Microsoft.EntityFrameworkCore;
namespace ProyectoConsola.Model;

public class UserContext : DbContext
{
    public DbSet<User>? Users{ get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Data Source=DESKTOP-LIVLF6G;Initial Catalog=bd_sd2023;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");

    }
}