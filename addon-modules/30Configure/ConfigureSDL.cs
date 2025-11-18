/*
 * Copyright (c) Diva Configure Team. All rights reserved.
 */

using System;
using System.IO;

namespace Diva.Configure
{
    /// <summary>
    /// Standalone Konfiguration
    /// </summary>
    public static class ConfigureSDL
    {
        public static void Configure(ConfigureSettings settings)
        {
            Console.WriteLine("\n╔═══════════════════════════════════════════════════╗");
            Console.WriteLine("║        Standalone Configuration                   ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════╝");

            string binPath = settings.PathSettings.BinPath;
            string configPath = Path.Combine(binPath, "config-include");
            
            // 1. OpenSim.ini konfigurieren
            ConfigureOpenSimIni(binPath, settings);
            
            // 2. Standalone.ini aktivieren
            ConfigureStandaloneIni(configPath, settings);
            
            // 3. Datenbank konfigurieren
            ConfigureDatabase(configPath, settings);

            Console.WriteLine("\n[SUCCESS] Standalone configuration completed!");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        private static void ConfigureOpenSimIni(string binPath, ConfigureSettings settings)
        {
            string iniPath = Path.Combine(binPath, "OpenSim.ini");
            
            Console.WriteLine($"\n[CONFIG] Processing {iniPath}...");

            // Setze Basis-Einstellungen
            ConfigureCol.SetIniValue(iniPath, "Network", "http_listener_port", 
                settings.StandaloneSettings.HttpPort);
            
            ConfigureCol.SetIniValue(iniPath, "Const", "BaseHostname", 
                settings.GlobalSettings.ExternalHostName);

            Console.WriteLine("[OK] OpenSim.ini configured");
        }

        private static void ConfigureStandaloneIni(string configPath, ConfigureSettings settings)
        {
            string iniPath = Path.Combine(configPath, "Standalone.ini");
            
            Console.WriteLine($"\n[CONFIG] Processing {iniPath}...");

            // Aktiviere Standalone-Module
            ConfigureCol.SetIniValue(iniPath, "Modules", "AssetServices", "LocalAssetServicesConnector");
            ConfigureCol.SetIniValue(iniPath, "Modules", "InventoryServices", "LocalInventoryServicesConnector");
            ConfigureCol.SetIniValue(iniPath, "Modules", "GridServices", "LocalGridServicesConnector");

            Console.WriteLine("[OK] Standalone.ini configured");
        }

        private static void ConfigureDatabase(string configPath, ConfigureSettings settings)
        {
            string dbType = settings.GlobalSettings.DatabaseType;
            string storageIni = dbType.Equals("MySQL", StringComparison.OrdinalIgnoreCase) 
                ? "MySQLStandalone.ini" 
                : "SQLiteStandalone.ini";

            string iniPath = Path.Combine(configPath, "storage", storageIni);
            
            Console.WriteLine($"\n[CONFIG] Processing {iniPath}...");

            if (dbType.Equals("MySQL", StringComparison.OrdinalIgnoreCase))
            {
                ConfigureCol.SetIniValue(iniPath, "DatabaseService", "ConnectionString", 
                    settings.GlobalSettings.MySQLConnectionString);
            }

            Console.WriteLine($"[OK] Database ({dbType}) configured");
        }
    }
}
