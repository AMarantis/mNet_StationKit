@echo off
setlocal

set "ROOT_EXE=%MNET_ROOT_EXE%"
if defined ROOT_EXE if exist "%ROOT_EXE%" goto run

set "SCRIPT_DIR=%~dp0"
for %%I in ("%SCRIPT_DIR%..\..\..\..") do set "KIT_ROOT=%%~fI"
if exist "%KIT_ROOT%\deps\root\bin\root.exe" (
  set "ROOT_EXE=%KIT_ROOT%\deps\root\bin\root.exe"
  goto run
)

for /d %%D in ("C:\root_v*") do (
  if exist "%%~fD\bin\root.exe" (
    set "ROOT_EXE=%%~fD\bin\root.exe"
    goto run
  )
)

echo ERROR: root.exe not found.>&2
echo Checked MNET_ROOT_EXE, "%KIT_ROOT%\deps\root\bin\root.exe", and C:\root_v*\bin\root.exe.>&2
exit /b 1

:run
"%ROOT_EXE%" -b -q "pulses.C(%1, %2, %3, %4)"
exit /b %ERRORLEVEL%
