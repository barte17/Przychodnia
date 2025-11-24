using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Backend.Data;
using Backend.Models.NonRelational;
using Backend.DTOs;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnkietaController : ControllerBase
{
    private readonly MongoDbContext _mongoContext;
    private readonly RelationalDbContext _relationalContext;
    private readonly ILogger<AnkietaController> _logger;

    public AnkietaController(MongoDbContext mongoContext, RelationalDbContext relationalContext, ILogger<AnkietaController> logger)
    {
        _mongoContext = mongoContext;
        _relationalContext = relationalContext;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AnkietaDto>>> GetAnkiety()
    {
        try
        {
            var ankiety = await _mongoContext.Ankiety
                .Find(_ => true)
                .SortByDescending(a => a.DataWypelnienia)
                .ToListAsync();

            var ankietyDto = ankiety.Select(a => MapToDto(a)).ToList();
            return Ok(ankietyDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania ankiet");
            return StatusCode(500, new { message = "Błąd serwera" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AnkietaDto>> GetAnkieta(string id)
    {
        try
        {
            var ankieta = await _mongoContext.Ankiety
                .Find(a => a.Id == id)
                .FirstOrDefaultAsync();

            if (ankieta == null)
                return NotFound(new { message = "Ankieta nie została znaleziona" });

            return Ok(MapToDto(ankieta));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania ankiety");
            return StatusCode(500, new { message = "Błąd serwera" });
        }
    }

    [HttpGet("pacjent/{pacjentId}")]
    public async Task<ActionResult<IEnumerable<AnkietaDto>>> GetAnkietyByPacjent(int pacjentId)
    {
        try
        {
            var ankiety = await _mongoContext.Ankiety
                .Find(a => a.IdPacjenta == pacjentId)
                .SortByDescending(a => a.DataWypelnienia)
                .ToListAsync();

            var ankietyDto = ankiety.Select(a => MapToDto(a)).ToList();
            return Ok(ankietyDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania ankiet pacjenta");
            return StatusCode(500, new { message = "Błąd serwera" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<AnkietaDto>> CreateAnkieta(CreateAnkietaDto createDto)
    {
        try
        {
            var maxIdAnkiety = await _mongoContext.Ankiety
                .Find(_ => true)
                .SortByDescending(a => a.IdAnkiety)
                .Limit(1)
                .Project(a => a.IdAnkiety)
                .FirstOrDefaultAsync();

            var ankieta = new Ankieta
            {
                IdAnkiety = maxIdAnkiety + 1,
                IdPacjenta = createDto.IdPacjenta,
                PESEL = createDto.PESEL,
                DataWypelnienia = createDto.DataWypelnienia ?? DateTime.UtcNow,
                TypAnkiety = createDto.TypAnkiety,
                Odpowiedzi = createDto.Odpowiedzi?.Select(o => new OdpowiedzAnkiety
                {
                    Pytanie = o.Pytanie,
                    Odpowiedz = o.Odpowiedz,
                    Kategoria = o.Kategoria
                }).ToList() ?? new List<OdpowiedzAnkiety>(),
                DodatkoweUwagi = createDto.DodatkoweUwagi,
                DataUtworzenia = DateTime.UtcNow,
                OstatniaModyfikacja = DateTime.UtcNow
            };

            await _mongoContext.Ankiety.InsertOneAsync(ankieta);

            return CreatedAtAction(nameof(GetAnkieta), new { id = ankieta.Id }, MapToDto(ankieta));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas tworzenia ankiety");
            return StatusCode(500, new { message = "Błąd serwera" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAnkieta(string id, UpdateAnkietaDto updateDto)
    {
        try
        {
            var ankieta = await _mongoContext.Ankiety
                .Find(a => a.Id == id)
                .FirstOrDefaultAsync();

            if (ankieta == null)
                return NotFound(new { message = "Ankieta nie została znaleziona" });

            if (updateDto.Odpowiedzi != null)
            {
                ankieta.Odpowiedzi = updateDto.Odpowiedzi.Select(o => new OdpowiedzAnkiety
                {
                    Pytanie = o.Pytanie,
                    Odpowiedz = o.Odpowiedz,
                    Kategoria = o.Kategoria
                }).ToList();
            }

            if (updateDto.DodatkoweUwagi != null)
                ankieta.DodatkoweUwagi = updateDto.DodatkoweUwagi;

            ankieta.OstatniaModyfikacja = DateTime.UtcNow;

            await _mongoContext.Ankiety.ReplaceOneAsync(a => a.Id == id, ankieta);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas aktualizacji ankiety");
            return StatusCode(500, new { message = "Błąd serwera" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAnkieta(string id)
    {
        try
        {
            var result = await _mongoContext.Ankiety.DeleteOneAsync(a => a.Id == id);

            if (result.DeletedCount == 0)
                return NotFound(new { message = "Ankieta nie została znaleziona" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas usuwania ankiety");
            return StatusCode(500, new { message = "Błąd serwera" });
        }
    }

    private static AnkietaDto MapToDto(Ankieta ankieta)
    {
        return new AnkietaDto
        {
            Id = ankieta.Id,
            IdAnkiety = ankieta.IdAnkiety,
            IdPacjenta = ankieta.IdPacjenta,
            IdWizyty = ankieta.IdWizyty,
            IdLekarza = ankieta.IdLekarza,
            NazwaLekarza = ankieta.NazwaLekarza,
            PESEL = ankieta.PESEL,
            CzyAnonimowa = ankieta.CzyAnonimowa,
            DataWypelnienia = ankieta.DataWypelnienia,
            DataWizyty = ankieta.DataWizyty,
            TypAnkiety = ankieta.TypAnkiety,
            OcenaWizyty = ankieta.OcenaWizyty,
            Odpowiedzi = ankieta.Odpowiedzi.Select(o => new OdpowiedzAnkietyDto
            {
                Pytanie = o.Pytanie,
                Odpowiedz = o.Odpowiedz,
                Kategoria = o.Kategoria
            }).ToList(),
            DodatkoweUwagi = ankieta.DodatkoweUwagi,
            DataUtworzenia = ankieta.DataUtworzenia,
            OstatniaModyfikacja = ankieta.OstatniaModyfikacja
        };
    }

    // POST: api/ankieta/ocena-wizyty - pacjent ocenia wizytę
    [HttpPost("ocena-wizyty")]
    [Authorize(Roles = "Patient,Admin")]
    public async Task<ActionResult<AnkietaDto>> CreateOcenaWizyty(CreateAnkietaOcenaWizytyDto createDto)
    {
        try
        {
            // Sprawdź czy wizyta już była oceniona
            var istniejacaOcena = await _mongoContext.Ankiety
                .Find(a => a.IdWizyty == createDto.IdWizyty)
                .FirstOrDefaultAsync();

            if (istniejacaOcena != null)
                return BadRequest(new { message = "Ta wizyta została już oceniona" });

            if (createDto.OcenaWizyty < 1 || createDto.OcenaWizyty > 5)
                return BadRequest(new { message = "Ocena musi być w skali 1-5" });

            var maxIdAnkiety = await _mongoContext.Ankiety
                .Find(_ => true)
                .SortByDescending(a => a.IdAnkiety)
                .Limit(1)
                .Project(a => a.IdAnkiety)
                .FirstOrDefaultAsync();

            var ankieta = new Ankieta
            {
                IdAnkiety = maxIdAnkiety + 1,
                IdPacjenta = createDto.IdPacjenta,
                IdWizyty = createDto.IdWizyty,
                IdLekarza = createDto.IdLekarza,
                PESEL = createDto.PESEL,
                DataWypelnienia = DateTime.UtcNow,
                TypAnkiety = "Ocena wizyty",
                OcenaWizyty = createDto.OcenaWizyty,
                Odpowiedzi = createDto.Odpowiedzi?.Select(o => new OdpowiedzAnkiety
                {
                    Pytanie = o.Pytanie,
                    Odpowiedz = o.Odpowiedz,
                    Kategoria = o.Kategoria
                }).ToList() ?? new List<OdpowiedzAnkiety>(),
                DodatkoweUwagi = createDto.DodatkoweUwagi,
                DataUtworzenia = DateTime.UtcNow,
                OstatniaModyfikacja = DateTime.UtcNow
            };

            await _mongoContext.Ankiety.InsertOneAsync(ankieta);

            return CreatedAtAction(nameof(GetAnkieta), new { id = ankieta.Id }, MapToDto(ankieta));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas tworzenia oceny wizyty");
            return StatusCode(500, new { message = "Błąd serwera" });
        }
    }

    // GET: api/ankieta/wizyta/{wizytaId} - sprawdź czy wizyta była oceniona
    [HttpGet("wizyta/{wizytaId}")]
    public async Task<ActionResult<AnkietaDto>> GetAnkietaByWizyta(int wizytaId)
    {
        try
        {
            var ankieta = await _mongoContext.Ankiety
                .Find(a => a.IdWizyty == wizytaId)
                .FirstOrDefaultAsync();

            if (ankieta == null)
                return NotFound(new { message = "Brak oceny dla tej wizyty" });

            return Ok(MapToDto(ankieta));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania ankiety wizyty");
            return StatusCode(500, new { message = "Błąd serwera" });
        }
    }

    // GET: api/ankieta/lekarz/{lekarzId} - oceny lekarza
    [HttpGet("lekarz/{lekarzId}")]
    public async Task<ActionResult<object>> GetOcenyLekarza(int lekarzId)
    {
        try
        {
            var ankiety = await _mongoContext.Ankiety
                .Find(a => a.IdLekarza == lekarzId && a.OcenaWizyty != null)
                .ToListAsync();

            var sredniaOcena = ankiety.Any() ? ankiety.Average(a => a.OcenaWizyty ?? 0) : 0;
            var liczbaOcen = ankiety.Count;

            return Ok(new
            {
                IdLekarza = lekarzId,
                SredniaOcena = Math.Round(sredniaOcena, 2),
                LiczbaOcen = liczbaOcen,
                Oceny = ankiety.Select(a => MapToDto(a)).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania ocen lekarza");
            return StatusCode(500, new { message = "Błąd serwera" });
        }
    }

    // POST: api/ankieta/anonimowa - anonimowa ankieta o wizycie
    [HttpPost("anonimowa")]
    [Authorize(Roles = "Patient,Admin")]
    public async Task<ActionResult<AnkietaDto>> CreateAnkietaAnonimowa(CreateAnkietaAnonimowaDto createDto)
    {
        try
        {
            // Sprawdź czy wizyta istnieje
            var wizyta = await _relationalContext.Wizyty
                .Include(w => w.Lekarz)
                .FirstOrDefaultAsync(w => w.IdWizyty == createDto.IdWizyty);

            if (wizyta == null)
                return NotFound(new { message = "Wizyta nie została znaleziona" });

            // Sprawdź czy wizyta jest odbyta
            if (wizyta.Status != "Odbyta")
                return BadRequest(new { message = "Można wypełnić ankietę tylko dla odbytych wizyt" });

            // Sprawdź czy wizyta już była oceniona
            var istniejacaOcena = await _mongoContext.Ankiety
                .Find(a => a.IdWizyty == createDto.IdWizyty)
                .FirstOrDefaultAsync();

            if (istniejacaOcena != null)
                return BadRequest(new { message = "Ta wizyta została już oceniona" });

            if (createDto.OcenaWizyty < 1 || createDto.OcenaWizyty > 5)
                return BadRequest(new { message = "Ocena musi być w skali 1-5" });

            var maxIdAnkiety = await _mongoContext.Ankiety
                .Find(_ => true)
                .SortByDescending(a => a.IdAnkiety)
                .Limit(1)
                .Project(a => a.IdAnkiety)
                .FirstOrDefaultAsync();

            var ankieta = new Ankieta
            {
                IdAnkiety = maxIdAnkiety + 1,
                IdPacjenta = null, // Anonimowa - nie przechowujemy ID pacjenta
                IdWizyty = createDto.IdWizyty,
                IdLekarza = wizyta.IdLekarza,
                NazwaLekarza = $"{wizyta.Lekarz.Imie} {wizyta.Lekarz.Nazwisko}",
                PESEL = null, // Anonimowa - nie przechowujemy PESEL
                CzyAnonimowa = true,
                DataWypelnienia = DateTime.UtcNow,
                DataWizyty = wizyta.Data,
                TypAnkiety = "Ocena wizyty",
                OcenaWizyty = createDto.OcenaWizyty,
                Odpowiedzi = createDto.Odpowiedzi?.Select(o => new OdpowiedzAnkiety
                {
                    Pytanie = o.Pytanie,
                    Odpowiedz = o.Odpowiedz,
                    Kategoria = o.Kategoria
                }).ToList() ?? new List<OdpowiedzAnkiety>(),
                DodatkoweUwagi = createDto.DodatkoweUwagi,
                DataUtworzenia = DateTime.UtcNow,
                OstatniaModyfikacja = DateTime.UtcNow
            };

            await _mongoContext.Ankiety.InsertOneAsync(ankieta);

            _logger.LogInformation("Utworzono anonimową ankietę dla wizyty {IdWizyty}", createDto.IdWizyty);

            return CreatedAtAction(nameof(GetAnkieta), new { id = ankieta.Id }, MapToDto(ankieta));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas tworzenia anonimowej ankiety");
            return StatusCode(500, new { message = "Błąd serwera" });
        }
    }

    // GET: api/ankieta/czy-oceniona/{wizytaId} - sprawdź czy wizyta była oceniona (true/false)
    [HttpGet("czy-oceniona/{wizytaId}")]
    public async Task<ActionResult<object>> CzyWizytaOceniona(int wizytaId)
    {
        try
        {
            var ankieta = await _mongoContext.Ankiety
                .Find(a => a.IdWizyty == wizytaId)
                .FirstOrDefaultAsync();

            return Ok(new { 
                wizytaId = wizytaId,
                czyOceniona = ankieta != null,
                ocena = ankieta?.OcenaWizyty
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas sprawdzania oceny wizyty");
            return StatusCode(500, new { message = "Błąd serwera" });
        }
    }
}
