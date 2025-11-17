using Microsoft.AspNetCore.Mvc;
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
    private readonly ILogger<AnkietaController> _logger;

    public AnkietaController(MongoDbContext mongoContext, ILogger<AnkietaController> logger)
    {
        _mongoContext = mongoContext;
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
            PESEL = ankieta.PESEL,
            DataWypelnienia = ankieta.DataWypelnienia,
            TypAnkiety = ankieta.TypAnkiety,
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
}
