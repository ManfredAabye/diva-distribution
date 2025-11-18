/*
 * Copyright (c) Diva Configure Team. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Diva.Configure
{
    /// <summary>
    /// Hauptprogramm für OpenSim Konfigurationsverwaltung
    /// Main program for OpenSim configuration management
    /// </summary>
    public partial class Configure
    {
        private static ConfigureSettings _settings;
        private const string ConfigFile = "Configure.json";

        public static void Main(string[] args)
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════╗");
            Console.WriteLine("║     Diva Configure - OpenSim Configuration       ║");
            Console.WriteLine("║              Management System                    ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════╝");
            Console.WriteLine();

            // Lade Konfiguration
            LoadSettings();

            if (args.Length > 0)
            {
                // Command-line Modus
                ProcessCommand(args);
            }
            else
            {
                // Interaktiver Modus
                RunInteractiveMode();
            }
        }

        private static void LoadSettings()
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFile);
            _settings = ConfigureCol.ReadJsonConfig<ConfigureSettings>(configPath);

            if (_settings == null)
            {
                Console.WriteLine($"[WARNING] Could not load {ConfigFile}, using defaults.");
                _settings = new ConfigureSettings();
            }
        }

        private static void RunInteractiveMode()
        {
            bool running = true;

            while (running)
            {
                Console.WriteLine("\n┌─────────────────────────────────────────────────┐");
                Console.WriteLine("│           Hauptmenü / Main Menu                 │");
                Console.WriteLine("├─────────────────────────────────────────────────┤");
                Console.WriteLine("│ 1. Standalone Configuration                     │");
                Console.WriteLine("│ 2. StandaloneHG Configuration                   │");
                Console.WriteLine("│ 3. Robust Configuration                         │");
                Console.WriteLine("│ 4. RobustHG Configuration                       │");
                Console.WriteLine("│ 5. Robust + Regions Configuration               │");
                Console.WriteLine("│ 6. RobustHG + Regions Configuration             │");
                Console.WriteLine("│                                                  │");
                Console.WriteLine("│ 7. Region Management (Create/Edit/Delete)       │");
                Console.WriteLine("│ 8. Global Settings (IP, Database, etc.)         │");
                Console.WriteLine("│ 9. Backup Management                            │");
                Console.WriteLine("│                                                  │");
                Console.WriteLine("│ 0. Exit / Beenden                               │");
                Console.WriteLine("└─────────────────────────────────────────────────┘");
                Console.Write("\nWählen Sie eine Option / Choose option: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ConfigureSDL.Configure(_settings);
                        break;
                    case "2":
                        ConfigureSDLHG.Configure(_settings);
                        break;
                    case "3":
                        ConfigureRO.Configure(_settings);
                        break;
                    case "4":
                        ConfigureROHG.Configure(_settings);
                        break;
                    case "5":
                        ConfigureROR.Configure(_settings);
                        break;
                    case "6":
                        ConfigureROHGR.Configure(_settings);
                        break;
                    case "7":
                        RegionManagementMenu(_settings);
                        break;
                    case "8":
                        ConfigureGlobalSettings();
                        break;
                    case "9":
                        ManageBackups();
                        break;
                    case "0":
                        running = false;
                        Console.WriteLine("\nAuf Wiedersehen! / Goodbye!");
                        break;
                    default:
                        Console.WriteLine("\n[ERROR] Ungültige Auswahl / Invalid choice!");
                        break;
                }
            }
        }

        private static void ProcessCommand(string[] args)
        {
            string command = args[0].ToLower();

            switch (command)
            {
                case "standalone":
                case "sdl":
                    ConfigureSDL.Configure(_settings);
                    break;
                case "standalonehg":
                case "sdlhg":
                    ConfigureSDLHG.Configure(_settings);
                    break;
                case "robust":
                case "ro":
                    ConfigureRO.Configure(_settings);
                    break;
                case "robusthg":
                case "rohg":
                    ConfigureROHG.Configure(_settings);
                    break;
                case "robust-regions":
                case "ror":
                    ConfigureROR.Configure(_settings);
                    break;
                case "robusthg-regions":
                case "rohgr":
                    ConfigureROHGR.Configure(_settings);
                    break;
                case "backup":
                    ManageBackups();
                    break;
                case "help":
                case "-h":
                case "--help":
                    ShowHelp();
                    break;
                default:
                    Console.WriteLine($"[ERROR] Unknown command: {command}");
                    ShowHelp();
                    break;
            }
        }

        private static void ConfigureGlobalSettings()
        {
            Console.WriteLine("\n┌─────────────────────────────────────────────────┐");
            Console.WriteLine("│         Global Settings / Globale Einstellungen │");
            Console.WriteLine("└─────────────────────────────────────────────────┘");

            Console.Write($"\nBase IP [{_settings.GlobalSettings.BaseIP}]: ");
            string ip = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(ip) && ConfigureCol.IsValidIP(ip))
                _settings.GlobalSettings.BaseIP = ip;

            Console.Write($"\nExternal HostName [{_settings.GlobalSettings.ExternalHostName}]: ");
            string hostname = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(hostname))
                _settings.GlobalSettings.ExternalHostName = hostname;

            Console.Write($"\nDatabase Type (SQLite/MySQL) [{_settings.GlobalSettings.DatabaseType}]: ");
            string dbType = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(dbType))
                _settings.GlobalSettings.DatabaseType = dbType;

            if (_settings.GlobalSettings.DatabaseType.Equals("MySQL", StringComparison.OrdinalIgnoreCase))
            {
                Console.Write($"\nMySQL Connection String [{_settings.GlobalSettings.MySQLConnectionString}]: ");
                string connStr = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(connStr))
                    _settings.GlobalSettings.MySQLConnectionString = connStr;
            }

            Console.Write($"\nGrid Name [{_settings.GlobalSettings.GridName}]: ");
            string gridName = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(gridName))
                _settings.GlobalSettings.GridName = gridName;

            Console.Write($"\nGrid Nick [{_settings.GlobalSettings.GridNick}]: ");
            string gridNick = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(gridNick))
                _settings.GlobalSettings.GridNick = gridNick;

            // Speichere Einstellungen
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFile);
            if (ConfigureCol.WriteJsonConfig(configPath, _settings))
            {
                Console.WriteLine("\n[SUCCESS] Global settings saved!");
            }
        }

        private static void ManageBackups()
        {
            Console.WriteLine("\n┌─────────────────────────────────────────────────┐");
            Console.WriteLine("│           Backup Management                      │");
            Console.WriteLine("└─────────────────────────────────────────────────┘");

            string backupDir = _settings.PathSettings.BackupPath;

            Console.WriteLine($"\n1. Create full configuration backup");
            Console.WriteLine($"2. List existing backups");
            Console.WriteLine($"3. Back to main menu");
            Console.Write("\nChoice: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    CreateFullBackup(backupDir);
                    break;
                case "2":
                    ListBackups(backupDir);
                    break;
            }
        }

        private static void CreateFullBackup(string backupDir)
        {
            Console.WriteLine("\n[BACKUP] Creating full configuration backup...");

            ConfigureCol.EnsureDirectory(backupDir);

            string[] dirsToBackup = { 
                _settings.PathSettings.BinPath,
                _settings.PathSettings.ConfigIncludePath,
                _settings.PathSettings.RegionsPath
            };

            foreach (var dir in dirsToBackup)
            {
                if (Directory.Exists(dir))
                {
                    ConfigureCol.BackupDirectory(dir, backupDir);
                }
            }

            Console.WriteLine("\n[SUCCESS] Full backup completed!");
        }

        private static void ListBackups(string backupDir)
        {
            if (!Directory.Exists(backupDir))
            {
                Console.WriteLine("\n[INFO] No backups found.");
                return;
            }

            var backups = Directory.GetDirectories(backupDir);
            if (backups.Length == 0)
            {
                Console.WriteLine("\n[INFO] No backups found.");
                return;
            }

            Console.WriteLine("\n┌─────────────────────────────────────────────────┐");
            Console.WriteLine("│           Available Backups                      │");
            Console.WriteLine("└─────────────────────────────────────────────────┘");

            foreach (var backup in backups)
            {
                var dirInfo = new DirectoryInfo(backup);
                Console.WriteLine($"  • {dirInfo.Name} - {dirInfo.CreationTime}");
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine("\nDiva Configure - Command Line Usage:");
            Console.WriteLine("  Configure.exe                    - Interactive mode");
            Console.WriteLine("  Configure.exe standalone|sdl     - Configure Standalone");
            Console.WriteLine("  Configure.exe standalonehg|sdlhg - Configure StandaloneHG");
            Console.WriteLine("  Configure.exe robust|ro          - Configure Robust");
            Console.WriteLine("  Configure.exe robusthg|rohg      - Configure RobustHG");
            Console.WriteLine("  Configure.exe robust-regions|ror - Configure Robust + Regions");
            Console.WriteLine("  Configure.exe robusthg-regions|rohgr - Configure RobustHG + Regions");
            Console.WriteLine("  Configure.exe backup             - Backup management");
            Console.WriteLine("  Configure.exe help               - Show this help");
        }
    }

    #region Configuration Classes

    public class ConfigureSettings
    {
        public GlobalSettings GlobalSettings { get; set; } = new GlobalSettings();
        public StandaloneSettings StandaloneSettings { get; set; } = new StandaloneSettings();
        public RobustSettings RobustSettings { get; set; } = new RobustSettings();
        public Dictionary<string, RegionSettings> RegionSettings { get; set; } = new Dictionary<string, RegionSettings>();
        public PathSettings PathSettings { get; set; } = new PathSettings();
    }

    public class GlobalSettings
    {
        public string BaseIP { get; set; } = "127.0.0.1";
        public string ExternalHostName { get; set; } = "SYSTEMIP";
        public string DatabaseType { get; set; } = "SQLite";
        public string MySQLConnectionString { get; set; } = "Data Source=localhost;Database=opensim;User ID=opensim;Password=***;Old Guids=true;";
        public string GridName { get; set; } = "My OpenSim Grid";
        public string GridNick { get; set; } = "MyGrid";
        public string WelcomeMessage { get; set; } = "Welcome to OpenSimulator";
        public string Economy { get; set; } = "false";
    }

    public class StandaloneSettings
    {
        public string HttpPort { get; set; } = "9000";
        public string InternalPort { get; set; } = "8003";
    }

    public class RobustSettings
    {
        public string PublicPort { get; set; } = "8002";
        public string PrivatePort { get; set; } = "8003";
        public string GridServerURI { get; set; } = "http://127.0.0.1:8002";
        public string GatekeeperURI { get; set; } = "http://127.0.0.1:8002";
    }

    public class RegionSettings
    {
        public string RegionName { get; set; }
        public string RegionUUID { get; set; }
        public string Location { get; set; }
        public string InternalPort { get; set; }
        public string ExternalHostName { get; set; }
        public string MaxPrims { get; set; } = "45000";
        public string MaxAgents { get; set; } = "100";
    }

    /// <summary>
    /// Region Datenstruktur
    /// </summary>
    public class RegionData
    {
        public string RegionName { get; set; }
        public string RegionUUID { get; set; }
        public string Location { get; set; }
        public string InternalPort { get; set; }
        public string ExternalHostName { get; set; }
        public string SizeX { get; set; } = "256";
        public string SizeY { get; set; } = "256";
        public string MaxPrims { get; set; } = "45000";
        public string MaxAgents { get; set; } = "100";
    }

    public class PathSettings
    {
        public string BinPath { get; set; } = "bin";
        public string ConfigIncludePath { get; set; } = "bin/config-include";
        public string RegionsPath { get; set; } = "bin/Regions";
        public string BackupPath { get; set; } = "backups";
    }

    #endregion

    #region Configure Extensions

    partial class Configure
    {
        #region Region Management Menu

        private static void RegionManagementMenu(ConfigureSettings settings)
        {
            bool running = true;

            while (running)
            {
                Console.WriteLine("\n┌─────────────────────────────────────────────────┐");
                Console.WriteLine("│          Region Management Menu                  │");
                Console.WriteLine("├─────────────────────────────────────────────────┤");
                Console.WriteLine("│ 1. Create new region                             │");
                Console.WriteLine("│ 2. Edit existing region                          │");
                Console.WriteLine("│ 3. List all regions                              │");
                Console.WriteLine("│ 4. Delete region                                 │");
                Console.WriteLine("│ 5. Clone region                                  │");
                Console.WriteLine("│                                                  │");
                Console.WriteLine("│ 0. Back to main menu                             │");
                Console.WriteLine("└─────────────────────────────────────────────────┘");
                Console.Write("\nChoose option: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        CreateNewRegion(settings);
                        break;
                    case "2":
                        EditExistingRegion(settings);
                        break;
                    case "3":
                        ConfigureCol.ListAllRegions(settings.PathSettings.RegionsPath);
                        Console.WriteLine("\nPress any key to continue...");
                        Console.ReadKey();
                        break;
                    case "4":
                        DeleteRegion(settings);
                        break;
                    case "5":
                        CloneRegion(settings);
                        break;
                    case "0":
                        running = false;
                        break;
                    default:
                        Console.WriteLine("\n[ERROR] Invalid choice!");
                        break;
                }
            }
        }

        private static void CreateNewRegion(ConfigureSettings settings)
        {
            var regionData = ConfigureCol.CreateRegionInteractive(settings);
            if (regionData == null)
                return;

            // Generiere Dateinamen
            string fileName = ConfigureCol.GenerateRegionFileName(regionData.RegionName);
            string regionsPath = settings.PathSettings.RegionsPath;
            string filePath = Path.Combine(regionsPath, fileName);

            // Prüfe ob Region bereits existiert
            if (File.Exists(filePath))
            {
                Console.WriteLine($"[ERROR] Region file already exists: {fileName}");
                Console.Write("Overwrite? (y/n): ");
                if (Console.ReadLine()?.ToLower() != "y")
                    return;
            }

            // Erstelle Region
            if (ConfigureCol.CreateOrUpdateRegionFile(filePath, regionData, true))
            {
                Console.WriteLine($"\n[SUCCESS] Region created: {fileName}");
                Console.WriteLine($"  Name: {regionData.RegionName}");
                Console.WriteLine($"  UUID: {regionData.RegionUUID}");
                Console.WriteLine($"  Location: {regionData.Location}");
                Console.WriteLine($"  Port: {regionData.InternalPort}");
            }
            else
            {
                Console.WriteLine("[ERROR] Failed to create region file!");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        private static void EditExistingRegion(ConfigureSettings settings)
        {
            string regionsPath = settings.PathSettings.RegionsPath;
            var regionFiles = ConfigureCol.GetAllRegionFiles(regionsPath);

            if (regionFiles.Count == 0)
            {
                Console.WriteLine("\n[INFO] No region files found.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            // Liste Regionen
            Console.WriteLine("\nAvailable regions:");
            for (int i = 0; i < regionFiles.Count; i++)
            {
                string fileName = Path.GetFileName(regionFiles[i]);
                string regionName = ConfigureCol.GetIniValue(regionFiles[i], "Region", "RegionName") ?? "Unknown";
                Console.WriteLine($"  {i + 1}. {fileName} - {regionName}");
            }

            Console.Write("\nSelect region number (0 to cancel): ");
            if (!int.TryParse(Console.ReadLine(), out int selection) || selection < 1 || selection > regionFiles.Count)
            {
                if (selection != 0)
                    Console.WriteLine("[ERROR] Invalid selection!");
                return;
            }

            string selectedFile = regionFiles[selection - 1];
            var regionData = ConfigureCol.ReadRegionFromFile(selectedFile);

            if (regionData == null)
            {
                Console.WriteLine("[ERROR] Could not read region file!");
                return;
            }

            // Bearbeite Region
            regionData = ConfigureCol.EditRegionInteractive(regionData);

            // Speichere Änderungen
            if (ConfigureCol.CreateOrUpdateRegionFile(selectedFile, regionData, true))
            {
                Console.WriteLine($"\n[SUCCESS] Region updated: {Path.GetFileName(selectedFile)}");
            }
            else
            {
                Console.WriteLine("[ERROR] Failed to update region!");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        private static void DeleteRegion(ConfigureSettings settings)
        {
            string regionsPath = settings.PathSettings.RegionsPath;
            var regionFiles = ConfigureCol.GetAllRegionFiles(regionsPath);

            if (regionFiles.Count == 0)
            {
                Console.WriteLine("\n[INFO] No region files found.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            // Liste Regionen
            Console.WriteLine("\nAvailable regions:");
            for (int i = 0; i < regionFiles.Count; i++)
            {
                string fileName = Path.GetFileName(regionFiles[i]);
                string regionName = ConfigureCol.GetIniValue(regionFiles[i], "Region", "RegionName") ?? "Unknown";
                Console.WriteLine($"  {i + 1}. {fileName} - {regionName}");
            }

            Console.Write("\nSelect region number to delete (0 to cancel): ");
            if (!int.TryParse(Console.ReadLine(), out int selection) || selection < 1 || selection > regionFiles.Count)
            {
                if (selection != 0)
                    Console.WriteLine("[ERROR] Invalid selection!");
                return;
            }

            string selectedFile = regionFiles[selection - 1];
            string selectedName = ConfigureCol.GetIniValue(selectedFile, "Region", "RegionName") ?? Path.GetFileName(selectedFile);

            Console.Write($"\n[WARNING] Delete region '{selectedName}'? (yes/no): ");
            string confirm = Console.ReadLine()?.ToLower();

            if (confirm == "yes")
            {
                if (ConfigureCol.DeleteRegionFile(selectedFile, true))
                {
                    Console.WriteLine($"[SUCCESS] Region deleted: {selectedName}");
                }
                else
                {
                    Console.WriteLine("[ERROR] Failed to delete region!");
                }
            }
            else
            {
                Console.WriteLine("[INFO] Deletion cancelled.");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        private static void CloneRegion(ConfigureSettings settings)
        {
            string regionsPath = settings.PathSettings.RegionsPath;
            var regionFiles = ConfigureCol.GetAllRegionFiles(regionsPath);

            if (regionFiles.Count == 0)
            {
                Console.WriteLine("\n[INFO] No region files found.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            // Liste Regionen
            Console.WriteLine("\nAvailable regions:");
            for (int i = 0; i < regionFiles.Count; i++)
            {
                string selectedFileName = Path.GetFileName(regionFiles[i]);
                string regionName = ConfigureCol.GetIniValue(regionFiles[i], "Region", "RegionName") ?? "Unknown";
                Console.WriteLine($"  {i + 1}. {selectedFileName} - {regionName}");
            }

            Console.Write("\nSelect region number to clone (0 to cancel): ");
            if (!int.TryParse(Console.ReadLine(), out int selection) || selection < 1 || selection > regionFiles.Count)
            {
                if (selection != 0)
                    Console.WriteLine("[ERROR] Invalid selection!");
                return;
            }

            string sourceFile = regionFiles[selection - 1];
            var sourceRegion = ConfigureCol.ReadRegionFromFile(sourceFile);

            if (sourceRegion == null)
            {
                Console.WriteLine("[ERROR] Could not read source region!");
                return;
            }

            // Neue Region Daten
            Console.Write($"\nNew region name [{sourceRegion.RegionName} Copy]: ");
            string newName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(newName))
                newName = sourceRegion.RegionName + " Copy";

            Console.Write($"New location (X,Y) [{sourceRegion.Location}]: ");
            string newLocation = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(newLocation))
                newLocation = null;

            Console.Write($"New port [{sourceRegion.InternalPort}]: ");
            string newPort = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(newPort) || !ConfigureCol.IsValidPort(newPort))
                newPort = null;

            // Klone Region
            var clonedRegion = ConfigureCol.CloneRegion(sourceRegion, newName, newLocation, newPort);
            Console.WriteLine($"Generated new UUID: {clonedRegion.RegionUUID}");

            // Erstelle geklonte Region
            string fileName = ConfigureCol.GenerateRegionFileName(clonedRegion.RegionName);
            string newFilePath = Path.Combine(regionsPath, fileName);

            if (ConfigureCol.CreateOrUpdateRegionFile(newFilePath, clonedRegion, true))
            {
                Console.WriteLine($"\n[SUCCESS] Region cloned: {fileName}");
            }
            else
            {
                Console.WriteLine("[ERROR] Failed to clone region!");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        #endregion
    }

    #endregion
}
