using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Backend.Models.Relational;

namespace Backend.Data;

public class RelationalDbContext : IdentityDbContext<User>
{
    public RelationalDbContext(DbContextOptions<RelationalDbContext> options)
        : base(options)
    {
    }

    public DbSet<Pacjent> Pacjenci { get; set; }
    public DbSet<Lekarz> Lekarze { get; set; }
    public DbSet<Wizyta> Wizyty { get; set; }
    public DbSet<TerminLekarza> TerminyLekarzy { get; set; }

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

        // Konfiguracja relacji User - Pacjent
        modelBuilder.Entity<Pacjent>()
            .HasOne(p => p.User)
            .WithOne(u => u.Pacjent)
            .HasForeignKey<Pacjent>(p => p.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Konfiguracja relacji User - Lekarz
        modelBuilder.Entity<Lekarz>()
            .HasOne(l => l.User)
            .WithOne(u => u.Lekarz)
            .HasForeignKey<Lekarz>(l => l.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indeksy dla lepszej wydajności
        modelBuilder.Entity<Pacjent>()
            .HasIndex(p => p.PESEL)
            .IsUnique();

        modelBuilder.Entity<Pacjent>()
            .HasIndex(p => p.UserId)
            .IsUnique();

        modelBuilder.Entity<Lekarz>()
            .HasIndex(l => l.UserId)
            .IsUnique();

        modelBuilder.Entity<Wizyta>()
            .HasIndex(w => w.Data);

        modelBuilder.Entity<Wizyta>()
            .HasIndex(w => w.Status);

        // Konfiguracja relacji TerminLekarza - Lekarz
        modelBuilder.Entity<TerminLekarza>()
            .HasOne(t => t.Lekarz)
            .WithMany(l => l.Terminy)
            .HasForeignKey(t => t.IdLekarza)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TerminLekarza>()
            .HasIndex(t => t.DataRozpoczecia);

        modelBuilder.Entity<TerminLekarza>()
            .HasIndex(t => t.CzyDostepny);
    }
}
