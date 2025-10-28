using Microsoft.EntityFrameworkCore;
using back.Models;

namespace back.Data
{
  public class ApplicationDbContext : DbContext
  {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<Song> Songs { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      modelBuilder.Entity<Team>()
          .HasMany(t => t.Users)
          .WithMany()
          .UsingEntity(j => j.ToTable("TeamUsers"));

      modelBuilder.Entity<Team>()
          .HasMany(t => t.Songs)
          .WithMany()
          .UsingEntity(j => j.ToTable("TeamSongs"));

      modelBuilder.Entity<Team>()
          .HasMany(t => t.Messages)
          .WithMany()
          .UsingEntity(j => j.ToTable("TeamChatMessages"));

      modelBuilder.Entity<User>()
          .Property(u => u.Role)
          .HasConversion<string>();
    }
  }
}
