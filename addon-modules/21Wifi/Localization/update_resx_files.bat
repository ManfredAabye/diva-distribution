@echo off
rem Update .resx files from .po files
rem This converts translated .po files into .NET .resx resource files

echo Converting .po files to .resx files...
echo.

set LANGUAGES=da de el en es fr it ja ko nl pl pt pt-BR ru sv tr zh-CN zh-TW

rem Check if po2resx or similar tool is available
where msgfmt >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo Using gettext msgfmt...
    echo Note: You may need a .po to .resx converter tool
    echo.
) else (
    echo Using PowerShell converter...
    echo.
    powershell -ExecutionPolicy Bypass -File update_resx_files.ps1
    goto :end
)

:end
echo.
echo .resx files updated!
echo Next step: Run make_all_language.bat to build satellite assemblies
echo.
pause
