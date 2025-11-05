@echo off
echo _______________________________________________________________________________________________________________________
echo OpenSim Windows Tool Version 05112025 V29
echo System requirements for building OpenSimulator:
echo DOTNET 8.0.x https://dotnet.microsoft.com/en-us/download/dotnet/8.0
echo Visual Studio 22 26 Community https://visualstudio.microsoft.com/vs/community/
echo Git https://git-scm.com/downloads/win
echo _______________________________________________________________________________________________________________________

:: Configurations
set "WORKDIR=%~dp0"
setlocal enabledelayedexpansion
echo The working directory is "%WORKDIR%"


set /p setupChoice="Do you want to start the setup? ([Y/y]/N/n): "

:: Set default value "Y" if no input was made
if "%setupChoice%"=="" set setupChoice=Y

if /i "%setupChoice%" NEQ "Y" (
    echo Setup aborted.
    exit /b
)

set /p setupChoice="Do you want to delete the old opensimsource directory? ([Y/y]/N/n): "

:: Set default value "N" if no input was made
if "%setupChoice%"=="" set setupChoice=N

if /i "%setupChoice%" EQU "Y" (
    echo Deleting opensimsource directory...
    rmdir /s /q "%WORKDIR%opensimsource" >nul 2>&1
    timeout /t 3
    if exist "%WORKDIR%opensimsource" (
        echo ERROR: Directory could not be deleted!
        pause
        exit /b 1
    )
    echo Directory successfully deleted.
)

echo _______________________________________________________________________________________________________________________
echo Starting OpenSim setup...
echo _______________________________________________________________________________________________________________________
echo Checking if the opensimsource directory exists
echo _______________________________________________________________________________________________________________________

if not exist "%WORKDIR%opensimsource" (
    echo opensimsource directory does not exist, downloading OpenSim...
    git clone https://github.com/opensim/opensim.git "%WORKDIR%opensimsource"
    if !errorlevel! neq 0 (
        echo ERROR: OpenSim could not be downloaded!
        pause
        exit /b 1
    )
) else (
    echo opensimsource present, updating...
    pushd "%WORKDIR%opensimsource"
    
    echo Determining default branch...
    set "BRANCH_INFO="
    for /f "tokens=*" %%i in ('git remote show origin ^| findstr "HEAD"') do (
        set "BRANCH_INFO=%%i"
    )
    
    :: Extract the branch name
    set "DEFAULT_BRANCH=!BRANCH_INFO:*HEAD branch: =!"
    
    if "!DEFAULT_BRANCH!"=="" (
        echo Could not determine default branch, using master...
        set "DEFAULT_BRANCH=master"
    )
    
    echo Executing git pull for branch '!DEFAULT_BRANCH!'...
    git pull origin !DEFAULT_BRANCH!
    
    if !errorlevel! neq 0 (
        echo ERROR: Git pull for branch !DEFAULT_BRANCH! failed!
        popd
        pause
        exit /b 1
    )
    
    echo Git pull completed successfully.
    popd
)

echo _______________________________________________________________________________________________________________________
echo WebRTC Janus Repository
echo _______________________________________________________________________________________________________________________

set /p webrtcChoice="Do you want to download the webrtc repository? ([N/n]/Y/y): "

:: Set default value "N" if no input was made
if "%webrtcChoice%"=="" set webrtcChoice=N

if /i "%webrtcChoice%"=="Y" (
    echo.
    echo Cloning os-webrtc-janus repository...
    :: D:\OpenSim_Win_Builder\opensimsource\addon-modules
    set "JANUS_DIR=%WORKDIR%opensimsource\addon-modules\os-webrtc-janus"

    if not exist "%JANUS_DIR%" (
        git clone https://github.com/Misterblue/os-webrtc-janus.git %WORKDIR%opensimsource\addon-modules\os-webrtc-janus"
        if !errorlevel! neq 0 (
            echo ERROR: os-webrtc-janus could not be downloaded!
            pause
            exit /b 1
        )
        echo os-webrtc-janus successfully downloaded!
    ) else (
        echo os-webrtc-janus present, updating...
        pushd "%WORKDIR%opensimsource\addon-modules\os-webrtc-janus"
        git pull origin main
        if !errorlevel! neq 0 (
            echo ERROR: Git pull for os-webrtc-janus failed!
            popd
            pause
            exit /b 1
        )
        echo os-webrtc-janus successfully updated!
        popd
    )
) else (
    echo WebRTC Janus Repository skipped.
)

