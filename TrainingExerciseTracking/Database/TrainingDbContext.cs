using Microsoft.EntityFrameworkCore;
using TrainingExerciseTracking.Database.Models;

namespace TrainingExerciseTracking.Database;

public class TrainingDbContext : DbContext
{
    public DbSet<Participant> Participants { get; set; }
    public DbSet<Movement> Movements { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = "server=localhost;database=TrainingExerciseTracking;user id=sa;password=Sml123456789;trustservercertificate=True";
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}