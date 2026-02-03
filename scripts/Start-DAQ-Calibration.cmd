@echo off
setlocal
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Start-DAQ-Calibration.ps1"
exit /b %ERRORLEVEL%