echo _______________________________________________________________________________________________________________________
echo Diva-Distribution Repository
echo _______________________________________________________________________________________________________________________

set /p divaChoice="Do you want to download the Diva-Distribution repository? ([N/n]/Y/y): "

:: Set default value "N" if no input was made
if "%divaChoice%"=="" set divaChoice=N

if /i "%divaChoice%"=="Y" (
    if not exist "%WORKDIR%opensimsource\diva-distribution" (
        echo Downloading Diva-Distribution...
        git clone https://github.com/ManfredAabye/diva-distribution.git "%WORKDIR%opensimsource\diva-distribution"
        if !errorlevel! neq 0 (
            echo ERROR: Diva-Distribution could not be downloaded!
        ) else (
            echo Copying Diva-Distribution addon-modules files to addon-modules...
            if exist "%WORKDIR%opensimsource\diva-distribution\addon-modules" (
                if not exist "%WORKDIR%opensimsource\addon-modules" mkdir "%WORKDIR%opensimsource\addon-modules"
                xcopy /s /y /i "%WORKDIR%opensimsource\diva-distribution\addon-modules\*" "%WORKDIR%opensimsource\addon-modules\"
                echo Diva-Distribution addon-modules files successfully copied!
            )
            echo Copying Diva-Distribution bin files to bin...
            if exist "%WORKDIR%opensimsource\diva-distribution\bin" (
                if not exist "%WORKDIR%opensimsource\bin" mkdir "%WORKDIR%opensimsource\bin"
                xcopy /s /y /i "%WORKDIR%opensimsource\diva-distribution\bin\*" "%WORKDIR%opensimsource\bin\"
                echo Diva-Distribution bin files successfully copied!
            )
            echo Copying diva-doc directory to opensimsource...
            if exist "%WORKDIR%opensimsource\diva-distribution\diva-doc" (
                if not exist "%WORKDIR%opensimsource\diva-doc" mkdir "%WORKDIR%opensimsource\diva-doc"
                xcopy /s /y /i "%WORKDIR%opensimsource\diva-distribution\diva-doc\*" "%WORKDIR%opensimsource\diva-doc\"
                echo diva-doc directory successfully copied!
            )
            echo Copying *.md and *.bat files from diva-distribution to opensimsource...
            if exist "%WORKDIR%opensimsource\diva-distribution\*.md" (
                xcopy /y "%WORKDIR%opensimsource\diva-distribution\*.md" "%WORKDIR%opensimsource\"
                echo *.md files successfully copied!
            )
            if exist "%WORKDIR%opensimsource\diva-distribution\*.bat" (
                xcopy /y "%WORKDIR%opensimsource\diva-distribution\*.bat" "%WORKDIR%opensimsource\"
                echo *.bat files successfully copied!
            )
            echo Copying Wifi WifiPages to bin for web interface...
            if exist "%WORKDIR%opensimsource\diva-distribution\addon-modules\21Wifi\WifiPages" (
                if not exist "%WORKDIR%opensimsource\bin\WifiPages" mkdir "%WORKDIR%opensimsource\bin\WifiPages"
                xcopy /s /y /i "%WORKDIR%opensimsource\diva-distribution\addon-modules\21Wifi\WifiPages\*" "%WORKDIR%opensimsource\bin\WifiPages\"
                echo Wifi WifiPages successfully copied!
            )
            echo Diva-Distribution successfully downloaded!
        )
    ) else (
        echo Diva-Distribution present, updating...
        pushd "%WORKDIR%opensimsource\diva-distribution"
        git pull origin master
        if !errorlevel! neq 0 (
            echo ERROR: Git pull for Diva-Distribution failed!
        ) else (
            echo Copying Diva-Distribution addon-modules files to addon-modules...
            if exist "%WORKDIR%opensimsource\diva-distribution\addon-modules" (
                if not exist "%WORKDIR%opensimsource\addon-modules" mkdir "%WORKDIR%opensimsource\addon-modules"
                xcopy /s /y /i "%WORKDIR%opensimsource\diva-distribution\addon-modules\*" "%WORKDIR%opensimsource\addon-modules\"
                echo Diva-Distribution addon-modules files successfully copied!
            )
            echo Copying Diva-Distribution bin files to bin...
            if exist "%WORKDIR%opensimsource\diva-distribution\bin" (
                if not exist "%WORKDIR%opensimsource\bin" mkdir "%WORKDIR%opensimsource\bin"
                xcopy /s /y /i "%WORKDIR%opensimsource\diva-distribution\bin\*" "%WORKDIR%opensimsource\bin\"
                echo Diva-Distribution bin files successfully copied!
            )
            echo Copying diva-doc directory to opensimsource...
            if exist "%WORKDIR%opensimsource\diva-distribution\diva-doc" (
                if not exist "%WORKDIR%opensimsource\diva-doc" mkdir "%WORKDIR%opensimsource\diva-doc"
                xcopy /s /y /i "%WORKDIR%opensimsource\diva-distribution\diva-doc\*" "%WORKDIR%opensimsource\diva-doc\"
                echo diva-doc directory successfully copied!
            )
            echo Copying *.md and *.bat files from diva-distribution to opensimsource...
            if exist "%WORKDIR%opensimsource\diva-distribution\*.md" (
                xcopy /y "%WORKDIR%opensimsource\diva-distribution\*.md" "%WORKDIR%opensimsource\"
                echo *.md files successfully copied!
            )
            if exist "%WORKDIR%opensimsource\diva-distribution\*.bat" (
                xcopy /y "%WORKDIR%opensimsource\diva-distribution\*.bat" "%WORKDIR%opensimsource\"
                echo *.bat files successfully copied!
            )
            echo Copying Wifi WifiPages to bin for web interface...
            if exist "%WORKDIR%opensimsource\diva-distribution\addon-modules\21Wifi\WifiPages" (
                if not exist "%WORKDIR%opensimsource\bin\WifiPages" mkdir "%WORKDIR%opensimsource\bin\WifiPages"
                xcopy /s /y /i "%WORKDIR%opensimsource\diva-distribution\addon-modules\21Wifi\WifiPages\*" "%WORKDIR%opensimsource\bin\WifiPages\"
                echo Wifi WifiPages successfully copied!
            )
            echo Diva-Distribution successfully updated!
        )
        popd
    )
) else (
    echo Diva-Distribution Repository skipped.
)

