using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Entities.Authentication;
using SchoolManagement.Domain.Entities.AuthenticationAndAuthorization;
using SchoolManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.DataAccess.DataContext
{
    public sealed class ApplicationDbContext : IdentityDbContext<ApplicationUsers>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<ApplicationUsers> ApplicationUsers { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Student> Students { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure RefreshToken
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Token).IsRequired();
                entity.Property(e => e.UserId).IsRequired();
                entity.HasIndex(e => e.Token).IsUnique();
            });

            // Configure ApplicationUsers
            modelBuilder.Entity<ApplicationUsers>(entity =>
            {
                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Address)
                    .HasMaxLength(500);

                entity.Property(e => e.RefreshToken)
                    .HasMaxLength(500);
            });
            // Student configuration
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.StudentCode)
                    .IsUnique()
                    .HasDatabaseName("IX_Students_StudentCode");
                entity.HasIndex(e => e.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_Students_Email");

                entity.HasIndex(e => e.Status)
                    .HasDatabaseName("IX_Students_Status");
                entity.HasIndex(e => e.Class)
                    .HasDatabaseName("IX_Students_Class");
                entity.HasIndex(e => e.City)
                    .HasDatabaseName("IX_Students_City");
                entity.HasIndex(e => e.Country)
                    .HasDatabaseName("IX_Students_Country");
                entity.Property(e => e.Status)
                    .HasDefaultValue(StudentStatus.Active);
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);
            });
        }
    }
}