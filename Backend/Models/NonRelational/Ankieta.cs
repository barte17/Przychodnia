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
    [BsonRequired]
    public int IdPacjenta { get; set; }

    [BsonElement("pesel")]
    [BsonRequired]
    public string PESEL { get; set; } = string.Empty;

    [BsonElement("data_wypelnienia")]
    public DateTime DataWypelnienia { get; set; } = DateTime.UtcNow;

    [BsonElement("typ_ankiety")]
    public string TypAnkiety { get; set; } = "Ogólna";

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
