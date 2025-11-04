@echo off

REM ==============================================
REM Copy missing Wifi production assets to bin folder
REM ==============================================
REM This script implements the TODO from WifiCopy.bat:
REM Copy Wifi html Webseitenverzeichnis like in Diva-Distribution Repository from osmwintool028.bat

echo _______________________________________________________________________________________________________________________
echo Kopiere Wifi WifiPages nach bin fuer Web-Interface...
echo _______________________________________________________________________________________________________________________

:: Bestimme das Arbeitsverzeichnis (ein Verzeichnis über bin)
set "WORKDIR=%~dp0.."
echo Das Arbeitsverzeichnis ist "%WORKDIR%"

:: Prüfe ob diva-distribution existiert
if exist "%WORKDIR%\diva-distribution\addon-modules\21Wifi\WifiPages" (
    echo Gefunden: diva-distribution\addon-modules\21Wifi\WifiPages
    
    :: Erstelle WifiPages Verzeichnis falls es nicht existiert
    if not exist "%WORKDIR%\bin\WifiPages" (
        echo Erstelle WifiPages Verzeichnis...
        mkdir "%WORKDIR%\bin\WifiPages"
    )
    
    :: Kopiere alle WifiPages Dateien
    echo Kopiere WifiPages Dateien...
    xcopy /s /y /i "%WORKDIR%\diva-distribution\addon-modules\21Wifi\WifiPages\*" "%WORKDIR%\bin\WifiPages\"
    
    if %errorlevel% equ 0 (
        echo Wifi WifiPages erfolgreich kopiert!
        echo.
        echo Die folgenden Dateien wurden kopiert:
        dir /s /b "%WORKDIR%\bin\WifiPages\*.html" 2>nul | find /c ".html" > temp_count.txt
        set /p HTML_COUNT=<temp_count.txt
        del temp_count.txt >nul 2>&1
        echo - HTML-Dateien und Templates
        echo - CSS-Stylesheets  
        echo - JavaScript-Dateien
        echo - Lokalisierungsdateien (de, en, es, fr, pt, ru)
        echo - Bilder und Assets
    ) else (
        echo FEHLER: Kopieren der WifiPages fehlgeschlagen!
    )
) else (
    echo WARNUNG: diva-distribution\addon-modules\21Wifi\WifiPages nicht gefunden!
    echo.
    echo Mögliche Lösungen:
    echo 1. Führen Sie zuerst osmwintool028.bat aus um diva-distribution herunterzuladen
    echo 2. Stellen Sie sicher, dass Sie sich im OpenSim bin\ Verzeichnis befinden
    echo 3. Überprüfen Sie, ob das diva-distribution Repository korrekt geklont wurde
)

echo _______________________________________________________________________________________________________________________
echo WifiCopy Vorgang abgeschlossen.
echo _______________________________________________________________________________________________________________________
pause