echo _______________________________________________________________________________________________________________________
echo opensim-spl-sql-system Repository
echo _______________________________________________________________________________________________________________________
    set /p sqlsystemChoice="Do you want to download the opensim-spl-sql-system repository? ([N/n]/Y/y): "

    :: Set default value "N" if no input was made
    if "%sqlsystemChoice%"=="" set sqlsystemChoice=N

    if /i "%sqlsystemChoice%"=="Y" (
        if not exist "%WORKDIR%opensimsource\opensim-spl-sql-system" (
            echo Downloading opensim-spl-sql-system...
            git clone https://github.com/ManfredAabye/opensim-spl-sql-system.git "%WORKDIR%opensimsource\opensim-spl-sql-system"
            if %errorlevel% neq 0 (
                echo ERROR: opensim-spl-sql-system could not be downloaded!
            ) else (
                echo Copying opensim-spl-sql-system files...
                
                :: Copy OpenSim content to main OpenSim directory
                if exist "%WORKDIR%opensimsource\opensim-spl-sql-system\OpenSim" (
                    xcopy /s /y /i "%WORKDIR%opensimsource\opensim-spl-sql-system\OpenSim\*" "%WORKDIR%opensimsource\"
                )
                
                :: Fallback: Copy to addon-modules if no OpenSim directory exists
                if not exist "%WORKDIR%opensimsource\opensim-spl-sql-system\OpenSim" (
                    if not exist "%WORKDIR%opensimsource\addon-modules" mkdir "%WORKDIR%opensimsource\addon-modules"
                    if not exist "%WORKDIR%opensimsource\addon-modules\opensim-spl-sql-system" mkdir "%WORKDIR%opensimsource\addon-modules\opensim-spl-sql-system"
                    xcopy /s /y /i "%WORKDIR%opensimsource\opensim-spl-sql-system\*" "%WORKDIR%opensimsource\addon-modules\opensim-spl-sql-system\"
                )
                echo opensim-spl-sql-system successfully installed.
            )
        ) else (
            echo opensim-spl-sql-system already present, skipping download...
        )
    ) else (
        echo opensim-spl-sql-system Repository skipped.
    )

