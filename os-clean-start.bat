@echo off

echo ====================================
echo OpenSimulator Diva Distribution
echo .NET 8 Edition
echo ====================================
echo.

cd bin
:: Delete OpenSim.log
del *.log
:: Delete OpenSim sqlite database files
del *.db
del *.db-wal
del *.db-shm
:: Run OpenSim
dotnet OpenSim.dll