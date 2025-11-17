using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models.Relational;
using Backend.DTOs;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WizytaController : ControllerBase
{
    private readonly RelationalDbContext _context;

    public WizytaController(RelationalDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WizytaDto>>> GetWizyty()
    {
        var wizyty = await _context.Wizyty
            .Include(w => w.Pacjent)
            .Include(w => w.Lekarz)
            .Select(w => new WizytaDto
            {
                IdWizyty = w.IdWizyty,
                Data = w.Data,
                Status = w.Status,
                IdPacjenta = w.IdPacjenta,
                PacjentImie = w.Pacjent.Imie,
                PacjentNazwisko = w.Pacjent.Nazwisko,
                IdLekarza = w.IdLekarza,
                LekarzImie = w.Lekarz.Imie,
                LekarzNazwisko = w.Lekarz.Nazwisko,
                LekarzSpecjalizacja = w.Lekarz.Specjalizacja
            })
            .OrderBy(w => w.Data)
            .ToListAsync();

        return Ok(wizyty);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WizytaDto>> GetWizyta(int id)
    {
        var wizyta = await _context.Wizyty
            .Include(w => w.Pacjent)
            .Include(w => w.Lekarz)
            .FirstOrDefaultAsync(w => w.IdWizyty == id);

        if (wizyta == null)
            return NotFound(new { message = "Wizyta nie została znaleziona" });

        return Ok(new WizytaDto
        {
            IdWizyty = wizyta.IdWizyty,
            Data = wizyta.Data,
            Status = wizyta.Status,
            IdPacjenta = wizyta.IdPacjenta,
            PacjentImie = wizyta.Pacjent.Imie,
            PacjentNazwisko = wizyta.Pacjent.Nazwisko,
            IdLekarza = wizyta.IdLekarza,
            LekarzImie = wizyta.Lekarz.Imie,
            LekarzNazwisko = wizyta.Lekarz.Nazwisko,
            LekarzSpecjalizacja = wizyta.Lekarz.Specjalizacja
        });
    }

    [HttpGet("pacjent/{pacjentId}")]
    public async Task<ActionResult<IEnumerable<WizytaDto>>> GetWizytyByPacjent(int pacjentId)
    {
        var wizyty = await _context.Wizyty
            .Include(w => w.Pacjent)
            .Include(w => w.Lekarz)
            .Where(w => w.IdPacjenta == pacjentId)
            .Select(w => new WizytaDto
            {
                IdWizyty = w.IdWizyty,
                Data = w.Data,
                Status = w.Status,
                IdPacjenta = w.IdPacjenta,
                PacjentImie = w.Pacjent.Imie,
                PacjentNazwisko = w.Pacjent.Nazwisko,
                IdLekarza = w.IdLekarza,
                LekarzImie = w.Lekarz.Imie,
                LekarzNazwisko = w.Lekarz.Nazwisko,
                LekarzSpecjalizacja = w.Lekarz.Specjalizacja
            })
            .OrderBy(w => w.Data)
            .ToListAsync();

        return Ok(wizyty);
    }

    [HttpGet("lekarz/{lekarzId}")]
    public async Task<ActionResult<IEnumerable<WizytaDto>>> GetWizytyByLekarz(int lekarzId)
    {
        var wizyty = await _context.Wizyty
            .Include(w => w.Pacjent)
            .Include(w => w.Lekarz)
            .Where(w => w.IdLekarza == lekarzId)
            .Select(w => new WizytaDto
            {
                IdWizyty = w.IdWizyty,
                Data = w.Data,
                Status = w.Status,
                IdPacjenta = w.IdPacjenta,
                PacjentImie = w.Pacjent.Imie,
                PacjentNazwisko = w.Pacjent.Nazwisko,
                IdLekarza = w.IdLekarza,
                LekarzImie = w.Lekarz.Imie,
                LekarzNazwisko = w.Lekarz.Nazwisko,
                LekarzSpecjalizacja = w.Lekarz.Specjalizacja
            })
            .OrderBy(w => w.Data)
            .ToListAsync();

        return Ok(wizyty);
    }

    [HttpPost]
    public async Task<ActionResult<WizytaDto>> CreateWizyta(CreateWizytaDto createDto)
    {
        var pacjent = await _context.Pacjenci.FindAsync(createDto.IdPacjenta);
        if (pacjent == null)
            return BadRequest(new { message = "Pacjent o podanym ID nie istnieje" });

        var lekarz = await _context.Lekarze.FindAsync(createDto.IdLekarza);
        if (lekarz == null)
            return BadRequest(new { message = "Lekarz o podanym ID nie istnieje" });

        var isLekarzBusy = await _context.Wizyty
            .AnyAsync(w => w.IdLekarza == createDto.IdLekarza 
                && w.Data == createDto.Data 
                && w.Status != "Anulowana");

        if (isLekarzBusy)
            return BadRequest(new { message = "Lekarz jest zajęty w wybranym terminie" });

        var wizyta = new Wizyta
        {
            Data = createDto.Data,
            Status = "Zaplanowana",
            IdPacjenta = createDto.IdPacjenta,
            IdLekarza = createDto.IdLekarza
        };

        _context.Wizyty.Add(wizyta);
        await _context.SaveChangesAsync();

        var createdWizyta = await _context.Wizyty
            .Include(w => w.Pacjent)
            .Include(w => w.Lekarz)
            .FirstOrDefaultAsync(w => w.IdWizyty == wizyta.IdWizyty);

        var wizytaDto = new WizytaDto
        {
            IdWizyty = createdWizyta!.IdWizyty,
            Data = createdWizyta.Data,
            Status = createdWizyta.Status,
            IdPacjenta = createdWizyta.IdPacjenta,
            PacjentImie = createdWizyta.Pacjent.Imie,
            PacjentNazwisko = createdWizyta.Pacjent.Nazwisko,
            IdLekarza = createdWizyta.IdLekarza,
            LekarzImie = createdWizyta.Lekarz.Imie,
            LekarzNazwisko = createdWizyta.Lekarz.Nazwisko,
            LekarzSpecjalizacja = createdWizyta.Lekarz.Specjalizacja
        };

        return CreatedAtAction(nameof(GetWizyta), new { id = wizyta.IdWizyty }, wizytaDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateWizyta(int id, UpdateWizytaDto updateDto)
    {
        var wizyta = await _context.Wizyty.FindAsync(id);

        if (wizyta == null)
            return NotFound(new { message = "Wizyta nie została znaleziona" });

        if (updateDto.Data.HasValue)
            wizyta.Data = updateDto.Data.Value;

        if (!string.IsNullOrEmpty(updateDto.Status))
        {
            var validStatuses = new[] { "Zaplanowana", "Odbyta", "Anulowana" };
            if (!validStatuses.Contains(updateDto.Status))
                return BadRequest(new { message = "Nieprawidłowy status" });
            wizyta.Status = updateDto.Status;
        }

        if (updateDto.IdLekarza.HasValue)
        {
            var lekarz = await _context.Lekarze.FindAsync(updateDto.IdLekarza.Value);
            if (lekarz == null)
                return BadRequest(new { message = "Lekarz o podanym ID nie istnieje" });
            wizyta.IdLekarza = updateDto.IdLekarza.Value;
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWizyta(int id)
    {
        var wizyta = await _context.Wizyty.FindAsync(id);

        if (wizyta == null)
            return NotFound(new { message = "Wizyta nie została znaleziona" });

        _context.Wizyty.Remove(wizyta);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id}/cancel")]
    public async Task<IActionResult> CancelWizyta(int id)
    {
        var wizyta = await _context.Wizyty.FindAsync(id);

        if (wizyta == null)
            return NotFound(new { message = "Wizyta nie została znaleziona" });

        wizyta.Status = "Anulowana";
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
