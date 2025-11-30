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
    public DbSet<SongRating> SongRatings { get; set; }
    public DbSet<GlobalUser> GlobalUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      // Enforce unique constraint on GlobalUser.Name
      modelBuilder.Entity<GlobalUser>()
          .HasIndex(u => u.Name)
          .IsUnique();
      
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

      modelBuilder.Entity<SongRating>()
          .HasIndex(r => new { r.SongId, r.UserId })
          .IsUnique();
    }
  }
}