echo _______________________________________________________________________________________________________________________
echo OpenSimSearch Repository
echo _______________________________________________________________________________________________________________________
    set /p OpenSimSearchChoice="Do you want to download the OpenSimSearch repository? ([N/n]/Y/y): "

    :: Set default value "N" if no input was made
    if "%OpenSimSearchChoice%"=="" set OpenSimSearchChoice=N

    if /i "%OpenSimSearchChoice%"=="Y" (
        if not exist "%WORKDIR%opensimsource\opensimsearch" (
            echo Downloading OpenSimSearch...
            git clone https://github.com/ManfredAabye/OpenSimSearch.git "%WORKDIR%opensimsource\opensimsearch"
            if %errorlevel% neq 0 (
                echo ERROR: OpenSimSearch could not be downloaded!
            ) else (
                echo Copying OpenSimSearch to addon-modules...
                if not exist "%WORKDIR%opensimsource\addon-modules" mkdir "%WORKDIR%opensimsource\addon-modules"
                
                :: Copy addon-modules content to addon-modules
                if exist "%WORKDIR%opensimsource\opensimsearch\addon-modules" (
                    xcopy /s /y /i "%WORKDIR%opensimsource\opensimsearch\addon-modules\*" "%WORKDIR%opensimsource\addon-modules\"
                )
                
                :: Fallback: If OpenSimSearch subdirectory exists
                if exist "%WORKDIR%opensimsource\opensimsearch\OpenSimSearch" (
                    if not exist "%WORKDIR%opensimsource\addon-modules\OpenSimSearch" mkdir "%WORKDIR%opensimsource\addon-modules\OpenSimSearch"
                    xcopy /s /y /i "%WORKDIR%opensimsource\opensimsearch\OpenSimSearch\*" "%WORKDIR%opensimsource\addon-modules\OpenSimSearch\"
                )
                echo OpenSimSearch successfully installed.
            )
        ) else (
            echo opensimsearch already present, skipping download...
        )
    ) else (
        echo OpenSimSearch Repository skipped.
    )

