using Microsoft.EntityFrameworkCore;
using ElSentidoDelOido.Datos.Entities;
using ElSentidoDelOido.Datos.Entities.Enums;

namespace ElSentidoDelOido.Datos.DataContext
{
    public partial class ElSentidoDelOidoDBContext : DbContext
    {
        public ElSentidoDelOidoDBContext(DbContextOptions<ElSentidoDelOidoDBContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserRole> UserRoles { get; set; } = null!;
        public DbSet<Shift> Shifts { get; set; } = null!;
        public DbSet<ShiftSchedule> ShiftSchedules { get; set; } = null!;
        public DbSet<ShiftType> ShiftTypes { get; set; } = null!;
        public DbSet<Professional> Professionals { get; set; } = null!;
        public DbSet<Holidays> Holidays { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Users <-> UserRole (RoleId nullable)
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Role)
                      .WithMany(r => r.Users)
                      .HasForeignKey(e => e.RoleId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("UserRoles");
                entity.HasKey(e => e.Id);
            });

            // Professionals <-> Shifts
            modelBuilder.Entity<Professional>(entity =>
            {
                entity.ToTable("Professionals");
                entity.HasKey(e => e.Id);
                entity.HasMany(p => p.Shifts)
                      .WithOne(s => s.Professional)
                      .HasForeignKey(s => s.ProfessionalId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ShiftType <-> Shifts
            modelBuilder.Entity<ShiftType>(entity =>
            {
                entity.ToTable("ShiftTypes");
                entity.HasKey(e => e.Id);
                entity.HasMany(t => t.Shifts)
                      .WithOne(s => s.ShiftType)
                      .HasForeignKey(s => s.ShiftTypeId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ShiftSchedule <-> Shifts
            modelBuilder.Entity<ShiftSchedule>(entity =>
            {
                entity.ToTable("ShiftSchedules");
                entity.HasKey(e => e.Id);
                entity.HasMany(sc => sc.Shifts)
                      .WithOne(s => s.Schedule)
                      .HasForeignKey(s => s.ScheduleId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Si ShiftSchedule tiene relación opcional a ShiftType sin colección inversa:
                entity.HasOne(sc => sc.ShiftType)
                      .WithMany()
                      .HasForeignKey(sc => sc.ShiftTypeId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Shifts
            modelBuilder.Entity<Shift>(entity =>
            {
                entity.ToTable("Shifts");
                entity.HasKey(e => e.Id);
                // Shift.ShiftState es un enum nullable; EF Core lo mapea como entero por defecto.
            });

            // Holidays (si existe)
            modelBuilder.Entity<Holidays>(entity =>
            {
                entity.ToTable("Holidays");
                entity.HasKey(e => e.Id);
            });
        }
    }
}
