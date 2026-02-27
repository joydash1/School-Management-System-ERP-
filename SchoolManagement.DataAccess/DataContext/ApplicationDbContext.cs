using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Entities.Authentication;
using SchoolManagement.Domain.Entities.AuthenticationAndAuthorization;
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

                // Ignore FullName as it's computed
                entity.Ignore(e => e.FullName);

                // Relationships
                entity.HasOne(e => e.Student)
                    .WithMany(s => s.Users)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Teacher)
                    .WithMany(t => t.Users)
                    .HasForeignKey(e => e.TeacherId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Teacher
            modelBuilder.Entity<Teacher>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.EmployeeNumber)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Department)
                    .HasMaxLength(100);

                entity.Property(e => e.Qualification)
                    .HasMaxLength(200);

                entity.Property(e => e.JoinDate)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            });

            // Configure Student
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.StudentNumber)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Grade)
                    .HasMaxLength(20);

                entity.Property(e => e.Class)
                    .HasMaxLength(20);

                entity.Property(e => e.EnrollmentDate)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}