echo _______________________________________________________________________________________________________________________
echo OpenSim-MariaDB-Micro-Database
echo _______________________________________________________________________________________________________________________
    set /p mariaDBChoice="Do you want to download the OpenSim-MariaDB-Micro-Database? ([N/n]/Y/y): "

    :: Set default value "N" if no input was made
    if "%mariaDBChoice%"=="" set mariaDBChoice=N

    if /i "%mariaDBChoice%"=="Y" (
        if not exist "%WORKDIR%opensimsource\opensim-mariadb" (
            echo Downloading OpenSim-MariaDB-Micro-Database...
            git clone https://github.com/ManfredAabye/OpenSim-MariaDB-Micro-Database.git "%WORKDIR%opensimsource\opensim-mariadb"
            if %errorlevel% neq 0 (
                echo ERROR: OpenSim-MariaDB-Micro-Database could not be downloaded!
            ) else (
                echo OpenSim-MariaDB-Micro-Database successfully downloaded.
                echo Copying MariaDB files to opensimsource...
                if exist "%WORKDIR%opensimsource\opensim-mariadb\bin" (
                    xcopy /s /y /i "%WORKDIR%opensimsource\opensim-mariadb\bin\*" "%WORKDIR%opensimsource\bin\"
                )
                if exist "%WORKDIR%opensimsource\opensim-mariadb\addon-modules" (
                    if not exist "%WORKDIR%opensimsource\addon-modules" mkdir "%WORKDIR%opensimsource\addon-modules"
                    xcopy /s /y /i "%WORKDIR%opensimsource\opensim-mariadb\addon-modules\*" "%WORKDIR%opensimsource\addon-modules\"
                )
                if exist "%WORKDIR%opensimsource\opensim-mariadb\config-include" (
                    if not exist "%WORKDIR%opensimsource\bin\config-include" mkdir "%WORKDIR%opensimsource\bin\config-include"
                    xcopy /s /y /i "%WORKDIR%opensimsource\opensim-mariadb\config-include\*" "%WORKDIR%opensimsource\bin\config-include\"
                )
                echo MariaDB files successfully copied.
                echo To set up, mariadb.bat must be started twice.
            )
        ) else (
            echo opensim-mariadb already present, skipping download...
        )
    ) else (
        echo OpenSim-MariaDB-Micro-Database skipped.
    )

echo _______________________________________________________________________________________________________________________
echo Money Repository
echo _______________________________________________________________________________________________________________________
set /p moneyChoice="Do you want to download the Money repository? ([N/n]/Y/y): "

:: Set default value "N" if no input was made
if "%moneyChoice%"=="" set moneyChoice=N

if /i "%moneyChoice%"=="Y" (
    echo _______________________________________________________________________________________________________________________
    echo Clone Money Repository to a subdirectory of opensimsource
echo _______________________________________________________________________________________________________________________

    if not exist "%WORKDIR%opensimsource\opensimcurrencyserver" (
        echo Downloading Money module...
        git clone https://github.com/ManfredAabye/opensimcurrencyserver-dotnet.git "%WORKDIR%opensimsource\opensimcurrencyserver"
        if %errorlevel% neq 0 (
            echo ERROR: Money module could not be downloaded!
        ) else (
            echo Money module successfully downloaded.
        )
    ) else (
        echo Money module already present, skipping download...
    )

    echo _______________________________________________________________________________________________________________________
    echo Copying files from opensimcurrencyserver to opensimsource...
    echo _______________________________________________________________________________________________________________________

    if exist "%WORKDIR%opensimsource\opensimcurrencyserver\addon-modules" (
        if not exist "%WORKDIR%opensimsource\addon-modules" mkdir "%WORKDIR%opensimsource\addon-modules"
        xcopy /s /y /i "%WORKDIR%opensimsource\opensimcurrencyserver\addon-modules\*" "%WORKDIR%opensimsource\addon-modules\"
    )
    if exist "%WORKDIR%opensimsource\opensimcurrencyserver\bin" (
        xcopy /s /y /i "%WORKDIR%opensimsource\opensimcurrencyserver\bin\*" "%WORKDIR%opensimsource\bin\"
    )
    ) else (
        echo Money Repository skipped.
    )

echo _______________________________________________________________________________________________________________________
echo Inventory Repository
echo _______________________________________________________________________________________________________________________
set /p InventarChoice="Do you want to download the Inventory repository? ([Y/y]/N/n): "

:: Set default value "N" if no input was made
if "%InventarChoice%"=="" set InventarChoice=N

