using DoctorAppointmentApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace DoctorAppointmentApi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasIndex(d => d.Email).IsUnique();
            entity.Property(d => d.Fees).HasColumnType("decimal(10,2)");
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.Property(a => a.Amount).HasColumnType("decimal(10,2)");

            entity.HasOne(a => a.User)
                  .WithMany(u => u.Appointments)
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Doctor)
                  .WithMany(d => d.Appointments)
                  .HasForeignKey(a => a.DoctorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
