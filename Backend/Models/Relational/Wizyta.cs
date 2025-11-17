using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Relational;

[Table("wizyta")]
public class Wizyta
{
    [Key]
    [Column("id_wizyty")]
    public int IdWizyty { get; set; }

    [Required]
    [Column("data")]
    public DateTime Data { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("status")]
    public string Status { get; set; } = "Zaplanowana";

    [Required]
    [Column("id_pacjenta")]
    [ForeignKey(nameof(Pacjent))]
    public int IdPacjenta { get; set; }

    [Required]
    [Column("id_lekarza")]
    [ForeignKey(nameof(Lekarz))]
    public int IdLekarza { get; set; }

    // Navigation properties
    public virtual Pacjent Pacjent { get; set; } = null!;
    public virtual Lekarz Lekarz { get; set; } = null!;
}
