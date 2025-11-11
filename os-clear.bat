@echo off

REM Lösche obj Ordner, *.csproj Dateien und  *.sln Dateien,in allen Verzeichnissen und Unterverzeichnissen
echo Lösche obj Ordner...
for /d /r %%i in (obj) do @if exist "%%i" rd /s /q "%%i" 2>nul
echo Lösche *.csproj Dateien...
for /r %%i in (*.csproj) do @if exist "%%i" del /q "%%i" 2>nul
echo Lösche *.sln Dateien...
for /r %%i in (*.sln) do @if exist "%%i" del /q "%%i" 2>nul

echo Lösche addin-db-002 und addin-db-004 Ordner im bin Verzeichnis...
if exist "bin\addin-db-002" (
	del /F/Q/S bin\addin-db-002 > NUL
	rmdir /Q/S bin\addin-db-002
	)
if exist "bin\addin-db-004" (
	del /F/Q/S bin\addin-db-004 > NUL
	rmdir /Q/S bin\addin-db-004
	)

cd bin
echo Delete OpenSim.log
del *.log
echo Delete OpenSim sqlite database files
del *.db
del *.db-wal
del *.db-shm

echo Fertig.
pause