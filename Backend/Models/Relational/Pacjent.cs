using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Relational;

[Table("pacjent")]
public class Pacjent
{
    [Key]
    [Column("id_pacjenta")]
    public int IdPacjenta { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("imie")]
    public string Imie { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("nazwisko")]
    public string Nazwisko { get; set; } = string.Empty;

    [Required]
    [MaxLength(11)]
    [Column("pesel")]
    public string PESEL { get; set; } = string.Empty;

    [Column("user_id")]
    public string? UserId { get; set; }

    // Navigation properties
    public virtual User? User { get; set; }
    public virtual ICollection<Wizyta> Wizyty { get; set; } = new List<Wizyta>();
}
