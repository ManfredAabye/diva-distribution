@echo off
REM Lösche obj Ordner, *.csproj Dateien und  *.sln Dateien,in allen Verzeichnissen und Unterverzeichnissen
echo Lösche obj Ordner...
for /d /r %%i in (obj) do rd /s /q "%%i"
echo Lösche *.csproj Dateien...
for /r %%i in (*.csproj) do del /q "%%i"
echo Lösche *.sln Dateien...
for /r %%i in (*.sln) do del /q "%%i"
echo Fertig.

copy bin\System.Drawing.Common.dll.win bin\System.Drawing.Common.dll

dotnet bin\prebuild.dll /target vs2022 /targetframework net8_0 /excludedir = "obj | bin" /file prebuild.xml

    @echo Creating compile.bat
rem To compile in release mode
    @echo dotnet build --configuration Release OpenSim.sln > compile.bat
rem To compile in debug mode comment line (add rem to start) above and uncomment next (remove rem)
rem    @echo dotnet build --configuration Debug OpenSim.sln > compile.bat
:done


if exist "bin\addin-db-002" (
	del /F/Q/S bin\addin-db-002 > NUL
	rmdir /Q/S bin\addin-db-002
	)
if exist "bin\addin-db-004" (
	del /F/Q/S bin\addin-db-004 > NUL
	rmdir /Q/S bin\addin-db-004
	)

dotnet build --configuration Release OpenSim.sln 
pause