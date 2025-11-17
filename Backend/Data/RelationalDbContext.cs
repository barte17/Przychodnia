using Microsoft.EntityFrameworkCore;
using Backend.Models.Relational;

namespace Backend.Data;

public class RelationalDbContext : DbContext
{
    public RelationalDbContext(DbContextOptions<RelationalDbContext> options)
        : base(options)
    {
    }

    public DbSet<Pacjent> Pacjenci { get; set; }
    public DbSet<Lekarz> Lekarze { get; set; }
    public DbSet<Wizyta> Wizyty { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Konfiguracja relacji Wizyta - Pacjent
        modelBuilder.Entity<Wizyta>()
            .HasOne(w => w.Pacjent)
            .WithMany(p => p.Wizyty)
            .HasForeignKey(w => w.IdPacjenta)
            .OnDelete(DeleteBehavior.Cascade);

        // Konfiguracja relacji Wizyta - Lekarz
        modelBuilder.Entity<Wizyta>()
            .HasOne(w => w.Lekarz)
            .WithMany(l => l.Wizyty)
            .HasForeignKey(w => w.IdLekarza)
            .OnDelete(DeleteBehavior.Cascade);

        // Indeksy dla lepszej wydajności
        modelBuilder.Entity<Pacjent>()
            .HasIndex(p => p.PESEL)
            .IsUnique();

        modelBuilder.Entity<Wizyta>()
            .HasIndex(w => w.Data);

        modelBuilder.Entity<Wizyta>()
            .HasIndex(w => w.Status);
    }
}
