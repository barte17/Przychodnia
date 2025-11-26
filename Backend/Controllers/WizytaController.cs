using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateWizyta(int id, UpdateWizytaDto updateDto)
    {
        var wizyta = await _context.Wizyty.FindAsync(id);

        if (wizyta == null)
            return NotFound(new { message = "Wizyta nie została znaleziona" });

        if (updateDto.Data.HasValue)
            wizyta.Data = updateDto.Data.Value;

        if (!string.IsNullOrEmpty(updateDto.Status))
        {
            var validStatuses = new[] { "Zaplanowana", "Oczekująca", "Zaakceptowana", "Odbyta", "Odrzucona", "Anulowana" };
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
    [Authorize(Roles = "Admin")]
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

    // POST: api/wizyta/rezerwuj - pacjent rezerwuje termin
    [HttpPost("rezerwuj")]
    [Authorize(Roles = "Patient,Admin")]
    public async Task<ActionResult<WizytaDto>> RezerwujWizyte(RezerwujWizyteDto rezerwujDto)
    {
        var pacjent = await _context.Pacjenci.FindAsync(rezerwujDto.IdPacjenta);
        if (pacjent == null)
            return BadRequest(new { message = "Pacjent o podanym ID nie istnieje" });

        var termin = await _context.TerminyLekarzy
            .Include(t => t.Lekarz)
            .FirstOrDefaultAsync(t => t.IdTerminu == rezerwujDto.IdTerminu);

        if (termin == null)
            return BadRequest(new { message = "Termin o podanym ID nie istnieje" });

        if (!termin.CzyDostepny)
            return BadRequest(new { message = "Ten termin jest już zarezerwowany" });

        if (termin.DataRozpoczecia <= DateTime.Now)
            return BadRequest(new { message = "Nie można zarezerwować terminu z przeszłości" });

        // Utwórz wizytę ze statusem "Oczekująca"
        var wizyta = new Wizyta
        {
            Data = termin.DataRozpoczecia,
            Status = "Oczekująca",
            IdPacjenta = rezerwujDto.IdPacjenta,
            IdLekarza = termin.IdLekarza
        };

        // Oznacz termin jako niedostępny
        termin.CzyDostepny = false;

        _context.Wizyty.Add(wizyta);
        await _context.SaveChangesAsync();

        var createdWizyta = await _context.Wizyty
            .Include(w => w.Pacjent)
            .Include(w => w.Lekarz)
            .FirstOrDefaultAsync(w => w.IdWizyty == wizyta.IdWizyty);

        return CreatedAtAction(nameof(GetWizyta), new { id = wizyta.IdWizyty }, new WizytaDto
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
        });
    }

    // PATCH: api/wizyta/{id}/akceptuj - lekarz akceptuje wizytę
    [HttpPatch("{id}/akceptuj")]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<IActionResult> AkceptujWizyte(int id)
    {
        var wizyta = await _context.Wizyty.FindAsync(id);

        if (wizyta == null)
            return NotFound(new { message = "Wizyta nie została znaleziona" });

        if (wizyta.Status != "Oczekująca")
            return BadRequest(new { message = "Można akceptować tylko wizyty ze statusem 'Oczekująca'" });

        wizyta.Status = "Zaakceptowana";
        await _context.SaveChangesAsync();

        return Ok(new { message = "Wizyta została zaakceptowana" });
    }

    // PATCH: api/wizyta/{id}/odrzuc - lekarz odrzuca wizytę
    [HttpPatch("{id}/odrzuc")]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<IActionResult> OdrzucWizyte(int id)
    {
        var wizyta = await _context.Wizyty
            .Include(w => w.Lekarz)
            .FirstOrDefaultAsync(w => w.IdWizyty == id);

        if (wizyta == null)
            return NotFound(new { message = "Wizyta nie została znaleziona" });

        if (wizyta.Status != "Oczekująca")
            return BadRequest(new { message = "Można odrzucać tylko wizyty ze statusem 'Oczekująca'" });

        // Przywróć dostępność terminu
        var termin = await _context.TerminyLekarzy
            .FirstOrDefaultAsync(t => t.IdLekarza == wizyta.IdLekarza && t.DataRozpoczecia == wizyta.Data);

        if (termin != null)
        {
            termin.CzyDostepny = true;
        }

        wizyta.Status = "Odrzucona";
        await _context.SaveChangesAsync();

        return Ok(new { message = "Wizyta została odrzucona" });
    }

    // PATCH: api/wizyta/{id}/zakoncz - oznacz wizytę jako odbytą
    [HttpPatch("{id}/zakoncz")]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<IActionResult> ZakonczWizyte(int id)
    {
        var wizyta = await _context.Wizyty.FindAsync(id);

        if (wizyta == null)
            return NotFound(new { message = "Wizyta nie została znaleziona" });

        if (wizyta.Status != "Zaakceptowana")
            return BadRequest(new { message = "Można zakończyć tylko zaakceptowane wizyty" });

        wizyta.Status = "Odbyta";
        await _context.SaveChangesAsync();

        return Ok(new { message = "Wizyta została zakończona" });
    }

    // GET: api/wizyta/oczekujace/lekarz/{lekarzId} - wizyty oczekujące na akceptację
    [HttpGet("oczekujace/lekarz/{lekarzId}")]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<ActionResult<IEnumerable<WizytaDto>>> GetOczekujaceWizyty(int lekarzId)
    {
        var wizyty = await _context.Wizyty
            .Include(w => w.Pacjent)
            .Include(w => w.Lekarz)
            .Where(w => w.IdLekarza == lekarzId && w.Status == "Oczekująca")
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
}
