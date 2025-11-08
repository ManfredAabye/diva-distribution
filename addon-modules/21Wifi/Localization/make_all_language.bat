@echo off
rem This batch file builds satellite assemblies for all available languages
rem by calling make_languages.bat with all language codes found in the directory

echo Building satellite assemblies for all languages...
echo.

rem Check if -o parameter is provided
set OSBIN_PARAM=
if "%1" == "-o" (
    set OSBIN_PARAM=-o %2
    echo Using OpenSim bin directory: %2
    echo.
)

rem Build for all available languages
rem List of all language codes from .po/.resx files
call make_languages.bat %OSBIN_PARAM% da de el en es fr it ja ko nl pl pt pt-BR ru sv tr zh-CN zh-TW

echo.
echo All satellite assemblies have been created!
echo.
pause
