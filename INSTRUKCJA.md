# Instrukcja uruchomienia projektu Przychodnia

## Wymagania
- .NET 8.0 SDK
- Dostęp do internetu (dla połączeń z bazami w chmurze)

## Kroki instalacji

### 1. Sklonuj repozytorium
```bash
git clone https://github.com/barte17/Przychodnia.git
cd Przychodnia
```

### 2. Skonfiguruj połączenia z bazami danych

Skopiuj plik przykładowy i uzupełnij swoimi danymi:
```bash
cd Backend
copy appsettings.Development.json.example appsettings.Development.json
```

Edytuj plik `appsettings.Development.json` i uzupełnij dane dostępowe do:
- **PostgreSQL (Aiven)** - dane z panelu Aiven
- **MongoDB (Atlas)** - connection string z MongoDB Atlas

### 3. Zainstaluj zależności
```bash
dotnet restore
```

### 4. Zastosuj migracje bazy danych
```bash
dotnet ef database update
```

### 5. Uruchom aplikację
```bash
dotnet run
```

Lub w trybie Development:
```powershell
$env:ASPNETCORE_ENVIRONMENT='Development'
dotnet run
```

### 6. Otwórz aplikację
- API: http://localhost:5000
- Swagger UI: http://localhost:5000/swagger

## Struktura projektu

```
Backend/
├── Controllers/          # Kontrolery API (Pacjent, Lekarz, Wizyta, Ankieta)
├── Models/
│   ├── Relational/      # Modele dla PostgreSQL
│   └── NonRelational/   # Modele dla MongoDB
├── Data/                # Konteksty baz danych
├── DTOs/                # Data Transfer Objects
├── Migrations/          # Migracje EF Core
└── appsettings.json     # Konfiguracja (bez haseł)
```

## Dostępne endpointy

### Pacjenci (PostgreSQL)
- `GET /api/pacjent` - Lista wszystkich pacjentów
- `GET /api/pacjent/{id}` - Szczegóły pacjenta
- `POST /api/pacjent` - Dodaj nowego pacjenta
- `PUT /api/pacjent/{id}` - Zaktualizuj pacjenta
- `DELETE /api/pacjent/{id}` - Usuń pacjenta

### Lekarze (PostgreSQL)
- `GET /api/lekarz` - Lista wszystkich lekarzy
- `GET /api/lekarz/{id}` - Szczegóły lekarza
- `POST /api/lekarz` - Dodaj nowego lekarza
- `PUT /api/lekarz/{id}` - Zaktualizuj lekarza
- `DELETE /api/lekarz/{id}` - Usuń lekarza

### Wizyty (PostgreSQL)
- `GET /api/wizyta` - Lista wszystkich wizyt
- `GET /api/wizyta/{id}` - Szczegóły wizyty
- `POST /api/wizyta` - Zarezerwuj wizytę
- `PUT /api/wizyta/{id}` - Zaktualizuj wizytę
- `DELETE /api/wizyta/{id}` - Anuluj wizytę

### Ankiety (MongoDB)
- `GET /api/ankieta` - Lista wszystkich ankiet
- `GET /api/ankieta/{id}` - Szczegóły ankiety
- `POST /api/ankieta` - Dodaj nową ankietę
- `PUT /api/ankieta/{id}` - Zaktualizuj ankietę
- `DELETE /api/ankieta/{id}` - Usuń ankietę

## Bazy danych

### PostgreSQL (Aiven)
Przechowuje dane relacyjne:
- `pacjent` - dane pacjentów (PESEL, imię, nazwisko)
- `lekarz` - dane lekarzy (imię, nazwisko, specjalizacja)
- `wizyta` - wizyty lekarskie (data, status, powiązania z pacjentem i lekarzem)

### MongoDB (Atlas)
Przechowuje dane nierelacyjne:
- `Ankieta` - ankiety pacjentów (typ, pytania i odpowiedzi)

## Rozwiązywanie problemów

### Błąd połączenia z PostgreSQL
- Sprawdź czy dane w `appsettings.Development.json` są poprawne
- Upewnij się, że masz dostęp do internetu
- Sprawdź firewall

### Błąd połączenia z MongoDB
- Sprawdź connection string w `appsettings.Development.json`
- Upewnij się, że Twoje IP jest dodane do whitelist w MongoDB Atlas
- Sprawdź czy hasło nie zawiera znaków specjalnych wymagających enkodowania

### Migracje nie działają
```bash
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Kontakt
Projekt: https://github.com/barte17/Przychodnia
