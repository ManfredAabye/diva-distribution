@echo OFF

echo Building Diva Distribution for .NET 8
echo ======================================

rem Führe prebuild mit vs2022 target aus
bin\Prebuild.exe /target vs2022

setlocal ENABLEEXTENSIONS
set VALUE_NAME=MSBuildToolsPath

if "%PROCESSOR_ARCHITECTURE%"=="x86" set PROGRAMS=%ProgramFiles%
if defined ProgramFiles(x86) set PROGRAMS=%ProgramFiles(x86)%

rem Try to find VS2026 first
for %%e in (Enterprise Professional Community) do (
    if exist "%PROGRAMS%\Microsoft Visual Studio\2026\%%e\MSBuild\Current\Bin\MSBuild.exe" (
        set ValueValue="%PROGRAMS%\Microsoft Visual Studio\2026\%%e\MSBuild\Current\Bin\MSBuild"
        echo Found VS2026 %%e
        goto :found
    )
)

rem Try to find VS2022
for %%e in (Enterprise Professional Community) do (
    if exist "%PROGRAMS%\Microsoft Visual Studio\2022\%%e\MSBuild\Current\Bin\MSBuild.exe" (
        set ValueValue="%PROGRAMS%\Microsoft Visual Studio\2022\%%e\MSBuild\Current\Bin\MSBuild"
        echo Found VS2022 %%e
        goto :found
    )
)
for %%e in (Enterprise Professional Community) do (
    if exist "%PROGRAMS%\Microsoft Visual Studio\2017\%%e\MSBuild\15.0\Bin\MSBuild.exe" (

        set ValueValue="%PROGRAMS%\Microsoft Visual Studio\2017\%%e\MSBuild\15.0\Bin\MSBuild"
		goto :found
    )
)

rem We have to use grep or find to locate the correct line, because reg query spits
rem out 4 lines before Windows 7 but 2 lines after Windows 7.
rem We use grep if it's on the path; otherwise we use the built-in find command
rem from Windows. (We must use grep on Cygwin because it overrides the "find" command.)

for %%X in (grep.exe) do (set FOUNDGREP=%%~$PATH:X)
if defined FOUNDGREP (
  set FINDCMD=grep
) else (
  set FINDCMD=find
)

rem Try to find dotnet CLI if no Visual Studio found
where dotnet >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo Using dotnet CLI for build
    set ValueValue=dotnet
    goto :found
)

@echo Error: Neither Visual Studio 2022/2026 nor .NET CLI found!
@echo Please install one of the following:
@echo - Visual Studio 2022 or 2026 (Community, Professional, or Enterprise)
@echo - .NET 8 SDK from https://dotnet.microsoft.com/download/dotnet/8.0
@echo Not creating compile.bat
if exist "compile.bat" (
	del compile.bat
	)
goto :done

:found
if "%ValueValue%"=="dotnet" (
    @echo Found .NET CLI
    @echo Creating compile.bat for .NET 8
    @echo dotnet build opensim.sln -c Debug > compile.bat
    @echo echo "To compile in Release mode, use: dotnet build opensim.sln -c Release" >> compile.bat
) else (
    @echo Found MSBuild at %ValueValue%
    @echo Creating compile.bat
    @echo %ValueValue% opensim.sln > compile.bat
    rem To compile in release mode comment line (add rem to start) above and uncomment next (remove rem)
    rem @echo %ValueValue% /p:Configuration=Release opensim.sln > compile.bat
)
:done
if exist "bin\addin-db-002" (
	del /F/Q/S bin\addin-db-002 > NUL
	rmdir /Q/S bin\addin-db-002
	)