using CallManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace CallManagement.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<Call> Calls => Set<Call>();
    public DbSet<LeadExtractionResult> LeadResults => Set<LeadExtractionResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Call>()
            .HasOne(c => c.Lead)
            .WithOne()
            .HasForeignKey<LeadExtractionResult>(l => l.CallId);
    }
}
