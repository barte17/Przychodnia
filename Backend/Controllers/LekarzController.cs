using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models.Relational;
using Backend.DTOs;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LekarzController : ControllerBase
{
    private readonly RelationalDbContext _context;

    public LekarzController(RelationalDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LekarzDto>>> GetLekarze()
    {
        var lekarze = await _context.Lekarze
            .Select(l => new LekarzDto
            {
                IdLekarza = l.IdLekarza,
                Imie = l.Imie,
                Nazwisko = l.Nazwisko,
                Specjalizacja = l.Specjalizacja
            })
            .ToListAsync();

        return Ok(lekarze);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LekarzDto>> GetLekarz(int id)
    {
        var lekarz = await _context.Lekarze.FindAsync(id);

        if (lekarz == null)
            return NotFound(new { message = "Lekarz nie został znaleziony" });

        return Ok(new LekarzDto
        {
            IdLekarza = lekarz.IdLekarza,
            Imie = lekarz.Imie,
            Nazwisko = lekarz.Nazwisko,
            Specjalizacja = lekarz.Specjalizacja
        });
    }

    [HttpGet("specjalizacja/{specjalizacja}")]
    public async Task<ActionResult<IEnumerable<LekarzDto>>> GetLekarzeBySpecjalizacja(string specjalizacja)
    {
        var lekarze = await _context.Lekarze
            .Where(l => l.Specjalizacja.ToLower().Contains(specjalizacja.ToLower()))
            .Select(l => new LekarzDto
            {
                IdLekarza = l.IdLekarza,
                Imie = l.Imie,
                Nazwisko = l.Nazwisko,
                Specjalizacja = l.Specjalizacja
            })
            .ToListAsync();

        return Ok(lekarze);
    }

    [HttpPost]
    public async Task<ActionResult<LekarzDto>> CreateLekarz(CreateLekarzDto createDto)
    {
        var lekarz = new Lekarz
        {
            Imie = createDto.Imie,
            Nazwisko = createDto.Nazwisko,
            Specjalizacja = createDto.Specjalizacja
        };

        _context.Lekarze.Add(lekarz);
        await _context.SaveChangesAsync();

        var lekarzDto = new LekarzDto
        {
            IdLekarza = lekarz.IdLekarza,
            Imie = lekarz.Imie,
            Nazwisko = lekarz.Nazwisko,
            Specjalizacja = lekarz.Specjalizacja
        };

        return CreatedAtAction(nameof(GetLekarz), new { id = lekarz.IdLekarza }, lekarzDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLekarz(int id, UpdateLekarzDto updateDto)
    {
        var lekarz = await _context.Lekarze.FindAsync(id);

        if (lekarz == null)
            return NotFound(new { message = "Lekarz nie został znaleziony" });

        if (!string.IsNullOrEmpty(updateDto.Imie))
            lekarz.Imie = updateDto.Imie;

        if (!string.IsNullOrEmpty(updateDto.Nazwisko))
            lekarz.Nazwisko = updateDto.Nazwisko;

        if (!string.IsNullOrEmpty(updateDto.Specjalizacja))
            lekarz.Specjalizacja = updateDto.Specjalizacja;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLekarz(int id)
    {
        var lekarz = await _context.Lekarze.FindAsync(id);

        if (lekarz == null)
            return NotFound(new { message = "Lekarz nie został znaleziony" });

        _context.Lekarze.Remove(lekarz);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