if /i "%InventarChoice%"=="Y" (

    :: Download scripts
    if not exist "%WORKDIR%opensimsource\opensim-ossl-example-scripts" (
    echo Downloading scripts...
    cd "%WORKDIR%opensimsource"
    git clone https://github.com/ManfredAabye/opensim-ossl-example-scripts.git opensim-ossl-example-scripts
    cd "%WORKDIR%"
    ) else (
        echo opensim-ossl-example-scripts already present, skipping download...
    )
    if not exist "%WORKDIR%opensimsource\bin\assets\ScriptsLibrary" mkdir "%WORKDIR%opensimsource\bin\assets\ScriptsLibrary"
    if not exist "%WORKDIR%opensimsource\bin\inventory\ScriptsLibrary" mkdir "%WORKDIR%opensimsource\bin\inventory\ScriptsLibrary"
    xcopy /s /y /i "%WORKDIR%opensimsource\opensim-ossl-example-scripts\ScriptsAssetSet\*" "%WORKDIR%opensimsource\bin\assets\ScriptsLibrary\"
    xcopy /s /y /i "%WORKDIR%opensimsource\opensim-ossl-example-scripts\inventory\ScriptsLibrary\*" "%WORKDIR%opensimsource\bin\inventory\ScriptsLibrary\"

    :: Inventory Ruth2
    if not exist "%WORKDIR%opensimsource\Ruth2" (
    echo Downloading Ruth2...
    cd "%WORKDIR%opensimsource"
    git clone https://github.com/ManfredAabye/Ruth2.git Ruth2
    cd "%WORKDIR%"
    ) else (
        echo Ruth2 already present, skipping download...
    )
    if not exist "%WORKDIR%opensimsource\bin\Library" mkdir "%WORKDIR%opensimsource\bin\Library"
    xcopy /s /y /i "%WORKDIR%opensimsource\Ruth2\Artifacts\IAR\*.iar" "%WORKDIR%opensimsource\bin\Library\"

    :: Inventory Roth2
    if not exist "%WORKDIR%opensimsource\Roth2" (
    echo Downloading Roth2...
    cd "%WORKDIR%opensimsource"
    git clone https://github.com/ManfredAabye/Roth2.git Roth2
    cd "%WORKDIR%"
    ) else (
        echo Roth2 already present, skipping download...
    )
    xcopy /s /y /i "%WORKDIR%opensimsource\Roth2\Artifacts\IAR\*.iar" "%WORKDIR%opensimsource\bin\Library\"
    ) else (
        echo Inventory Repository skipped.
)

echo _______________________________________________________________________________________________________________________
echo Switching to the opensimsource directory...
echo _______________________________________________________________________________________________________________________

cd "%WORKDIR%opensimsource"

echo _______________________________________________________________________________________________________________________
echo Remove example extensions
echo _______________________________________________________________________________________________________________________
set /p exampleChoice="Do you want to remove .example extensions from all files? ([Y/y]/N/n): "

:: Set default value "N" if no input was made
if "%exampleChoice%"=="" set exampleChoice=N

if /i "%exampleChoice%"=="Y" (
for /r %%f in (*.example) do (
    set "newname=%%~dpnf"
    >nul 2>&1 ren "%%f" "%%~nf")
)
@REM echo _______________________________________________________________________________________________________________________
@REM echo Adjust configuration files for mariadb
@REM echo _______________________________________________________________________________________________________________________
@REM todo:
@REM In the D:\opensimsource\bin\config-include\GridCommon.ini change the following section:
@REM [DatabaseService]
@REM     Include-Storage = "config-include/storage/SQLiteStandalone.ini"; change to: StorageProvider = "OpenSim.Data.MySQL.dll"
@REM     Remove the semicolon before the line.
@REM     ConnectionString = "Data Source=localhost;Database=opensim;User ID=opensim;Password=***;Old Guids=true;SslMode=None;"
@REM In the opensimsource\bin\Robust.ini and Robust.HG.ini change the following section:
@REM [DatabaseService]
@REM     StorageProvider = "OpenSim.Data.MySQL.dll"
@REM     ConnectionString = "Data Source=localhost;Database=opensim;User ID=opensim;Password=*****;Old Guids=true;SslMode=None;"

echo _______________________________________________________________________________________________________________________
echo Create test region
echo _______________________________________________________________________________________________________________________
set /p RegionChoice="Do you want to create a test region? ([Y/y]/N/n): "

