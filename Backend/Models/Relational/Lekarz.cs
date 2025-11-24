using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Relational;

[Table("lekarz")]
public class Lekarz
{
    [Key]
    [Column("id_lekarza")]
    public int IdLekarza { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("imie")]
    public string Imie { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("nazwisko")]
    public string Nazwisko { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [Column("specjalizacja")]
    public string Specjalizacja { get; set; } = string.Empty;

    [Column("user_id")]
    public string? UserId { get; set; }

    // Navigation properties
    public virtual User? User { get; set; }
    public virtual ICollection<Wizyta> Wizyty { get; set; } = new List<Wizyta>();
    public virtual ICollection<TerminLekarza> Terminy { get; set; } = new List<TerminLekarza>();
}
