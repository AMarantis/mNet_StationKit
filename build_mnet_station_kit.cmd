@echo off
setlocal
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0build_mnet_station_kit.ps1"
echo.
echo If there were errors above, copy them and send them for debugging.
pause
exit /b %ERRORLEVEL%

