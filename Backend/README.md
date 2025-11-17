# System Rezerwacji Wizyt - Przychodnia

System rezerwacji wizyt lekarskich z rozproszonym systemem bazodanowym wykorzystujący PostgreSQL (relacyjny) i MongoDB (nierelacyjny).

## 🏗️ Architektura

### Backend
- **.NET 8 Web API** - główny serwis REST API
- **PostgreSQL** - baza relacyjna (Pacjent, Wizyta, Lekarz)
- **MongoDB** - baza nierelacyjna (Ankiety pacjentów)
- **Entity Framework Core** - ORM dla obu baz danych

### Frontend
- **React** - framework UI
- **TypeScript** - typowanie
- **Vite** - build tool

## 📦 Struktura Bazy Danych

### PostgreSQL (Relacyjna)
- **Pacjent** (id_pacjenta, imie, nazwisko, pesel)
- **Lekarz** (id_lekarza, imie, nazwisko, specjalizacja)
- **Wizyta** (id_wizyty, data, status, id_pacjenta, id_lekarza)

### MongoDB (Nierelacyjna)
- **Ankieta** (id_ankiety, id_pacjenta, pesel, typ_ankiety, odpowiedzi, data_wypelnienia)
  - **OdpowiedzAnkiety**: pytanie, odpowiedź, kategoria
  - Typy ankiet: Ogólna, Przedoperacyjna, Kontrolna, itp.

## 🚀 Uruchomienie

### 1. Uruchomienie baz danych (Docker)

```bash
# Uruchom PostgreSQL i MongoDB
docker-compose up -d

# Sprawdź czy kontenery działają
docker-compose ps
```

**Dostępne usługi:**
- PostgreSQL: `localhost:5432`
- MongoDB: `localhost:27017`
- pgAdmin: `http://localhost:5050` (admin@przychodnia.pl / admin)
- Mongo Express: `http://localhost:8081`

### 2. Uruchomienie Backendu

```bash
cd Backend

# Zainstaluj narzędzie EF Core (jeśli nie masz)
dotnet tool install --global dotnet-ef

# Dodaj migrację
dotnet ef migrations add InitialCreate

# Zastosuj migrację (utwórz tabele w PostgreSQL)
dotnet ef database update

# Uruchom API
dotnet run
```

Backend będzie dostępny pod:
- API: `https://localhost:7xxx` (port może się różnić)
- Swagger UI: `https://localhost:7xxx/swagger`

### 3. Uruchomienie Frontendu

```bash
cd frontend

# Zainstaluj zależności
npm install

# Uruchom serwer deweloperski
npm run dev
```

Frontend będzie dostępny pod: `http://localhost:5173`

## 📡 Endpointy API

### Pacjent
- `GET /api/Pacjent` - Lista wszystkich pacjentów
- `GET /api/Pacjent/{id}` - Szczegóły pacjenta
- `GET /api/Pacjent/pesel/{pesel}` - Pacjent po PESEL
- `POST /api/Pacjent` - Dodaj pacjenta
- `PUT /api/Pacjent/{id}` - Aktualizuj pacjenta
- `DELETE /api/Pacjent/{id}` - Usuń pacjenta

### Lekarz
- `GET /api/Lekarz` - Lista lekarzy
- `GET /api/Lekarz/{id}` - Szczegóły lekarza
- `GET /api/Lekarz/specjalizacja/{spec}` - Lekarze po specjalizacji
- `POST /api/Lekarz` - Dodaj lekarza
- `PUT /api/Lekarz/{id}` - Aktualizuj lekarza
- `DELETE /api/Lekarz/{id}` - Usuń lekarza

### Wizyta
- `GET /api/Wizyta` - Lista wizyt
- `GET /api/Wizyta/{id}` - Szczegóły wizyty
- `GET /api/Wizyta/pacjent/{id}` - Wizyty pacjenta
- `GET /api/Wizyta/lekarz/{id}` - Wizyty lekarza
- `GET /api/Wizyta/status/{status}` - Wizyty po statusie
- `POST /api/Wizyta` - Zarezerwuj wizytę
- `PUT /api/Wizyta/{id}` - Aktualizuj wizytę
- `PATCH /api/Wizyta/{id}/cancel` - Anuluj wizytę
- `DELETE /api/Wizyta/{id}` - Usuń wizytę

