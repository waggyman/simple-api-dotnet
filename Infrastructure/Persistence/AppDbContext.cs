using Microsoft.EntityFrameworkCore;
using simple_api.Domain.Entities;

namespace simple_api.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Todo> Todos => Set<Todo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(user => user.Id);
            entity.HasIndex(user => user.Email).IsUnique();
            entity.Property(user => user.Email).HasMaxLength(256).IsRequired();
            entity.Property(user => user.PasswordHash).IsRequired();
        });

        modelBuilder.Entity<Todo>(entity =>
        {
            entity.HasKey(todo => todo.Id);
            entity.Property(todo => todo.Title).HasMaxLength(200).IsRequired();
            entity.Property(todo => todo.Description).HasMaxLength(2000);
            entity.HasOne(todo => todo.User)
                .WithMany(user => user.Todos)
                .HasForeignKey(todo => todo.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
