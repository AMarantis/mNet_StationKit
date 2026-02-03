@echo off
if "%MNET_ROOT_EXE%"=="" (
  echo ERROR: MNET_ROOT_EXE is not set.>&2
  echo Expected something like: E:\mNetStationKit\deps\root\bin\root.exe>&2
  exit /b 1
)
"%MNET_ROOT_EXE%" -b -q vhist.C
