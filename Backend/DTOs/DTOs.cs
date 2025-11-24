namespace Backend.DTOs;

// Pacjent DTOs
public class PacjentDto
{
    public int IdPacjenta { get; set; }
    public string Imie { get; set; } = string.Empty;
    public string Nazwisko { get; set; } = string.Empty;
    public string PESEL { get; set; } = string.Empty;
}

public class CreatePacjentDto
{
    public string Imie { get; set; } = string.Empty;
    public string Nazwisko { get; set; } = string.Empty;
    public string PESEL { get; set; } = string.Empty;
}

public class UpdatePacjentDto
{
    public string? Imie { get; set; }
    public string? Nazwisko { get; set; }
}

// Lekarz DTOs
public class LekarzDto
{
    public int IdLekarza { get; set; }
    public string Imie { get; set; } = string.Empty;
    public string Nazwisko { get; set; } = string.Empty;
    public string Specjalizacja { get; set; } = string.Empty;
}

public class CreateLekarzDto
{
    public string Imie { get; set; } = string.Empty;
    public string Nazwisko { get; set; } = string.Empty;
    public string Specjalizacja { get; set; } = string.Empty;
}

public class UpdateLekarzDto
{
    public string? Imie { get; set; }
    public string? Nazwisko { get; set; }
    public string? Specjalizacja { get; set; }
}

// Wizyta DTOs
public class WizytaDto
{
    public int IdWizyty { get; set; }
    public DateTime Data { get; set; }
    public string Status { get; set; } = string.Empty;
    public int IdPacjenta { get; set; }
    public string PacjentImie { get; set; } = string.Empty;
    public string PacjentNazwisko { get; set; } = string.Empty;
    public int IdLekarza { get; set; }
    public string LekarzImie { get; set; } = string.Empty;
    public string LekarzNazwisko { get; set; } = string.Empty;
    public string LekarzSpecjalizacja { get; set; } = string.Empty;
}

public class CreateWizytaDto
{
    public DateTime Data { get; set; }
    public int IdPacjenta { get; set; }
    public int IdLekarza { get; set; }
}

public class UpdateWizytaDto
{
    public DateTime? Data { get; set; }
    public string? Status { get; set; }
    public int? IdLekarza { get; set; }
}

// Ankieta DTOs
public class AnkietaDto
{
    public string? Id { get; set; }
    public int IdAnkiety { get; set; }
    public int? IdPacjenta { get; set; }
    public int? IdWizyty { get; set; }
    public int? IdLekarza { get; set; }
    public string? NazwaLekarza { get; set; }
    public string? PESEL { get; set; }
    public bool CzyAnonimowa { get; set; }
    public DateTime DataWypelnienia { get; set; }
    public DateTime? DataWizyty { get; set; }
    public string TypAnkiety { get; set; } = string.Empty;
    public int? OcenaWizyty { get; set; }
    public List<OdpowiedzAnkietyDto> Odpowiedzi { get; set; } = new();
    public string? DodatkoweUwagi { get; set; }
    public DateTime DataUtworzenia { get; set; }
    public DateTime OstatniaModyfikacja { get; set; }
}

public class OdpowiedzAnkietyDto
{
    public string Pytanie { get; set; } = string.Empty;
    public string Odpowiedz { get; set; } = string.Empty;
    public string? Kategoria { get; set; }
}

public class CreateAnkietaDto
{
    public int? IdPacjenta { get; set; }
    public string? PESEL { get; set; }
    public DateTime? DataWypelnienia { get; set; }
    public string TypAnkiety { get; set; } = "Ogólna";
    public List<OdpowiedzAnkietyDto>? Odpowiedzi { get; set; }
    public string? DodatkoweUwagi { get; set; }
}

public class UpdateAnkietaDto
{
    public List<OdpowiedzAnkietyDto>? Odpowiedzi { get; set; }
    public string? DodatkoweUwagi { get; set; }
}

// DTO dla anonimowej ankiety o wizycie
public class CreateAnkietaAnonimowaDto
{
    public int IdWizyty { get; set; }
    public int OcenaWizyty { get; set; } // 1-5
    public List<OdpowiedzAnkietyDto>? Odpowiedzi { get; set; }
    public string? DodatkoweUwagi { get; set; }
}

// Authentication DTOs
public class RegisterDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = "Patient"; // Patient, Doctor, Admin
    public string? PESEL { get; set; } // Required for patients
    public string? Specjalizacja { get; set; } // Required for doctors
}

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime Expires { get; set; }
}

public class UserInfoDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public PacjentDto? Pacjent { get; set; }
    public LekarzDto? Lekarz { get; set; }
}

// Admin User Management DTOs
public class UserListItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? PESEL { get; set; } // For patients
    public string? Specjalizacja { get; set; } // For doctors
}

public class UpdateUserRoleDto
{
    public string UserId { get; set; } = string.Empty;
    public string NewRole { get; set; } = string.Empty;
    public string? PESEL { get; set; } // Required when changing to Patient
    public string? Specjalizacja { get; set; } // Required when changing to Doctor
}

public class CreateUserByAdminDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = "Patient";
    public string? PESEL { get; set; }
    public string? Specjalizacja { get; set; }
}

public class UpdateUserByAdminDto
{
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Role { get; set; }
    public string? PESEL { get; set; }
    public string? Specjalizacja { get; set; }
}

// TerminLekarza DTOs
public class TerminLekarzaDto
{
    public int IdTerminu { get; set; }
    public DateTime DataRozpoczecia { get; set; }
    public DateTime DataZakonczenia { get; set; }
    public bool CzyDostepny { get; set; }
    public int IdLekarza { get; set; }
    public string LekarzImie { get; set; } = string.Empty;
    public string LekarzNazwisko { get; set; } = string.Empty;
    public string LekarzSpecjalizacja { get; set; } = string.Empty;
}

public class CreateTerminDto
{
    public DateTime DataRozpoczecia { get; set; }
    public DateTime DataZakonczenia { get; set; }
    public int IdLekarza { get; set; }
}

public class CreateTerminyBatchDto
{
    public int IdLekarza { get; set; }
    public DateTime DataOd { get; set; }
    public DateTime DataDo { get; set; }
    public int CzasTrwaniaMinuty { get; set; } = 30; // domyślnie 30 minut
}

// Rezerwacja wizyty przez pacjenta
public class RezerwujWizyteDto
{
    public int IdTerminu { get; set; }
    public int IdPacjenta { get; set; }
}

// Rozszerzone Ankieta DTOs
public class CreateAnkietaOcenaWizytyDto
{
    public int IdPacjenta { get; set; }
    public int IdWizyty { get; set; }
    public int IdLekarza { get; set; }
    public string PESEL { get; set; } = string.Empty;
    public int OcenaWizyty { get; set; } // 1-5
    public List<OdpowiedzAnkietyDto>? Odpowiedzi { get; set; }
    public string? DodatkoweUwagi { get; set; }
}
