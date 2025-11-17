using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models.Relational;
using Backend.DTOs;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PacjentController : ControllerBase
{
    private readonly RelationalDbContext _context;

    public PacjentController(RelationalDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PacjentDto>>> GetPacjenci()
    {
        var pacjenci = await _context.Pacjenci
            .Select(p => new PacjentDto
            {
                IdPacjenta = p.IdPacjenta,
                Imie = p.Imie,
                Nazwisko = p.Nazwisko,
                PESEL = p.PESEL
            })
            .ToListAsync();

        return Ok(pacjenci);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PacjentDto>> GetPacjent(int id)
    {
        var pacjent = await _context.Pacjenci.FindAsync(id);

        if (pacjent == null)
            return NotFound(new { message = "Pacjent nie został znaleziony" });

        return Ok(new PacjentDto
        {
            IdPacjenta = pacjent.IdPacjenta,
            Imie = pacjent.Imie,
            Nazwisko = pacjent.Nazwisko,
            PESEL = pacjent.PESEL
        });
    }

    [HttpGet("pesel/{pesel}")]
    public async Task<ActionResult<PacjentDto>> GetPacjentByPesel(string pesel)
    {
        var pacjent = await _context.Pacjenci.FirstOrDefaultAsync(p => p.PESEL == pesel);

        if (pacjent == null)
            return NotFound(new { message = "Pacjent o podanym PESEL nie został znaleziony" });

        return Ok(new PacjentDto
        {
            IdPacjenta = pacjent.IdPacjenta,
            Imie = pacjent.Imie,
            Nazwisko = pacjent.Nazwisko,
            PESEL = pacjent.PESEL
        });
    }

    [HttpPost]
    public async Task<ActionResult<PacjentDto>> CreatePacjent(CreatePacjentDto createDto)
    {
        if (await _context.Pacjenci.AnyAsync(p => p.PESEL == createDto.PESEL))
            return BadRequest(new { message = "Pacjent o podanym PESEL już istnieje" });

        var pacjent = new Pacjent
        {
            Imie = createDto.Imie,
            Nazwisko = createDto.Nazwisko,
            PESEL = createDto.PESEL
        };

        _context.Pacjenci.Add(pacjent);
        await _context.SaveChangesAsync();

        var pacjentDto = new PacjentDto
        {
            IdPacjenta = pacjent.IdPacjenta,
            Imie = pacjent.Imie,
            Nazwisko = pacjent.Nazwisko,
            PESEL = pacjent.PESEL
        };

        return CreatedAtAction(nameof(GetPacjent), new { id = pacjent.IdPacjenta }, pacjentDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePacjent(int id, UpdatePacjentDto updateDto)
    {
        var pacjent = await _context.Pacjenci.FindAsync(id);

        if (pacjent == null)
            return NotFound(new { message = "Pacjent nie został znaleziony" });

        if (!string.IsNullOrEmpty(updateDto.Imie))
            pacjent.Imie = updateDto.Imie;

        if (!string.IsNullOrEmpty(updateDto.Nazwisko))
            pacjent.Nazwisko = updateDto.Nazwisko;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePacjent(int id)
    {
        var pacjent = await _context.Pacjenci.FindAsync(id);

        if (pacjent == null)
            return NotFound(new { message = "Pacjent nie został znaleziony" });

        _context.Pacjenci.Remove(pacjent);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