:: Set default value "N" if no input was made
if "%RegionChoice%"=="" set RegionChoice=N

if /i "%RegionChoice%"=="Y" (
:: Target path to file
cd "%WORKDIR%opensimsource"
git clone https://github.com/ManfredAabye/OpenSim-Terrain.git OpenSim-Terrain
cd "%WORKDIR%"
if not exist "%WORKDIR%opensimsource\bin\Regions" mkdir "%WORKDIR%opensimsource\bin\Regions"
xcopy /s /y /i "%WORKDIR%opensimsource\OpenSim-Terrain\*.raw" "%WORKDIR%opensimsource\bin\"
xcopy /s /y /i "%WORKDIR%opensimsource\OpenSim-Terrain\*.png" "%WORKDIR%opensimsource\bin\"
xcopy /s /y /i "%WORKDIR%opensimsource\OpenSim-Terrain\*.oar" "%WORKDIR%opensimsource\bin\"
xcopy /s /y /i "%WORKDIR%opensimsource\OpenSim-Terrain\*.ini" "%WORKDIR%opensimsource\bin\Regions\"

:: Output success message
echo The region data files have been updated.
)

echo _______________________________________________________________________________________________________________________
echo Copy System.Drawing.Common.dll.win to bin...
echo _______________________________________________________________________________________________________________________
cd "%WORKDIR%opensimsource"
copy bin\System.Drawing.Common.dll.win bin\System.Drawing.Common.dll
cd "%WORKDIR%"
echo _______________________________________________________________________________________________________________________
echo Create the Prebuild files...
echo _______________________________________________________________________________________________________________________
:: Clean first, otherwise duplicate entries from dotnet 8.0
:: If the OpenSim.sln file exists, a clean won't hurt, right?
cd "%WORKDIR%opensimsource"
if exist "OpenSim.sln" (
dotnet bin\prebuild.dll /file prebuild.xml /clean
)

dotnet bin\prebuild.dll /target vs2022 /targetframework net8_0 /excludedir = "obj | bin" /file prebuild.xml

echo _______________________________________________________________________________________________________________________
echo Create the compile.bat...
echo _______________________________________________________________________________________________________________________

@echo Creating compile.bat
@echo dotnet build --configuration Release OpenSim.sln > compile.bat

echo _______________________________________________________________________________________________________________________
set /p compileChoice="Do you want to compile the OpenSimulator now? ([Y/y]/N/n): "

:: Set default value "Y" if no input was made
if "%compileChoice%"=="" set compileChoice=Y

echo _______________________________________________________________________________________________________________________
if /i "%compileChoice%"=="Y" (
    echo Starting compilation of OpenSimulator...
    dotnet build --configuration Release OpenSim.sln
) else (
    echo Compilation skipped.
)
cd "%WORKDIR%"

echo _______________________________________________________________________________________________________________________
echo Back to the main directory...
cd "%WORKDIR%"

echo _______________________________________________________________________________________________________________________
echo Setup completed.
echo _______________________________________________________________________________________________________________________
echo You still need an OpenSim-compatible viewer which you can get here: "https://wiki.firestormviewer.org/downloads"
echo Now start the OpenSimulator with the command "dotnet opensim.dll" or "OpenSim.exe" which is located in the opensimsource/bin directory
echo In the following window:
echo Follow the instructions or press Enter to accept the 2 Estate instructions.
echo Enter "create user" and create a new user with the first name "opensim" and last name "avatar".
echo If the land is underwater, enter: "terrain fill 20.5" then your land is 50cm above the water level.
echo If you want to have a prefabricated land then enter "load oar filename.oar" in the console.
echo Start your Firestorm Viewer and select - Settings - OpenSim - add new grid - 
echo and add "http://127.0.0.1:9000" there or if available select Grid:localhost.
echo With xampp and mariadb the following databases must be created: Database=opensim User=opensim Password=*****
echo Have fun with your OpenSimulator!

pause