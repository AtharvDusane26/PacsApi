using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PacsApi.Models;

namespace PacsApi.Context
{
    //dotnet ef migrations add InitialCreate --context PacsDbContext
    //dotnet ef database update --context PacsDbContext
    //dotnet ef migrations add MigrationName
    public class PacsDbContext : DbContext
    {
        public PacsDbContext(DbContextOptions<PacsDbContext> options)
            : base(options) { }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Study> Studies { get; set; }
        public DbSet<Series> Series { get; set; }
        public DbSet<Image> Images { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Study>()
                .HasOne(s => s.Patient)
                .WithMany(p => p.Studies)
                .HasForeignKey(s => s.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Series>()
                .HasOne(s => s.Study)
                .WithMany(st => st.Series)
                .HasForeignKey(s => s.StudyInstanceUid)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Image>()
                .HasOne(i => i.Series)
                .WithMany(s => s.Images)
                .HasForeignKey(i => i.SeriesInstanceUid)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔥 UNIQUE CONSTRAINTS (VERY IMPORTANT)
            modelBuilder.Entity<Patient>()
                .HasIndex(p => p.PatientId)
                .IsUnique();

            modelBuilder.Entity<Study>()
                .HasIndex(s => s.StudyInstanceUid)
                .IsUnique();
            modelBuilder.Entity<Study>()
                .HasIndex(s => s.PatientId);

            modelBuilder.Entity<Series>()
                .HasIndex(s => s.SeriesInstanceUid)
                .IsUnique();
            modelBuilder.Entity<Series>()
                .HasIndex(s => s.StudyInstanceUid);


            modelBuilder.Entity<Image>()
        .HasIndex(i => i.SopInstanceUid)
        .IsUnique();

            modelBuilder.Entity<Image>()
                .HasIndex(i => new { i.SeriesInstanceUid, i.InstanceNumber });

            modelBuilder.Entity<Image>()
                .HasIndex(i => i.StudyInstanceUid);

        }
    }


    public class PacsDbContextFactory : IDesignTimeDbContextFactory<PacsDbContext>
    {
        public PacsDbContext CreateDbContext(string[] args)
        {
            var dbPath = Path.Combine(
                GeneralSettings.
                BaseDirectory,
                GeneralSettings.DatabaseName);

            var optionsBuilder = new DbContextOptionsBuilder<PacsDbContext>();
            optionsBuilder.UseSqlite($"Data Source={dbPath}");

            return new PacsDbContext(optionsBuilder.Options);
        }
    }
}
