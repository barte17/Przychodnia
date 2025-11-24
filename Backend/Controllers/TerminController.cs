using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models.Relational;
using Backend.DTOs;
using System.Security.Claims;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TerminController : ControllerBase
{
    private readonly RelationalDbContext _context;

    public TerminController(RelationalDbContext context)
    {
        _context = context;
    }

    // GET: api/termin - wszystkie dostępne terminy
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TerminLekarzaDto>>> GetTerminy()
    {
        var now = DateTime.UtcNow;
        
        var terminy = await _context.TerminyLekarzy
            .Include(t => t.Lekarz)
            .Where(t => t.CzyDostepny && t.DataRozpoczecia > now)
            .Select(t => new TerminLekarzaDto
            {
                IdTerminu = t.IdTerminu,
                DataRozpoczecia = t.DataRozpoczecia,
                DataZakonczenia = t.DataZakonczenia,
                CzyDostepny = t.CzyDostepny,
                IdLekarza = t.IdLekarza,
                LekarzImie = t.Lekarz.Imie,
                LekarzNazwisko = t.Lekarz.Nazwisko,
                LekarzSpecjalizacja = t.Lekarz.Specjalizacja
            })
            .OrderBy(t => t.DataRozpoczecia)
            .ToListAsync();

        return Ok(terminy);
    }

    // GET: api/termin/lekarz/{lekarzId} - terminy konkretnego lekarza
    [HttpGet("lekarz/{lekarzId}")]
    public async Task<ActionResult<IEnumerable<TerminLekarzaDto>>> GetTerminyByLekarz(int lekarzId, [FromQuery] bool tylkoDostepne = true)
    {
        var now = DateTime.UtcNow;
        
        var terminy = await _context.TerminyLekarzy
            .Include(t => t.Lekarz)
            .Where(t => t.IdLekarza == lekarzId)
            .Where(t => !tylkoDostepne || (t.CzyDostepny && t.DataRozpoczecia > now))
            .Select(t => new TerminLekarzaDto
            {
                IdTerminu = t.IdTerminu,
                DataRozpoczecia = t.DataRozpoczecia,
                DataZakonczenia = t.DataZakonczenia,
                CzyDostepny = t.CzyDostepny,
                IdLekarza = t.IdLekarza,
                LekarzImie = t.Lekarz.Imie,
                LekarzNazwisko = t.Lekarz.Nazwisko,
                LekarzSpecjalizacja = t.Lekarz.Specjalizacja
            })
            .OrderBy(t => t.DataRozpoczecia)
            .ToListAsync();

        return Ok(terminy);
    }

    // POST: api/termin - lekarz dodaje pojedynczy termin
    [HttpPost]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<ActionResult<TerminLekarzaDto>> CreateTermin(CreateTerminDto createDto)
    {
        var lekarz = await _context.Lekarze.FindAsync(createDto.IdLekarza);
        if (lekarz == null)
            return BadRequest(new { message = "Lekarz o podanym ID nie istnieje" });

        // Sprawdź czy termin nie nachodzi na inny
        var konflikt = await _context.TerminyLekarzy
            .AnyAsync(t => t.IdLekarza == createDto.IdLekarza &&
                ((createDto.DataRozpoczecia >= t.DataRozpoczecia && createDto.DataRozpoczecia < t.DataZakonczenia) ||
                 (createDto.DataZakonczenia > t.DataRozpoczecia && createDto.DataZakonczenia <= t.DataZakonczenia)));

        if (konflikt)
            return BadRequest(new { message = "Termin nachodzi na inny istniejący termin" });

        var termin = new TerminLekarza
        {
            DataRozpoczecia = createDto.DataRozpoczecia,
            DataZakonczenia = createDto.DataZakonczenia,
            CzyDostepny = true,
            IdLekarza = createDto.IdLekarza
        };

        _context.TerminyLekarzy.Add(termin);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTerminyByLekarz), new { lekarzId = termin.IdLekarza }, new TerminLekarzaDto
        {
            IdTerminu = termin.IdTerminu,
            DataRozpoczecia = termin.DataRozpoczecia,
            DataZakonczenia = termin.DataZakonczenia,
            CzyDostepny = termin.CzyDostepny,
            IdLekarza = termin.IdLekarza,
            LekarzImie = lekarz.Imie,
            LekarzNazwisko = lekarz.Nazwisko,
            LekarzSpecjalizacja = lekarz.Specjalizacja
        });
    }

    // POST: api/termin/batch - lekarz dodaje wiele terminów naraz
    [HttpPost("batch")]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<ActionResult<IEnumerable<TerminLekarzaDto>>> CreateTerminyBatch(CreateTerminyBatchDto createDto)
    {
        var lekarz = await _context.Lekarze.FindAsync(createDto.IdLekarza);
        if (lekarz == null)
            return BadRequest(new { message = "Lekarz o podanym ID nie istnieje" });

        var terminy = new List<TerminLekarza>();
        var currentTime = createDto.DataOd;

        while (currentTime.AddMinutes(createDto.CzasTrwaniaMinuty) <= createDto.DataDo)
        {
            var endTime = currentTime.AddMinutes(createDto.CzasTrwaniaMinuty);

            // Sprawdź czy nie ma konfliktu
            var konflikt = await _context.TerminyLekarzy
                .AnyAsync(t => t.IdLekarza == createDto.IdLekarza &&
                    ((currentTime >= t.DataRozpoczecia && currentTime < t.DataZakonczenia) ||
                     (endTime > t.DataRozpoczecia && endTime <= t.DataZakonczenia)));

            if (!konflikt)
            {
                terminy.Add(new TerminLekarza
                {
                    DataRozpoczecia = currentTime,
                    DataZakonczenia = endTime,
                    CzyDostepny = true,
                    IdLekarza = createDto.IdLekarza
                });
            }

            currentTime = endTime;
        }

        _context.TerminyLekarzy.AddRange(terminy);
        await _context.SaveChangesAsync();

        var result = terminy.Select(t => new TerminLekarzaDto
        {
            IdTerminu = t.IdTerminu,
            DataRozpoczecia = t.DataRozpoczecia,
            DataZakonczenia = t.DataZakonczenia,
            CzyDostepny = t.CzyDostepny,
            IdLekarza = t.IdLekarza,
            LekarzImie = lekarz.Imie,
            LekarzNazwisko = lekarz.Nazwisko,
            LekarzSpecjalizacja = lekarz.Specjalizacja
        });

        return Ok(new { message = $"Utworzono {terminy.Count} terminów", terminy = result });
    }

    // DELETE: api/termin/{id} - usuń termin
    [HttpDelete("{id}")]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<IActionResult> DeleteTermin(int id)
    {
        var termin = await _context.TerminyLekarzy.FindAsync(id);
        if (termin == null)
            return NotFound(new { message = "Termin nie został znaleziony" });

        if (!termin.CzyDostepny)
            return BadRequest(new { message = "Nie można usunąć zarezerwowanego terminu" });

        _context.TerminyLekarzy.Remove(termin);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
