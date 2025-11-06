@echo off
REM Lösche obj Ordner, *.csproj Dateien und  *.sln Dateien,in allen Verzeichnissen und Unterverzeichnissen
echo Lösche obj Ordner...
for /d /r %%i in (obj) do @if exist "%%i" rd /s /q "%%i" 2>nul
echo Lösche *.csproj Dateien...
for /r %%i in (*.csproj) do @if exist "%%i" del /q "%%i" 2>nul
echo Lösche *.sln Dateien...
for /r %%i in (*.sln) do @if exist "%%i" del /q "%%i" 2>nul
echo Fertig.

if exist "bin\addin-db-002" (
	del /F/Q/S bin\addin-db-002 > NUL
	rmdir /Q/S bin\addin-db-002
	)
if exist "bin\addin-db-004" (
	del /F/Q/S bin\addin-db-004 > NUL
	rmdir /Q/S bin\addin-db-004
	)

copy bin\System.Drawing.Common.dll.win bin\System.Drawing.Common.dll

dotnet bin\prebuild.dll /target vs2022 /targetframework net8_0 /excludedir = "obj | bin" /file prebuild.xml

REM Wähle Build-Konfiguration: Debug oder Release
set BUILD_CONFIG=Debug
REM Für Release-Build diese Zeile auskommentieren (REM entfernen) und Debug-Zeile auskommentieren:
REM set BUILD_CONFIG=Release

@echo Creating compile.bat mit %BUILD_CONFIG% Konfiguration...
rem To compile in release mode
@echo dotnet build --configuration %BUILD_CONFIG% OpenSim.sln > compile.bat
rem To compile in debug mode comment line (add rem to start) above and uncomment next (remove rem)
rem    @echo dotnet build --configuration Debug OpenSim.sln > compile.bat
:done

echo Kompiliere mit %BUILD_CONFIG% Konfiguration...
dotnet build --configuration %BUILD_CONFIG% OpenSim.sln
pause