@echo off

echo ====================================
echo OpenSimulator Diva Distribution
echo .NET 8 Edition
echo ====================================
echo.

cd bin
:: Delete OpenSim.log
del *.log
@REM del *.db
@REM del *.db-wal
@REM del *.db-shm
:: Run OpenSim
dotnet Configure.dll