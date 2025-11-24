@echo off
echo Uruchamianie projektu Przychodnia...
echo.

echo Uruchamianie Backend na porcie 5178...
start "Backend" cmd /k "cd /d Backend && dotnet run"

echo Czekanie na uruchomienie backendu...
timeout /t 5 /nobreak >nul

echo Uruchamianie Frontend na porcie 5173...
start "Frontend" cmd /k "cd /d frontend && npm run dev"

echo.
echo ===== PROJEKT URUCHOMIONY =====
echo Backend: http://localhost:5178
echo Frontend: http://localhost:5173
echo Swagger: http://localhost:5178/swagger
echo.
echo Konta testowe:
echo   - pacjent1@przychodnia.pl / Haslo!23
echo   - lekarz1@pans.pl / Haslo!23
echo   - lekarz2@pans.pl / Haslo!23
echo   - lekarz3@pans.pl / Haslo!23
echo   - admin@pans.pl / Haslo!23
echo.
echo Okna zostana zamkniete za 10 sekund...
timeout /t 10 /nobreak >nul
exit