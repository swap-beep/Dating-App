using System;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data;


public class DataContext(DbContextOptions options) : DbContext(options)
{
public required DbSet<AppUser> Users {get;set;}
public DbSet<Like> Likes { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Like>()
        .HasKey(like => new { like.SourceUserId, like.TargetUserId });

    modelBuilder.Entity<Like>()
        .HasOne(like => like.SourceUser)
        .WithMany(user => user.LikedUsers)
        .HasForeignKey(like => like.SourceUserId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<Like>()
        .HasOne(like => like.TargetUser)
        .WithMany(user => user.LikedByUsers)
        .HasForeignKey(like => like.TargetUserId)
        .OnDelete(DeleteBehavior.Restrict);
}

    }

