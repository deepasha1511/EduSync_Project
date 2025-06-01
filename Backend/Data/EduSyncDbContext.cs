using EduSync.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Backend.Data
{
    public class EduSyncDbContext : DbContext
    {
        public EduSyncDbContext() { }

        public EduSyncDbContext(DbContextOptions<EduSyncDbContext> options)
            : base(options) { }

       
        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Assessment> Assessments { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=EduSyncDB;Integrated Security=True;TrustServerCertificate=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User Entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.UserId)
                      .ValueGeneratedNever(); 

                entity.Property(e => e.Name)
                      .HasMaxLength(50)
                      .IsUnicode(false);
            });

            // Course Entity
            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(e => e.CourseId);

                entity.Property(c => c.Title)
                      .HasMaxLength(200);

                entity.Property(c => c.Description)
                      .HasMaxLength(1000);

                entity.HasOne(c => c.Instructor)
                      .WithMany(u => u.Courses)
                      .HasForeignKey(c => c.InstructorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Assessment Entity
            modelBuilder.Entity<Assessment>(entity =>
            {
                entity.HasKey(a => a.AssessmentId);

                entity.Property(a => a.Title)
                      .HasMaxLength(200);

                entity.Property(a => a.Questions)
                      .HasColumnType("varchar(MAX)");

                entity.Property(a => a.MaxScore);

                entity.HasOne(a => a.Course)
                      .WithMany(c => c.Assessments)
                      .HasForeignKey(a => a.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Result Entity
            modelBuilder.Entity<Result>(entity =>
            {
                entity.HasKey(r => r.ResultId);

                entity.HasOne(r => r.User)
                      .WithMany(u => u.Results)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Assessment)
                      .WithMany(a => a.Results)
                      .HasForeignKey(r => r.AssessmentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            //Enrollment Entity
            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.HasOne(e => e.Course)
                    .WithMany(c => c.Enrollments)
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Enrollments)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

        }
    }
}