### Ankieta (MongoDB)
- `GET /api/Ankieta` - Lista ankiet
- `GET /api/Ankieta/{id}` - Szczegóły ankiety
- `GET /api/Ankieta/pacjent/{id}` - Ankiety pacjenta
- `GET /api/Ankieta/pesel/{pesel}` - Ankiety po PESEL
- `GET /api/Ankieta/typ/{typ}` - Ankiety po typie
- `POST /api/Ankieta` - Utwórz ankietę
- `PUT /api/Ankieta/{id}` - Aktualizuj ankietę
- `PATCH /api/Ankieta/{id}/odpowiedz` - Dodaj odpowiedź do ankiety
- `DELETE /api/Ankieta/{id}` - Usuń ankietę

## 🧪 Przykładowe Requesty

### Dodanie pacjenta
```json
POST /api/Pacjent
{
  "imie": "Jan",
  "nazwisko": "Kowalski",
  "pesel": "90010112345"
}
```

### Dodanie lekarza
```json
POST /api/Lekarz
{
  "imie": "Anna",
  "nazwisko": "Nowak",
  "specjalizacja": "Kardiolog"
}
```

### Rezerwacja wizyty
```json
POST /api/Wizyta
{
  "data": "2025-11-10T10:00:00",
  "idPacjenta": 1,
  "idLekarza": 1
}
```

### Utworzenie ankiety
```json
POST /api/Ankieta
{
  "idPacjenta": 1,
  "pesel": "90010112345",
  "typAnkiety": "Przedoperacyjna",
  "odpowiedzi": [
    {
      "pytanie": "Czy jest Pan/Pani alergikiem?",
      "odpowiedz": "Tak, uczulenie na penicylinę",
      "kategoria": "Dane medyczne"
    },
    {
      "pytanie": "Czy przyjmuje Pan/Pani jakieś leki?",
      "odpowiedz": "Aspiryna 100mg codziennie",
      "kategoria": "Leki"
    },
    {
      "pytanie": "Grupa krwi",
      "odpowiedz": "A+",
      "kategoria": "Dane medyczne"
    }
  ],
  "dodatkoweUwagi": "Pacjent zgłasza nadciśnienie"
}
```

### Dodanie odpowiedzi do ankiety
```json
PATCH /api/Ankieta/{id}/odpowiedz
{
  "pytanie": "Czy pali Pan/Pani papierosy?",
  "odpowiedz": "Nie",
  "kategoria": "Styl życia"
}
```

## 🛠️ Technologie

### Backend
- .NET 8
- ASP.NET Core Web API
- Entity Framework Core 8
- Npgsql (PostgreSQL driver)
- MongoDB.Driver
- Swagger/OpenAPI

### Bazy Danych
- PostgreSQL 16
- MongoDB 7

### Frontend
- React 19
- TypeScript 5.9
- Vite 7

## 📝 Notatki

- Bazy danych są całkowicie rozdzielone (PostgreSQL i MongoDB)
- PESEL łączy dane między bazami (relacyjną i nierelacyjną)
- Status wizyty: `Zaplanowana`, `Odbyta`, `Anulowana`
- System sprawdza konflikty terminów dla lekarzy
- MongoDB przechowuje ankiety pacjentów z różnymi typami (elastyczna struktura)
- Ankiety mogą mieć dowolną liczbę pytań i odpowiedzi

## 🔧 Rozwój

### Potencjalne rozszerzenia:
- [ ] Autentykacja JWT
- [ ] System powiadomień (email/SMS)
- [ ] Szablony ankiet
- [ ] Harmonogram dostępności lekarzy
- [ ] Panel administracyjny
- [ ] Raporty i statystyki z ankiet
- [ ] API dla pacjentów (portal pacjenta)
- [ ] Analiza danych z ankiet (agregacje MongoDB)

## 📄 Licencja

Projekt edukacyjny - testowanie rozproszonych baz danych.
