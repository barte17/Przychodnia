@echo off
echo Uruchamianie projektu Przychodnia...
echo.

echo Uruchamianie Backend na porcie 5178...
start "Backend" cmd /k "cd /d Backend && dotnet run"

echo Czekanie na uruchomienie backendu...
timeout /t 5 /nobreak >nul

echo Uruchamianie Frontend na porcie 3000...
start "Frontend" cmd /k "cd /d frontend && npm run dev"

echo.
echo ===== PROJEKT URUCHOMIONY =====
echo Backend: https://localhost:5178
echo Frontend: http://localhost:3000
echo Swagger: https://localhost:5178/swagger
echo.
echo Okna zostana zamkniete za 10 sekund...
timeout /t 10 /nobreak >nul
exit