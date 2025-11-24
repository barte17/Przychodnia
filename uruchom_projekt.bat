@echo off
chcp 65001 >nul
echo ╔══════════════════════════════════════════════════════════╗
echo ║          PRZYCHODNIA - Uruchamianie projektu             ║
echo ╚══════════════════════════════════════════════════════════╝
echo.

echo [1/2] Uruchamianie Backend (.NET Core)...
echo       Port HTTP:  5178
echo       Port HTTPS: 5179
start "Backend - Przychodnia" cmd /k "cd /d Backend && dotnet run"

echo.
echo Czekanie na uruchomienie backendu (8 sekund)...
timeout /t 8 /nobreak >nul

echo.
echo [2/2] Uruchamianie Frontend (React + Vite)...
echo       Port: 5173
start "Frontend - Przychodnia" cmd /k "cd /d frontend && npm run dev"

echo.
echo ╔══════════════════════════════════════════════════════════╗
echo ║              PROJEKT URUCHOMIONY POMYSLNIE               ║
echo ╚══════════════════════════════════════════════════════════╝
echo.
echo  ADRESY:
echo  ─────────────────────────────────────────────────────────
echo  Frontend:     http://localhost:5173
echo  Backend API:  http://localhost:5178
echo  Swagger:      http://localhost:5178/swagger
echo.
echo  KONTA TESTOWE (haslo: Haslo!23):
echo  ─────────────────────────────────────────────────────────
echo  Lekarze:      lek@pans.pl, lek2@pans.pl
echo  Pacjenci:     pac@pans.pl, pac2@pans.pl
echo.
echo  BAZY DANYCH:
echo  ─────────────────────────────────────────────────────────
echo  PostgreSQL:   Aiven (relacyjna - uzytkownicy, wizyty)
echo  MongoDB:      Atlas (nierelacyjna - ankiety)
echo.
echo  Aby zatrzymac projekt, zamknij okna terminali.
echo.
pause