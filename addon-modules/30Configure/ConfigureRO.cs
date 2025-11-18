/*
 * Copyright (c) Diva Configure Team. All rights reserved.
 */

using System;
using System.IO;

namespace Diva.Configure
{
    /// <summary>
    /// Robust Grid Server Konfiguration
    /// </summary>
    public static class ConfigureRO
    {
        public static void Configure(ConfigureSettings settings)
        {
            Console.WriteLine("\n╔═══════════════════════════════════════════════════╗");
            Console.WriteLine("║         Robust Grid Configuration                 ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════╝");

            string binPath = settings.PathSettings.BinPath;
            
            // 1. Robust.ini konfigurieren
            ConfigureRobustIni(binPath, settings);
            
            // 2. Grid.ini aktivieren
            ConfigureGridIni(Path.Combine(binPath, "config-include"), settings);
            
            // 3. Datenbank konfigurieren
            ConfigureDatabase(binPath, settings);

            Console.WriteLine("\n[SUCCESS] Robust Grid configuration completed!");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        private static void ConfigureRobustIni(string binPath, ConfigureSettings settings)
        {
            string iniPath = Path.Combine(binPath, "Robust.ini");
            Console.WriteLine($"\n[CONFIG] Processing {iniPath}...");

            ConfigureCol.SetIniValue(iniPath, "Const", "BaseHostname", 
                settings.GlobalSettings.BaseIP);
            ConfigureCol.SetIniValue(iniPath, "Const", "PublicPort", 
                settings.RobustSettings.PublicPort);
            ConfigureCol.SetIniValue(iniPath, "Const", "PrivatePort", 
                settings.RobustSettings.PrivatePort);

            ConfigureCol.SetIniValue(iniPath, "GridService", "GridName", 
                settings.GlobalSettings.GridName);

            Console.WriteLine("[OK] Robust.ini configured");
        }

        private static void ConfigureGridIni(string configPath, ConfigureSettings settings)
        {
            string iniPath = Path.Combine(configPath, "Grid.ini");
            Console.WriteLine($"\n[CONFIG] Processing {iniPath}...");

            string gridServerURI = settings.RobustSettings.GridServerURI;
            
            ConfigureCol.SetIniValue(iniPath, "Modules", "GridServices", "RemoteGridServicesConnector");
            ConfigureCol.SetIniValue(iniPath, "GridService", "GridServerURI", gridServerURI);

            Console.WriteLine("[OK] Grid.ini configured");
        }

        private static void ConfigureDatabase(string binPath, ConfigureSettings settings)
        {
            string iniPath = Path.Combine(binPath, "Robust.ini");
            string dbType = settings.GlobalSettings.DatabaseType;
            
            Console.WriteLine($"\n[CONFIG] Configuring {dbType} database...");

            if (dbType.Equals("MySQL", StringComparison.OrdinalIgnoreCase))
            {
                ConfigureCol.SetIniValue(iniPath, "DatabaseService", "StorageProvider", 
                    "OpenSim.Data.MySQL.dll");
                ConfigureCol.SetIniValue(iniPath, "DatabaseService", "ConnectionString", 
                    settings.GlobalSettings.MySQLConnectionString);
            }
            else
            {
                ConfigureCol.SetIniValue(iniPath, "DatabaseService", "StorageProvider", 
                    "OpenSim.Data.SQLite.dll");
            }

            Console.WriteLine($"[OK] Database ({dbType}) configured");
        }
    }
}
