@echo off
REM Lösche obj Ordner, *.csproj Dateien und  *.sln Dateien,in allen Verzeichnissen und Unterverzeichnissen
echo Lösche obj Ordner...
for /d /r %%i in (obj) do rd /s /q "%%i"
echo Lösche *.csproj Dateien...
for /r %%i in (*.csproj) do del /q "%%i"
echo Lösche *.sln Dateien...
for /r %%i in (*.sln) do del /q "%%i"
echo Fertig.

./prebuild.bat
pause