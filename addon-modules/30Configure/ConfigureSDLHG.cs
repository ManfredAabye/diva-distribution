/*
 * Copyright (c) Diva Configure Team. All rights reserved.
 */

using System;
using System.IO;

namespace Diva.Configure
{
    /// <summary>
    /// StandaloneHG Konfiguration (Hypergrid)
    /// </summary>
    public static class ConfigureSDLHG
    {
        public static void Configure(ConfigureSettings settings)
        {
            Console.WriteLine("\n╔═══════════════════════════════════════════════════╗");
            Console.WriteLine("║      StandaloneHG Configuration (Hypergrid)       ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════╝");

            string binPath = settings.PathSettings.BinPath;
            string configPath = Path.Combine(binPath, "config-include");
            
            // 1. OpenSim.ini konfigurieren
            ConfigureOpenSimIni(binPath, settings);
            
            // 2. StandaloneHypergrid.ini aktivieren
            ConfigureStandaloneHGIni(configPath, settings);
            
            // 3. Datenbank konfigurieren
            ConfigureDatabase(configPath, settings);
            
            // 4. Hypergrid-spezifische Einstellungen
            ConfigureHypergrid(configPath, settings);

            Console.WriteLine("\n[SUCCESS] StandaloneHG configuration completed!");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        private static void ConfigureOpenSimIni(string binPath, ConfigureSettings settings)
        {
            string iniPath = Path.Combine(binPath, "OpenSim.ini");
            Console.WriteLine($"\n[CONFIG] Processing {iniPath}...");

            ConfigureCol.SetIniValue(iniPath, "Network", "http_listener_port", 
                settings.StandaloneSettings.HttpPort);
            
            ConfigureCol.SetIniValue(iniPath, "Const", "BaseHostname", 
                settings.GlobalSettings.ExternalHostName);

            Console.WriteLine("[OK] OpenSim.ini configured");
        }

        private static void ConfigureStandaloneHGIni(string configPath, ConfigureSettings settings)
        {
            string iniPath = Path.Combine(configPath, "StandaloneHypergrid.ini");
            Console.WriteLine($"\n[CONFIG] Processing {iniPath}...");

            // Aktiviere StandaloneHG-Module
            ConfigureCol.SetIniValue(iniPath, "Modules", "AssetServices", "HGAssetBroker");
            ConfigureCol.SetIniValue(iniPath, "Modules", "InventoryServices", "HGInventoryBroker");
            
            Console.WriteLine("[OK] StandaloneHypergrid.ini configured");
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

        private static void ConfigureHypergrid(string configPath, ConfigureSettings settings)
        {
            string iniPath = Path.Combine(configPath, "StandaloneHypergrid.ini");
            Console.WriteLine($"\n[CONFIG] Configuring Hypergrid settings...");

            string gatekeeperURI = $"http://{settings.GlobalSettings.ExternalHostName}:{settings.StandaloneSettings.HttpPort}";
            
            ConfigureCol.SetIniValue(iniPath, "GridService", "GatekeeperURI", gatekeeperURI);
            ConfigureCol.SetIniValue(iniPath, "GridService", "GridName", settings.GlobalSettings.GridName);

            Console.WriteLine("[OK] Hypergrid settings configured");
        }
    }
}
