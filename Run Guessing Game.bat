set SCRIPT_DIR=%~dp0
set APP_DIR=%SCRIPT_DIR%Dist\GuessingGame.exe
set URL=https://localhost:5001/

start "" "%APP_DIR%"
timeout /t 5 /nobreak >nul
start "" "%URL%"
