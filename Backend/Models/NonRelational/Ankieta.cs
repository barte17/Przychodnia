using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend.Models.NonRelational;

public class Ankieta
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("id_ankiety")]
    public int IdAnkiety { get; set; }

    [BsonElement("id_pacjenta")]
    public int? IdPacjenta { get; set; } // Opcjonalne dla anonimowych ankiet

    [BsonElement("id_wizyty")]
    public int? IdWizyty { get; set; }

    [BsonElement("id_lekarza")]
    public int? IdLekarza { get; set; }

    [BsonElement("nazwa_lekarza")]
    public string? NazwaLekarza { get; set; } // Przechowujemy dla historii

    [BsonElement("pesel")]
    public string? PESEL { get; set; } // Opcjonalne dla anonimowych ankiet

    [BsonElement("czy_anonimowa")]
    public bool CzyAnonimowa { get; set; } = true;

    [BsonElement("data_wypelnienia")]
    public DateTime DataWypelnienia { get; set; } = DateTime.UtcNow;

    [BsonElement("data_wizyty")]
    public DateTime? DataWizyty { get; set; }

    [BsonElement("typ_ankiety")]
    public string TypAnkiety { get; set; } = "OcenaWizyty";

    [BsonElement("ocena_wizyty")]
    public int? OcenaWizyty { get; set; } // 1-5 gwiazdek

    [BsonElement("odpowiedzi")]
    public List<OdpowiedzAnkiety> Odpowiedzi { get; set; } = new();

    [BsonElement("dodatkowe_uwagi")]
    public string? DodatkoweUwagi { get; set; }

    [BsonElement("data_utworzenia")]
    public DateTime DataUtworzenia { get; set; } = DateTime.UtcNow;

    [BsonElement("ostatnia_modyfikacja")]
    public DateTime OstatniaModyfikacja { get; set; } = DateTime.UtcNow;
}

public class OdpowiedzAnkiety
{
    [BsonElement("pytanie")]
    public string Pytanie { get; set; } = string.Empty;

    [BsonElement("odpowiedz")]
    public string Odpowiedz { get; set; } = string.Empty;

    [BsonElement("kategoria")]
    public string? Kategoria { get; set; }
}
