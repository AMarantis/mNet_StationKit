@echo off
setlocal
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Stop-DAQ.ps1"
set "ec=%ERRORLEVEL%"
if not "%ec%"=="0" (
  echo.
  echo [ERROR] Stop-DAQ failed with exit code %ec%.
  pause
)
exit /b %ec%

