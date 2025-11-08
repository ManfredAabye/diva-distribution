@echo off
rem Update all .po files with new entries from the .pot file
rem This script merges old translations with new entries from Diva.Wifi.pot
rem Requires msgmerge from gettext tools (or uses PowerShell fallback)

echo Updating all .po files from Diva.Wifi.pot...
echo.

set LANGUAGES=da de el en es fr it ja ko nl pl pt pt-BR ru sv tr zh-CN zh-TW

rem Check if msgmerge is available
where msgmerge >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo Using msgmerge for professional .po file updates...
    echo.
    for %%L in (%LANGUAGES%) do (
        if exist Diva.Wifi.%%L.po (
            echo Updating Diva.Wifi.%%L.po...
            msgmerge --update --backup=none Diva.Wifi.%%L.po Diva.Wifi.pot
        ) else (
            echo Creating new Diva.Wifi.%%L.po...
            msginit --no-translator --input=Diva.Wifi.pot --output-file=Diva.Wifi.%%L.po --locale=%%L
        )
    )
) else (
    echo msgmerge not found. Using PowerShell fallback...
    echo Note: Install gettext tools for better .po file handling.
    echo Download: https://mlocati.github.io/articles/gettext-iconv-windows.html
    echo.
    powershell -ExecutionPolicy Bypass -File update_po_files.ps1
)

echo.
echo All .po files have been updated!
echo.
echo Next steps:
echo 1. Translate the new entries in each .po file (marked with empty msgstr)
echo 2. Run update_resx_files.bat to generate .resx files from .po files
echo 3. Run make_all_language.bat to build satellite assemblies
echo.
pause
