@echo off
setlocal
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Create-Desktop-Shortcuts.ps1"
set "ec=%ERRORLEVEL%"
if not "%ec%"=="0" (
  echo.
  echo [ERROR] Create-Desktop-Shortcuts failed with exit code %ec%.
  pause
)
exit /b %ec%

