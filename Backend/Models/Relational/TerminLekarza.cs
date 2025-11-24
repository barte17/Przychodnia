using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Relational;

[Table("termin_lekarza")]
public class TerminLekarza
{
    [Key]
    [Column("id_terminu")]
    public int IdTerminu { get; set; }

    [Required]
    [Column("data_rozpoczecia")]
    public DateTime DataRozpoczecia { get; set; }

    [Required]
    [Column("data_zakonczenia")]
    public DateTime DataZakonczenia { get; set; }

    [Required]
    [Column("czy_dostepny")]
    public bool CzyDostepny { get; set; } = true;

    [Required]
    [Column("id_lekarza")]
    [ForeignKey(nameof(Lekarz))]
    public int IdLekarza { get; set; }

    // Navigation properties
    public virtual Lekarz Lekarz { get; set; } = null!;
}
