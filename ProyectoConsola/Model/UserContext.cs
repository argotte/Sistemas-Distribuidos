using Microsoft.EntityFrameworkCore;
namespace ProyectoConsola.Model;

public class UserContext : DbContext
{
    public DbSet<User>? Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=localhost;Database=bd_sd2023;user=sa;password=Tr4_oht10q;Trusted_Connection=False;TrustServerCertificate=true");
    }
}