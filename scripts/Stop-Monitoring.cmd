@echo off
setlocal
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Stop-Monitoring.ps1"
set "ec=%ERRORLEVEL%"
if not "%ec%"=="0" (
  echo.
  echo [ERROR] Stop-Monitoring failed with exit code %ec%.
  pause
)
exit /b %ec%

