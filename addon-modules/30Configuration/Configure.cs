using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using OpenMetaverse;

#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality

namespace MetaverseInk.Configuration
{
    public class Configure
    {
        /*
        1. Tool starten
        2. Benutzereingaben sammeln
        3. Hauptbackup erstellen (wenn aktiviert)
        ├─ ZIP-Archiv erstellen
        └─ Individuelle .bak Dateien
        4. Für jede Konfigurationsdatei:
        ├─ Einzeldatei-Backup erstellen
        ├─ .example Datei lesen
        ├─ Werte ersetzen
        └─ Neue .ini Datei schreiben
        5. Zusammenfassung anzeigen

        Hauptverzeichnis /bin
        OpenSim.ini
        Robust.ini
        Robust.HG.ini
        Wifi.ini

        Konfigurationsverzeichnis /bin/config-include
        DivaPreferences.ini
        GridCommon.ini
        MyWorld.ini
        StandaloneCommon.ini
        StandaloneHypergrid.ini

        Konfigurationsverzeichnis /bin/Regions
        Regions.ini
        */

        private static ConfigurationSettings _settings;
        private static readonly string ConfigFile = "ConfigureSettings.json";
        private static readonly string BackupDirectory = "config-backups";
        private static bool myWorldReconfig = false;

        private enum RegionConfigStatus : uint
        {
            OK = 0,
            NeedsCreation = 1,
            NeedsEditing = 2
        }

        public static void Main(string[] args)
        {
            DisplayBanner();
            
            try
            {
                LoadConfiguration();
                
                if (args.Length > 0)
                {
                    HandleCommandLineArgs(args);
                }
                else
                {
                    InteractiveConfiguration();
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ Critical Error: {e.Message}");
                Console.ResetColor();
                if (_settings?.Debug == true)
                {
                    Console.WriteLine($"\nStack Trace:\n{e.StackTrace}");
                }
            }
            
            Console.WriteLine("\n<Press return to exit>");
            Console.ReadLine();
        }

        private static void DisplayBanner()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║    Diva Distribution Configuration Tool for .NET 8          ║");
            Console.WriteLine("║    Version 1.0.0                                             ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine($"Runtime: .NET {Environment.Version}");
            Console.WriteLine($"Current Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");
        }

        private static void LoadConfiguration()
        {
            try
            {
                if (File.Exists(ConfigFile))
                {
                    string json = File.ReadAllText(ConfigFile);
                    _settings = JsonSerializer.Deserialize<ConfigurationSettings>(json);
                    
                    // Ensure Architecture property is set (for backward compatibility)
                    if (_settings.Architecture == 0)
                    {
                        _settings.Architecture = ArchitectureType.Standalone;
                    }
                    
                    // Ensure Region1Name is set
                    if (string.IsNullOrEmpty(_settings.Region1Name))
                    {
                        _settings.Region1Name = _settings.WorldName;
                    }
                    
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"✓ Configuration loaded from {ConfigFile}");
                    Console.WriteLine($"  World: {_settings.WorldName}");
                    Console.WriteLine($"  Architecture: {_settings.Architecture}");
                    Console.ResetColor();
                }
                else
                {
                    // Load from architecture-specific default settings
                    LoadArchitectureDefaultSettings();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Error loading configuration: {ex.Message}");
                Console.ResetColor();
                LoadArchitectureDefaultSettings();
            }
        }

        private static void LoadArchitectureDefaultSettings()
        {
            // Default to Standalone (local grid without Hypergrid)
            string defaultSettingsFile = "StandaloneSettings.json";
            
            try
            {
                if (File.Exists(defaultSettingsFile))
                {
                    string json = File.ReadAllText(defaultSettingsFile);
                    _settings = JsonSerializer.Deserialize<ConfigurationSettings>(json);
                    
                    // Auto-detect external IP if not set
                    if (_settings.IpAddress == "127.0.0.1" || string.IsNullOrEmpty(_settings.IpAddress))
                    {
                        _settings.IpAddress = "127.0.0.1";
                    }
                    
                    // Ensure Region1Name is set
                    if (string.IsNullOrEmpty(_settings.Region1Name))
                    {
                        _settings.Region1Name = _settings.WorldName;
                    }
                    
                    SaveConfiguration();
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"✓ Default configuration loaded from {defaultSettingsFile}");
                    Console.WriteLine($"  Architecture: {_settings.Architecture}");
                    Console.ResetColor();
                }
                else
                {
                    // Fallback: Create minimal default configuration
                    _settings = new ConfigurationSettings
                    {
                        WorldName = "My World",
                        DbType = "SQLite",
                        DbHost = "localhost",
                        DbSchema = "opensim",
                        DbUser = "opensim",
                        DbPassword = "secret",
                        AdminFirstName = "Wifi",
                        AdminLastName = "Administrator",
                        AdminPassword = "secret",
                        AdminEmail = "admin@localhost",
                        IpAddress = "127.0.0.1",
                        HttpPort = 9000,
                        BaseLocationX = 1000,
                        BaseLocationY = 1000,
                        RegionSizeX = 512,
                        RegionSizeY = 512,
                        RegionSizeZ = 512,
                        GmailAccount = string.Empty,
                        GmailPassword = string.Empty,
                        AutoBackup = true,
                        Debug = false,
                        Architecture = ArchitectureType.Standalone,
                        Region1Name = "Welcome"
                    };
                    SaveConfiguration();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("⚠ No default settings found. Created fallback configuration.");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Error loading default settings: {ex.Message}");
                Console.ResetColor();
                _settings = new ConfigurationSettings();
            }
        }

        private static void SaveConfiguration()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_settings, options);
                File.WriteAllText(ConfigFile, json);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ Configuration saved");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Error saving configuration: {ex.Message}");
                Console.ResetColor();
            }
        }

        private static void HandleCommandLineArgs(string[] args)
        {
            string command = args[0].ToLower();
            
            switch (command)
            {
                case "--auto":
                case "-a":
                    AutoConfigure();
                    break;
                case "--validate":
                case "-v":
                    ValidateConfiguration();
                    break;
                case "--backup":
                case "-b":
                    CreateConfigBackup();
                    break;
                case "--restore":
                case "-r":
                    RestoreFromBackup();
                    break;
                case "--help":
                case "-h":
                    DisplayHelp();
                    break;
                default:
                    Console.WriteLine($"Unknown command: {command}");
                    DisplayHelp();
                    break;
            }
        }

        private static void InteractiveConfiguration()
        {
            Console.WriteLine("\n╔════════════════════════════════════╗");
            Console.WriteLine("║   Interactive Configuration        ║");
            Console.WriteLine("╚════════════════════════════════════╝\n");
            
            GetUserInput();
            
            if (_settings.AutoBackup)
            {
                Console.WriteLine("\n→ Creating automatic backup...");
                CreateConfigBackup();
            }
            
            ConfigureAllFiles();
            ApplyEngineSettings(_settings);
            DisplayInfo();
        }

        private static void AutoConfigure()
        {
            Console.WriteLine("\n╔════════════════════════════════════╗");
            Console.WriteLine("║   Automatic Configuration          ║");
            Console.WriteLine("╚════════════════════════════════════╝\n");
            
            Console.WriteLine("→ Using default settings from configuration file");
            
            if (_settings.AutoBackup)
            {
                CreateConfigBackup();
            }
            
            ConfigureAllFiles();
            ApplyEngineSettings(_settings);
            DisplayInfo();
        }
        
        private static void ConfigureAllFiles()
        {
            // Configure all configuration files based on selected architecture
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n→ Configuring for Architecture: {_settings.Architecture}");
            Console.ResetColor();
            
            // Always configure these files regardless of architecture
            ConfigureRegions();
            ConfigureMyWorld();
            ConfigureOpenSimIni(); // Sets Include-Architecture based on _settings.Architecture
            ConfigureWifiIni();
            ConfigureDivaPreferences();
            ConfigureOsslEnable();
            
            // Configure architecture-specific files
            switch (_settings.Architecture)
            {
                case ArchitectureType.Standalone:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("\n→ Configuring Standalone architecture files...");
                    Console.ResetColor();
                    ConfigureStandaloneCommon();
                    // Standalone.ini is included by OpenSim.ini, no separate configuration needed
                    break;
                    
                case ArchitectureType.StandaloneHypergrid:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("\n→ Configuring Standalone with Hypergrid architecture files...");
                    Console.ResetColor();
                    ConfigureStandaloneCommon();
                    ConfigureStandaloneHypergrid();
                    break;
                    
                case ArchitectureType.Grid:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("\n→ Configuring Grid architecture files...");
                    Console.WriteLine("   (Own grid with separate Robust server)");
                    Console.ResetColor();
                    ConfigureGridCommon();
                    ConfigureRobustIni();
                    break;
                    
                case ArchitectureType.GridHypergrid:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("\n→ Configuring Grid with Hypergrid architecture files...");
                    Console.WriteLine("   (Own grid with separate Robust server and Hypergrid support)");
                    Console.ResetColor();
                    ConfigureGridCommon();
                    ConfigureRobustHGIni();
                    ConfigureGridHypergrid();
                    break;
                    
                default:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"⚠ Unknown architecture: {_settings.Architecture}, using Standalone defaults");
                    Console.ResetColor();
                    ConfigureStandaloneCommon();
                    // Standalone.ini is included by OpenSim.ini, no separate configuration needed
                    break;
            }
        }

        private static void GetUserInput()
        {
            Console.WriteLine("Please provide the following information (press Enter to use default):\n");
            
            // ============================================
            // STEP 1: ARCHITECTURE SELECTION (FIRST!)
            // ============================================
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          STEP 1: Architecture Selection                   ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");
            Console.WriteLine("1. Standalone (offline mode - single server, local grid only)");
            Console.WriteLine("2. Standalone with Hypergrid (online mode - single server with Hypergrid)");
            Console.WriteLine("3. Grid Mode (offline mode - own grid with separate Robust server)");
            Console.WriteLine("4. Grid with Hypergrid (online mode - own grid with Robust server and Hypergrid)");
            
            string currentArch = _settings.Architecture.ToString();
            int defaultChoice = _settings.Architecture == ArchitectureType.Standalone ? 1 :
                                _settings.Architecture == ArchitectureType.StandaloneHypergrid ? 2 :
                                _settings.Architecture == ArchitectureType.Grid ? 3 :
                                _settings.Architecture == ArchitectureType.GridHypergrid ? 4 : 1;
            
            Console.Write($"\nSelect architecture [1-4, default: {defaultChoice}]: ");
            string input = Console.ReadLine();
            
            int archChoice = defaultChoice;
            if (!string.IsNullOrWhiteSpace(input) && int.TryParse(input, out int userChoice))
            {
                archChoice = userChoice;
            }
            
            switch (archChoice)
            {
                case 1:
                    _settings.Architecture = ArchitectureType.Standalone;
                    break;
                case 2:
                    _settings.Architecture = ArchitectureType.StandaloneHypergrid;
                    break;
                case 3:
                    _settings.Architecture = ArchitectureType.Grid;
                    break;
                case 4:
                    _settings.Architecture = ArchitectureType.GridHypergrid;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"⚠ Invalid choice, keeping current: {currentArch}");
                    Console.ResetColor();
                    break;
            }
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Selected Architecture: {_settings.Architecture}");
            Console.ResetColor();

            // ============================================
            // STEP 2: BASIC SETTINGS
            // ============================================
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          STEP 2: Basic Settings                           ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");
            
            // World Name
            Console.Write($"Name of your world [{_settings.WorldName}]: ");
            input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
                _settings.WorldName = input;

            // IP Address
            Console.Write($"Your external IP address or domain name [{_settings.IpAddress}]: ");
            input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
            {
                if (ValidateIPOrDomain(input))
                    _settings.IpAddress = input;
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"⚠ Invalid IP/domain, using default: {_settings.IpAddress}");
                    Console.ResetColor();
                }
            }

            // HTTP Port
            Console.Write($"HTTP Port [{_settings.HttpPort}]: ");
            input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input) && int.TryParse(input, out int port))
                _settings.HttpPort = port;

            // ============================================
            // STEP 3: DATABASE SETTINGS
            // ============================================
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          STEP 3: Database Settings                        ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");
            
            Console.Write($"Database type (MySQL/SQLite) [{_settings.DbType}]: ");
            input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
            {
                input = input.Trim();
                if (input.Equals("MySQL", StringComparison.OrdinalIgnoreCase) || 
                    input.Equals("SQLite", StringComparison.OrdinalIgnoreCase))
                {
                    _settings.DbType = input.Equals("MySQL", StringComparison.OrdinalIgnoreCase) ? "MySQL" : "SQLite";
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"⚠ Invalid database type, using default: {_settings.DbType}");
                    Console.ResetColor();
                }
            }

            // MySQL specific settings
            if (_settings.DbType.Equals("MySQL", StringComparison.OrdinalIgnoreCase))
            {
                Console.Write($"Database host [{_settings.DbHost}]: ");
                input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input))
                    _settings.DbHost = input;

                Console.Write($"Database schema [{_settings.DbSchema}]: ");
                input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input))
                    _settings.DbSchema = input;

                Console.Write($"Database user [{_settings.DbUser}]: ");
                input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input))
                    _settings.DbUser = input;

                Console.Write($"Database password [{new string('*', _settings.DbPassword.Length)}]: ");
                input = ReadPassword();
                if (!string.IsNullOrWhiteSpace(input))
                    _settings.DbPassword = input;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("→ SQLite will use local database files (no additional configuration needed)");
                Console.ResetColor();
            }

            // ============================================
            // STEP 4: WIFI ADMIN SETTINGS
            // ============================================
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          STEP 4: Wifi Administrator Settings             ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");
            
            Console.Write($"Wifi admin first name [{_settings.AdminFirstName}]: ");
            input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
                _settings.AdminFirstName = input;

            Console.Write($"Wifi admin last name [{_settings.AdminLastName}]: ");
            input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
                _settings.AdminLastName = input;

            Console.Write($"Wifi admin password [{new string('*', _settings.AdminPassword.Length)}]: ");
            input = ReadPassword();
            if (!string.IsNullOrWhiteSpace(input))
                _settings.AdminPassword = input;

            Console.Write($"Wifi admin email [{_settings.AdminEmail}]: ");
            input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
            {
                if (ValidateEmail(input))
                    _settings.AdminEmail = input;
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"⚠ Invalid email format, using default: {_settings.AdminEmail}");
                    Console.ResetColor();
                }
            }

            // ============================================
            // STEP 5: OPTIONAL EMAIL SETTINGS
            // ============================================
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          STEP 5: Email Notifications (Optional)          ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");
            
            Console.Write($"Gmail account for notifications (optional) [{_settings.GmailAccount}]: ");
            input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
            {
                _settings.GmailAccount = input;
                
                Console.Write("Gmail password: ");
                string gmailPwd = ReadPassword();
                if (!string.IsNullOrWhiteSpace(gmailPwd))
                    _settings.GmailPassword = gmailPwd;
            }

            // ============================================
            // STEP 6: REGION SETTINGS
            // ============================================
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          STEP 6: Region Settings                          ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");
            
            Console.Write($"First region name [{_settings.WorldName}]: ");
            input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
                _settings.Region1Name = input;
            else
                _settings.Region1Name = _settings.WorldName; // Default to WorldName
            
            Console.Write($"Base location X coordinate [{_settings.BaseLocationX}]: ");
            input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input) && int.TryParse(input, out int locX))
                _settings.BaseLocationX = locX;

            Console.Write($"Base location Y coordinate [{_settings.BaseLocationY}]: ");
            input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input) && int.TryParse(input, out int locY))
                _settings.BaseLocationY = locY;

            Console.Write($"Region size (applies to X, Y, and Z) [{_settings.RegionSizeX}]: ");
            input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input) && int.TryParse(input, out int regionSize))
            {
                _settings.RegionSizeX = regionSize;
                _settings.RegionSizeY = regionSize;
                _settings.RegionSizeZ = regionSize;
            }

            SaveConfiguration();
        }

        private static string ReadPassword()
        {
            StringBuilder password = new StringBuilder();
            ConsoleKeyInfo key;
            
            do
            {
                key = Console.ReadKey(true);
                
                if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password.Remove(password.Length - 1, 1);
                    Console.Write("\b \b");
                }
                else if (key.Key != ConsoleKey.Enter && !char.IsControl(key.KeyChar))
                {
                    password.Append(key.KeyChar);
                    Console.Write("*");
                }
            } while (key.Key != ConsoleKey.Enter);
            
            Console.WriteLine();
            return password.ToString();
        }

        private static bool ValidateIPOrDomain(string input)
        {
            // Check if it's a valid IP address
            if (IPAddress.TryParse(input, out _))
                return true;
            
            // Check if it's a valid domain name
            return Regex.IsMatch(input, @"^[a-zA-Z0-9][a-zA-Z0-9-]{0,61}[a-zA-Z0-9](\.[a-zA-Z]{2,})+$");
        }

        private static bool ValidateEmail(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private static string DetectExternalIP()
        {
            try
            {
                // Try to get the local IP address
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                    {
                        return ip.ToString();
                    }
                }
            }
            catch
            {
                // Fallback to localhost if detection fails
            }
            
            return "127.0.0.1";
        }

        private static string GetConnectionString()
        {
            if (_settings.DbType.Equals("SQLite", StringComparison.OrdinalIgnoreCase))
            {
                // Use optimized SQLite connection string from settings if available
                if (_settings.Database != null && !string.IsNullOrEmpty(_settings.Database.SQLiteConnectionString))
                {
                    return _settings.Database.SQLiteConnectionString;
                }
                // Fallback to ADO.NET format connection string
                return "Data Source=opensim.db;Version=3;UseUTF16Encoding=True";
            }
            else // MySQL
            {
                return $"Data Source={_settings.DbHost};Database={_settings.DbSchema};User ID={_settings.DbUser};Password={_settings.DbPassword};Old Guids=true;Allow Zero Datetime=true;";
            }
        }

        private static RegionConfigStatus CheckRegionConfig()
        {
            if (File.Exists("Regions/Regions.ini"))
            {
                using (TextReader tr = new StreamReader("Regions/Regions.ini"))
                {
                    string line;
                    while ((line = tr.ReadLine()) != null)
                    {
                        if (line.Contains("MasterAvatar"))
                            return RegionConfigStatus.NeedsEditing;
                    }
                }
                return RegionConfigStatus.OK;
            }

            return RegionConfigStatus.NeedsCreation;
        }

        private static void ConfigureRegions()
        {
            Console.WriteLine("\n╔════════════════════════════════════╗");
            Console.WriteLine("║   Configuring Regions...           ║");
            Console.WriteLine("╚════════════════════════════════════╝\n");
            
            RegionConfigStatus status = CheckRegionConfig();

            if (status == RegionConfigStatus.OK)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ Your regions have been preserved.");
                Console.ResetColor();
                return;
            }

            if (status == RegionConfigStatus.NeedsEditing)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ Warning: Master Avatar is obsolete.");
                Console.WriteLine("   Please edit file Regions/Regions.ini and delete all references to MasterAvatar.");
                Console.ResetColor();
                return;
            }

            // else RegionConfigStatus.NeedsCreation
            
            try
            {
                // Generate a clean Regions.ini based on the template structure
                using (TextWriter tw = new StreamWriter("Regions/Regions.ini"))
                {
                    tw.WriteLine($"[{_settings.WorldName}]");
                    tw.WriteLine($"\tLocation = {_settings.BaseLocationX},{_settings.BaseLocationY}");
                    tw.WriteLine($"\tRegionUUID = {UUID.Random()}");
                    tw.WriteLine($"\tSizeX = {_settings.RegionSizeX}");
                    tw.WriteLine($"\tSizeY = {_settings.RegionSizeY}");
                    tw.WriteLine($"\tSizeZ = {_settings.RegionSizeZ}");
                    tw.WriteLine("\tInternalAddress = 0.0.0.0");
                    tw.WriteLine($"\tInternalPort = {_settings.HttpPort + 10}");
                    tw.WriteLine("\tResolveAddress = False");
                    tw.WriteLine($"\tExternalHostName = {_settings.IpAddress}");
                    tw.WriteLine($"\tMaptileStaticUUID = {UUID.Random()}");
                    tw.WriteLine("\tAllowAlternatePorts = False");
                    tw.WriteLine("\t;NonPhysicalPrimMax = 512");
                    tw.WriteLine("\t;PhysicalPrimMax = 128");
                    tw.WriteLine("\t;ClampPrimSize = false");
                    tw.WriteLine("\t;MaxPrimsPerUser = -1");
                    tw.WriteLine($"\t;ScopeID = {UUID.Random()}");
                    tw.WriteLine("\t;RegionType = Mainland");
                    tw.WriteLine("\t;RenderMinHeight = -1");
                    tw.WriteLine("\t;RenderMaxHeight = 100");
                    tw.WriteLine("\t;MapImageModule = Warp3DImageModule");
                    tw.WriteLine("\t;TextureOnMapTile = true");
                    tw.WriteLine("\t;DrawPrimOnMapTile = true");
                    tw.WriteLine("\t;GenerateMaptiles = true");
                    tw.WriteLine("\t;MaptileRefresh = 0");
                    tw.WriteLine("\t;MaptileStaticFile = path/to/SomeFile.png");
                    tw.WriteLine("\t;MasterAvatarFirstName = John");
                    tw.WriteLine("\t;MasterAvatarLastName = Doe");
                    tw.WriteLine("\t;MasterAvatarSandboxPassword = passwd");
                }
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✓ Your region has been configured for first run");
                Console.WriteLine($"  Region name: {_settings.WorldName}");
                Console.WriteLine($"  Location: {_settings.BaseLocationX},{_settings.BaseLocationY}");
                Console.WriteLine($"  Size: {_settings.RegionSizeX}x{_settings.RegionSizeY}x{_settings.RegionSizeZ}");
                Console.ResetColor();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Error configuring regions: {e.Message}");
                Console.ResetColor();
            }
        }

        private static void CheckMyWorldConfig()
        {
            if (File.Exists("config-include/MyWorld.ini"))
            {
                try
                {
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    File.Move("config-include/MyWorld.ini", $"config-include/MyWorld.ini.backup.{timestamp}");
                    myWorldReconfig = true;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"⚠ Existing MyWorld.ini backed up to MyWorld.ini.backup.{timestamp}");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"⚠ Could not backup existing MyWorld.ini: {ex.Message}");
                    Console.ResetColor();
                }
            }
        }

        private static void ConfigureMyWorld()
        {
            Console.WriteLine("\n╔════════════════════════════════════╗");
            Console.WriteLine("║   Configuring MyWorld...           ║");
            Console.WriteLine("╚════════════════════════════════════╝\n");
            
            CheckMyWorldConfig();

            if (!File.Exists("config-include/MyWorld.ini.example"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ Error: config-include/MyWorld.ini.example not found!");
                Console.ResetColor();
                return;
            }

            string connString = $"ConnectionString = \"{GetConnectionString()}\"";

            try
            {
                bool inWifiSection = false;
                bool wifiSectionWritten = false;
                
                using (TextReader tr = new StreamReader("config-include/MyWorld.ini.example"))
                {
                    using (TextWriter tw = new StreamWriter("config-include/MyWorld.ini"))
                    {
                        string line;
                        bool inUserAgentService = false;
                        bool storageProviderAdded = false;
                        
                        while ((line = tr.ReadLine()) != null)
                        {
                            // Track wenn wir in der [WifiService] Sektion sind
                            if (line.Trim().Equals("[WifiService]", StringComparison.OrdinalIgnoreCase))
                            {
                                inWifiSection = true;
                                wifiSectionWritten = true;
                                
                                // Write complete WifiService section with all required services
                                tw.WriteLine("[WifiService]");
                                tw.WriteLine("    Enabled = true");
                                tw.WriteLine("    ServerPort = ${Const|PublicPort}");
                                tw.WriteLine($"    GridName = \"{_settings.WorldName}\"");
                                tw.WriteLine($"    LoginURL = \"http://{_settings.IpAddress}:{_settings.HttpPort}\"");
                                tw.WriteLine($"    WebAddress = \"http://{_settings.IpAddress}:{_settings.HttpPort}\"");
                                tw.WriteLine();
                                tw.WriteLine("    ;; Service dependencies - REQUIRED for Wifi to work properly");
                                tw.WriteLine("    UserAccountService = \"OpenSim.Services.UserAccountService.dll:UserAccountService\"");
                                tw.WriteLine("    AuthenticationService = \"OpenSim.Services.AuthenticationService.dll:PasswordAuthenticationService\"");
                                tw.WriteLine("    GridService = \"OpenSim.Services.GridService.dll:GridService\"");
                                tw.WriteLine("    InventoryService = \"OpenSim.Services.InventoryService.dll:XInventoryService\"");
                                tw.WriteLine("    AvatarService = \"OpenSim.Services.AvatarService.dll:AvatarService\"");
                                tw.WriteLine("    GridUserService = \"OpenSim.Services.UserAccountService.dll:GridUserService\"");
                                tw.WriteLine();
                                tw.WriteLine("    ;; The Wifi Administrator account");
                                tw.WriteLine($"    AdminFirst = \"{_settings.AdminFirstName}\"");
                                tw.WriteLine($"    AdminLast = \"{_settings.AdminLastName}\"");
                                tw.WriteLine($"    AdminEmail = \"{_settings.AdminEmail}\"");
                                tw.WriteLine($"    AdminPassword = \"{_settings.AdminPassword}\"");
                                tw.WriteLine();
                                tw.WriteLine("    ;; Do you want to be able to control grid registrations?");
                                tw.WriteLine("    AccountConfirmationRequired = false");
                                tw.WriteLine();
                                tw.WriteLine("    ;; Variables for your mail server");
                                if (!string.IsNullOrEmpty(_settings.GmailAccount))
                                {
                                    tw.WriteLine("    SmtpHost = \"smtp.gmail.com\"");
                                    tw.WriteLine("    SmtpPort = \"587\"");
                                    tw.WriteLine($"    SmtpUsername = \"{_settings.GmailAccount}\"");
                                    tw.WriteLine($"    SmtpPassword = \"{_settings.GmailPassword}\"");
                                }
                                else
                                {
                                    tw.WriteLine("    ;SmtpHost = \"smtp.gmail.com\"");
                                    tw.WriteLine("    ;SmtpPort = \"587\"");
                                    tw.WriteLine("    ;SmtpUsername = \"your_email@gmail.com\"");
                                    tw.WriteLine("    ;SmtpPassword = \"secret\"");
                                }
                                tw.WriteLine();
                                tw.WriteLine($"    HomeLocation = \"{_settings.Region1Name}/128/128/30\"");
                                
                                // Skip all lines in original WifiService section until next section
                                continue;
                            }
                            
                            // Track when we leave WifiService section
                            if (inWifiSection && line.TrimStart().StartsWith("["))
                            {
                                inWifiSection = false;
                            }
                            
                            // Skip lines within WifiService section (we already wrote our complete version)
                            if (inWifiSection)
                            {
                                continue;
                            }
                            
                            // Track wenn wir in der [UserAgentService] Sektion sind
                            if (line.Trim().StartsWith("[UserAgentService]"))
                            {
                                inUserAgentService = true;
                                storageProviderAdded = false;
                                tw.WriteLine(line);
                                continue;
                            }
                            else if (line.Trim().StartsWith("[") && inUserAgentService)
                            {
                                // Neue Sektion beginnt - füge StorageProvider hinzu falls noch nicht geschehen
                                if (!storageProviderAdded)
                                {
                                    tw.WriteLine("    StorageProvider = \"OpenSim.Data.SQLite.dll\"");
                                    tw.WriteLine(connString);
                                }
                                inUserAgentService = false;
                            }
                            
                            if (line.Contains("ConnectionString"))
                                line = connString;
                            if (line.Contains("127.0.0.1"))
                                line = line.Replace("127.0.0.1", _settings.IpAddress);
                            if (line.Contains("welcome_message"))
                                line = line.Replace("Your World", _settings.WorldName);
                            if (line.Contains("DefaultRegion"))
                            {
                                string defRegionName = "Region_" + _settings.WorldName.Replace(' ', '_') + "_1";
                                line = line.Replace("Region_My_World_1", defRegionName);
                            }
                            if (line.Contains("SmtpUsername") && !string.IsNullOrEmpty(_settings.GmailAccount))
                                line = line.Replace("your_email", _settings.GmailAccount);
                            if (line.Contains("SmtpPassword") && !string.IsNullOrEmpty(_settings.GmailPassword))
                                line = line.Replace("secret", _settings.GmailPassword);
                            if (line.Contains("HomeLocation"))
                                line = line.Replace("My_World", $"{_settings.WorldName}/128/128/30");
                            if (line.Contains(":9000"))
                                line = line.Replace(":9000", $":{_settings.HttpPort}");
                            
                            // Check ob StorageProvider bereits in der Datei existiert
                            if (inUserAgentService && line.Contains("StorageProvider"))
                                storageProviderAdded = true;

                            tw.WriteLine(line);
                        }
                        
                        // Falls die Datei mit [UserAgentService] endet
                        if (inUserAgentService && !storageProviderAdded)
                        {
                            tw.WriteLine("    StorageProvider = \"OpenSim.Data.SQLite.dll\"");
                            tw.WriteLine(connString);
                        }
                        
                        // ========================================
                        // UserProfilesService Configuration
                        // Required for user profiles functionality
                        // ========================================
                        tw.WriteLine();
                        tw.WriteLine("; ========================================");
                        tw.WriteLine("; User Profiles Service");
                        tw.WriteLine("; ========================================");
                        tw.WriteLine();
                        tw.WriteLine("[UserProfilesService]");
                        tw.WriteLine("    Enabled = true");
                        tw.WriteLine("    LocalServiceModule = \"OpenSim.Services.UserProfilesService.dll:UserProfilesService\"");
                        tw.WriteLine(connString);
                        tw.WriteLine("    UserAccountService = OpenSim.Services.UserAccountService.dll:UserAccountService");
                        tw.WriteLine("    AuthenticationServiceModule = \"OpenSim.Services.AuthenticationService.dll:PasswordAuthenticationService\"");
                        
                        // ========================================
                        // Groups Configuration
                        // Using Groups Module V2 with Local Service Connector
                        // ========================================
                        tw.WriteLine();
                        tw.WriteLine("; ========================================");
                        tw.WriteLine("; Groups Module V2 Configuration");
                        tw.WriteLine("; ========================================");
                        tw.WriteLine();
                        tw.WriteLine("[Groups]");
                        tw.WriteLine("    Enabled = true");
                        tw.WriteLine("    Module = \"Groups Module V2\"");
                        tw.WriteLine("    StorageProvider = \"OpenSim.Data.SQLite.dll\"");
                        tw.WriteLine(connString);
                        tw.WriteLine("    ServicesConnectorModule = \"Groups Local Service Connector\"");
                        tw.WriteLine("    MessagingEnabled = true");
                        tw.WriteLine("    MessagingModule = \"Groups Messaging Module V2\"");
                        
                        // Apply MessageOnlineUsersOnly if configured
                        if (_settings.Groups != null && _settings.Groups.MessageOnlineUsersOnly)
                        {
                            tw.WriteLine("    MessageOnlineUsersOnly = true");
                        }
                    }
                }
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ Your World has been successfully configured for .NET 8");
                if (wifiSectionWritten)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("  ℹ [WifiService] section with AuthenticationService configured");
                    Console.WriteLine("  ℹ [UserProfilesService] section added");
                    Console.WriteLine("  ℹ [Groups] Module V2 with Local Service Connector configured");
                }
                Console.ResetColor();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Error configuring MyWorld: {e.Message}");
                Console.ResetColor();
            }
        }

        private static void ConfigureOpenSimIni()
        {
            Console.WriteLine("\n→ Configuring OpenSim.ini...");
            
            if (!File.Exists("OpenSim.ini.example"))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠ OpenSim.ini.example not found, skipping...");
                Console.ResetColor();
                return;
            }

            // Create backup before modifying
            BackupSingleFile("OpenSim.ini");

            // Bestimme die korrekte Include-Architecture-Datei basierend auf gewählter Architektur
            // HINWEIS: DivaPreferences.ini ist KEIN gültiger Include-Architecture-Wert
            // Diva-spezifische Einstellungen sind jetzt direkt in StandaloneHypergrid.ini integriert
            string architectureFile = _settings.Architecture switch
            {
                ArchitectureType.Standalone => "config-include/Standalone.ini",
                ArchitectureType.StandaloneHypergrid => "config-include/StandaloneHypergrid.ini",
                ArchitectureType.Grid => "config-include/Grid.ini",
                ArchitectureType.GridHypergrid => "config-include/GridHypergrid.ini",
                _ => "config-include/Standalone.ini" // Standard-Fallback: Standalone
            };

            try
            {
                bool architectureSet = false;
                using (TextReader tr = new StreamReader("OpenSim.ini.example"))
                {
                    using (TextWriter tw = new StreamWriter("OpenSim.ini"))
                    {
                        string line;
                        while ((line = tr.ReadLine()) != null)
                        {
                            if (line.Contains("BaseHostname =") && !line.TrimStart().StartsWith(";"))
                                line = $"    BaseHostname = \"{_settings.IpAddress}\"";
                            else if (line.Contains("BaseURL =") && !line.TrimStart().StartsWith(";"))
                                line = $"    BaseURL = http://${{Const|BaseHostname}}";
                            else if (line.Contains("PublicPort") && !line.TrimStart().StartsWith(";"))
                                line = $"    PublicPort = \"{_settings.HttpPort}\"";
                            else if (line.Contains("Include-Architecture"))
                            {
                                // Prüfe ob dies die gewählte Architecture ist
                                string trimmed = line.TrimStart().TrimStart(';').TrimStart();
                                
                                if (trimmed.Contains(architectureFile))
                                {
                                    // Dies ist die gewählte Architecture - aktivieren (Kommentar entfernen)
                                    line = $"    Include-Architecture = \"{architectureFile}\"";
                                    architectureSet = true;
                                }
                                else
                                {
                                    // Andere Architectures auskommentiert lassen oder auskommentieren
                                    if (!line.TrimStart().StartsWith(";"))
                                        line = "    ; " + trimmed;
                                    else
                                        line = "    ; " + trimmed; // Sicherstellen dass es auskommentiert bleibt
                                }
                            }
                            else if (line.Contains("127.0.0.1") && !line.TrimStart().StartsWith(";"))
                                line = line.Replace("127.0.0.1", _settings.IpAddress);
                            
                            tw.WriteLine(line);
                        }
                    }
                }
                
                if (architectureSet)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"✓ OpenSim.ini configured with Architecture: {_settings.Architecture}");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"  ℹ Include-Architecture = \"{architectureFile}\"");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"❌ Failed to set Include-Architecture in OpenSim.ini");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Error configuring OpenSim.ini: {ex.Message}");
                Console.ResetColor();
            }
        }

        private static void ConfigureRobustIni()
        {
            Console.WriteLine("\n→ Configuring Robust.ini...");
            
            if (!File.Exists("Robust.ini.example"))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠ Robust.ini.example not found, skipping...");
                Console.ResetColor();
                return;
            }

            // Create backup before modifying
            BackupSingleFile("Robust.ini");

            try
            {
                string connString = GetConnectionString();
                
                using (TextReader tr = new StreamReader("Robust.ini.example"))
                {
                    using (TextWriter tw = new StreamWriter("Robust.ini"))
                    {
                        string line;
                        while ((line = tr.ReadLine()) != null)
                        {
                            if (line.Contains("ConnectionString =") && !line.TrimStart().StartsWith(";"))
                                line = $"    ConnectionString = \"{connString}\"";
                            else if (line.Contains("BaseHostname =") && !line.TrimStart().StartsWith(";"))
                                line = $"    BaseHostname = \"{_settings.IpAddress}\"";
                            else if (line.Contains("BaseURL =") && !line.TrimStart().StartsWith(";"))
                                line = $"    BaseURL = \"http://${{Const|BaseHostname}}\"";
                            else if (line.Contains("PublicPort =") && !line.TrimStart().StartsWith(";"))
                                line = $"    PublicPort = \"{_settings.HttpPort}\"";
                            else if (line.Contains("127.0.0.1") && !line.TrimStart().StartsWith(";"))
                                line = line.Replace("127.0.0.1", _settings.IpAddress);
                            else if (line.Contains(":8002") && !line.TrimStart().StartsWith(";"))
                                line = line.Replace(":8002", $":{_settings.HttpPort}");
                            
                            tw.WriteLine(line);
                        }
                    }
                }
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ Robust.ini configured");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Error configuring Robust.ini: {ex.Message}");
                Console.ResetColor();
            }
        }

        private static void ConfigureRobustHGIni()
        {
            Console.WriteLine("\n→ Configuring Robust.HG.ini...");
            
            if (!File.Exists("Robust.HG.ini.example"))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠ Robust.HG.ini.example not found, skipping...");
                Console.ResetColor();
                return;
            }

            // Create backup before modifying
            BackupSingleFile("Robust.HG.ini");

            try
            {
                string connString = GetConnectionString();
                
                using (TextReader tr = new StreamReader("Robust.HG.ini.example"))
                {
                    using (TextWriter tw = new StreamWriter("Robust.HG.ini"))
                    {
                        string line;
                        while ((line = tr.ReadLine()) != null)
                        {
                            if (line.Contains("ConnectionString =") && !line.TrimStart().StartsWith(";"))
                                line = $"    ConnectionString = \"{connString}\"";
                            else if (line.Contains("BaseHostname =") && !line.TrimStart().StartsWith(";"))
                                line = $"    BaseHostname = \"{_settings.IpAddress}\"";
                            else if (line.Contains("BaseURL =") && !line.TrimStart().StartsWith(";"))
                                line = $"    BaseURL = \"http://${{Const|BaseHostname}}\"";
                            else if (line.Contains("PublicPort =") && !line.TrimStart().StartsWith(";"))
                                line = $"    PublicPort = \"{_settings.HttpPort}\"";
                            else if (line.Contains("GatekeeperURI =") && !line.TrimStart().StartsWith(";"))
                                line = $"    GatekeeperURI = \"http://{_settings.IpAddress}:{_settings.HttpPort}\"";
                            else if (line.Contains("127.0.0.1") && !line.TrimStart().StartsWith(";"))
                                line = line.Replace("127.0.0.1", _settings.IpAddress);
                            else if (line.Contains(":8002") && !line.TrimStart().StartsWith(";"))
                                line = line.Replace(":8002", $":{_settings.HttpPort}");
                            
                            tw.WriteLine(line);
                        }
                    }
                }
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ Robust.HG.ini configured");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Error configuring Robust.HG.ini: {ex.Message}");
                Console.ResetColor();
            }
        }

        private static void ConfigureWifiIni()
        {
            Console.WriteLine("\n→ Configuring Wifi.ini...");
            
            if (!File.Exists("Wifi.ini.example"))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠ Wifi.ini.example not found, skipping...");
                Console.ResetColor();
                return;
            }

            // Create backup before modifying
            BackupSingleFile("Wifi.ini");

            try
            {
                using (TextReader tr = new StreamReader("Wifi.ini.example"))
                {
                    using (TextWriter tw = new StreamWriter("Wifi.ini"))
                    {
                        string line;
                        while ((line = tr.ReadLine()) != null)
                        {
                            if (line.Contains("GridName") && !line.TrimStart().StartsWith(";"))
                                line = $"    GridName = \"{_settings.WorldName}\"";
                            if (line.Contains("LoginURL") && !line.TrimStart().StartsWith(";"))
                                line = $"    LoginURL = \"http://{_settings.IpAddress}:{_settings.HttpPort}\"";
                            if (line.Contains("WebAddress") && !line.TrimStart().StartsWith(";"))
                                line = $"    WebAddress = \"http://{_settings.IpAddress}:{_settings.HttpPort}\"";
                            if (line.Contains("AdminFirst") && !line.TrimStart().StartsWith(";"))
                                line = $"    AdminFirst = \"{_settings.AdminFirstName}\"";
                            if (line.Contains("AdminLast") && !line.TrimStart().StartsWith(";"))
                                line = $"    AdminLast = \"{_settings.AdminLastName}\"";
                            if (line.Contains("AdminEmail") && !line.TrimStart().StartsWith(";"))
                                line = $"    AdminEmail = \"{_settings.AdminEmail}\"";
                            if (line.Contains("SmtpUsername") && !string.IsNullOrEmpty(_settings.GmailAccount) && !line.TrimStart().StartsWith(";"))
                                line = $"    SmtpUsername = \"{_settings.GmailAccount}\"";
                            if (line.Contains("SmtpPassword") && !string.IsNullOrEmpty(_settings.GmailPassword) && !line.TrimStart().StartsWith(";"))
                                line = $"    SmtpPassword = \"{_settings.GmailPassword}\"";
                            
                            tw.WriteLine(line);
                        }
                    }
                }
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ Wifi.ini configured");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Error configuring Wifi.ini: {ex.Message}");
                Console.ResetColor();
            }
        }

        private static void ConfigureDivaPreferences()
        {
            Console.WriteLine("\n→ Configuring config-include/DivaPreferences.ini...");
            
            if (!File.Exists("config-include/DivaPreferences.ini.example"))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠ DivaPreferences.ini.example not found, skipping...");
                Console.ResetColor();
                return;
            }

            // Create backup before modifying
            BackupSingleFile("config-include/DivaPreferences.ini");

            try
            {
                using (TextReader tr = new StreamReader("config-include/DivaPreferences.ini.example"))
                {
                    using (TextWriter tw = new StreamWriter("config-include/DivaPreferences.ini"))
                    {
                        string line;
                        while ((line = tr.ReadLine()) != null)
                        {
                            if (line.Contains("HomeURI") && !line.TrimStart().StartsWith(";"))
                                line = $"    HomeURI = \"http://{_settings.IpAddress}:{_settings.HttpPort}\"";
                            if (line.Contains("GatekeeperURI") && !line.TrimStart().StartsWith(";"))
                                line = $"    GatekeeperURI = \"http://{_settings.IpAddress}:{_settings.HttpPort}\"";
                            
                            tw.WriteLine(line);
                        }
                    }
                }
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ DivaPreferences.ini configured");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Error configuring DivaPreferences.ini: {ex.Message}");
                Console.ResetColor();
            }
        }

        private static void ConfigureGridCommon()
        {
            Console.WriteLine("\n→ Configuring config-include/GridCommon.ini...");
            
            if (!File.Exists("config-include/GridCommon.ini.example"))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠ GridCommon.ini.example not found, skipping...");
                Console.ResetColor();
                return;
            }

            // Create backup before modifying
            BackupSingleFile("config-include/GridCommon.ini");

            try
            {
                using (TextReader tr = new StreamReader("config-include/GridCommon.ini.example"))
                {
                    using (TextWriter tw = new StreamWriter("config-include/GridCommon.ini"))
                    {
                        string line;
                        while ((line = tr.ReadLine()) != null)
                        {
                            if (line.Contains("127.0.0.1") && !line.TrimStart().StartsWith(";"))
                                line = line.Replace("127.0.0.1", _settings.IpAddress);
                            if (line.Contains(":8002") && !line.TrimStart().StartsWith(";"))
                                line = line.Replace(":8002", $":{_settings.HttpPort}");
                            
                            tw.WriteLine(line);
                        }
                    }
                }
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ GridCommon.ini configured");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Error configuring GridCommon.ini: {ex.Message}");
                Console.ResetColor();
            }
        }

        private static void ConfigureStandaloneCommon()
        {
            Console.WriteLine("\n→ Configuring config-include/StandaloneCommon.ini...");
            
            if (!File.Exists("config-include/StandaloneCommon.ini.example"))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠ StandaloneCommon.ini.example not found, skipping...");
                Console.ResetColor();
                return;
            }

            // Create backup before modifying
            BackupSingleFile("config-include/StandaloneCommon.ini");

            try
            {
                string connString = GetConnectionString();
                bool wifiSectionFound = false;
                bool inWifiSection = false;
                List<string> lines = new List<string>();
                
                // First pass: read all lines and check if WifiService exists
                using (TextReader tr = new StreamReader("config-include/StandaloneCommon.ini.example"))
                {
                    string line;
                    while ((line = tr.ReadLine()) != null)
                    {
                        if (line.Trim().Equals("[WifiService]", StringComparison.OrdinalIgnoreCase))
                        {
                            wifiSectionFound = true;
                            inWifiSection = true;
                        }
                        else if (inWifiSection && line.TrimStart().StartsWith("["))
                        {
                            inWifiSection = false;
                        }
                        
                        // Skip WifiService section if found (we'll add our own)
                        if (!inWifiSection || !wifiSectionFound)
                        {
                            // Apply normal configuration replacements
                            if (line.Contains("ConnectionString") && !line.TrimStart().StartsWith(";"))
                                line = $"    ConnectionString = \"{connString}\"";
                            if (line.Contains("127.0.0.1") && !line.TrimStart().StartsWith(";"))
                                line = line.Replace("127.0.0.1", _settings.IpAddress);
                            if (line.Contains("welcome_message") && !line.TrimStart().StartsWith(";"))
                                line = $"    welcome_message = \"Welcome to {_settings.WorldName}\"";
                            
                            lines.Add(line);
                        }
                    }
                }
                
                // Write all lines and append WifiService section at the end
                using (TextWriter tw = new StreamWriter("config-include/StandaloneCommon.ini"))
                {
                    foreach (string line in lines)
                    {
                        tw.WriteLine(line);
                    }
                    
                    // Add WifiService section (either replacing or adding new)
                    tw.WriteLine();
                    tw.WriteLine("; ===================================================================");
                    tw.WriteLine("; Diva Wifi Service Configuration");
                    tw.WriteLine("; Configured by Configure Tool");
                    tw.WriteLine("; ===================================================================");
                    tw.WriteLine();
                    tw.WriteLine("[WifiService]");
                    tw.WriteLine("    Enabled = true");
                    tw.WriteLine("    ServerPort = ${Const|PublicPort}");
                    tw.WriteLine($"    GridName = \"{_settings.WorldName}\"");
                    tw.WriteLine($"    LoginURL = \"http://{_settings.IpAddress}:{_settings.HttpPort}\"");
                    tw.WriteLine($"    WebAddress = \"http://{_settings.IpAddress}:{_settings.HttpPort}\"");
                    tw.WriteLine();
                    tw.WriteLine("    ;; Service dependencies - REQUIRED for Wifi to work properly");
                    tw.WriteLine("    UserAccountService = \"OpenSim.Services.UserAccountService.dll:UserAccountService\"");
                    tw.WriteLine("    AuthenticationService = \"OpenSim.Services.AuthenticationService.dll:PasswordAuthenticationService\"");
                    tw.WriteLine("    GridService = \"OpenSim.Services.GridService.dll:GridService\"");
                    tw.WriteLine("    InventoryService = \"OpenSim.Services.InventoryService.dll:XInventoryService\"");
                    tw.WriteLine("    AvatarService = \"OpenSim.Services.AvatarService.dll:AvatarService\"");
                    tw.WriteLine("    GridUserService = \"OpenSim.Services.UserAccountService.dll:GridUserService\"");
                    tw.WriteLine();
                    tw.WriteLine("    ;; The Wifi Administrator account");
                    tw.WriteLine($"    AdminFirst = \"{_settings.AdminFirstName}\"");
                    tw.WriteLine($"    AdminLast = \"{_settings.AdminLastName}\"");
                    tw.WriteLine($"    AdminEmail = \"{_settings.AdminEmail}\"");
                    tw.WriteLine($"    AdminPassword = \"{_settings.AdminPassword}\"");
                    tw.WriteLine();
                    tw.WriteLine("    ;; Do you want to be able to control grid registrations?");
                    tw.WriteLine("    AccountConfirmationRequired = false");
                    tw.WriteLine();
                    tw.WriteLine("    ;; Variables for your mail server");
                    tw.WriteLine("    ;; Users will get email notifications from this account.");
                    if (!string.IsNullOrEmpty(_settings.GmailAccount))
                    {
                        tw.WriteLine("    SmtpHost = \"smtp.gmail.com\"");
                        tw.WriteLine("    SmtpPort = \"587\"");
                        tw.WriteLine($"    SmtpUsername = \"{_settings.GmailAccount}\"");
                        tw.WriteLine($"    SmtpPassword = \"{_settings.GmailPassword}\"");
                    }
                    else
                    {
                        tw.WriteLine("    ;SmtpHost = \"smtp.gmail.com\"");
                        tw.WriteLine("    ;SmtpPort = \"587\"");
                        tw.WriteLine("    ;SmtpUsername = \"your_email@gmail.com\"");
                        tw.WriteLine("    ;SmtpPassword = \"secret\"");
                    }
                    tw.WriteLine();
                    tw.WriteLine($"    HomeLocation = \"{_settings.Region1Name}/128/128/30\"");
                    
                    // ========================================
                    // UserProfilesService Configuration
                    // Required for user profiles functionality
                    // ========================================
                    tw.WriteLine();
                    tw.WriteLine("; ========================================");
                    tw.WriteLine("; User Profiles Service");
                    tw.WriteLine("; ========================================");
                    tw.WriteLine();
                    tw.WriteLine("[UserProfilesService]");
                    tw.WriteLine("    Enabled = true");
                    tw.WriteLine("    LocalServiceModule = \"OpenSim.Services.UserProfilesService.dll:UserProfilesService\"");
                    tw.WriteLine($"    ConnectionString = \"{connString}\"");
                    tw.WriteLine("    UserAccountService = OpenSim.Services.UserAccountService.dll:UserAccountService");
                    tw.WriteLine("    AuthenticationServiceModule = \"OpenSim.Services.AuthenticationService.dll:PasswordAuthenticationService\"");
                    
                    // ========================================
                    // Groups Configuration
                    // Using Groups Module V2 with Local Service Connector
                    // ========================================
                    tw.WriteLine();
                    tw.WriteLine("; ========================================");
                    tw.WriteLine("; Groups Module V2 Configuration");
                    tw.WriteLine("; ========================================");
                    tw.WriteLine();
                    tw.WriteLine("[Groups]");
                    tw.WriteLine("    Enabled = true");
                    tw.WriteLine("    Module = \"Groups Module V2\"");
                    tw.WriteLine("    StorageProvider = \"OpenSim.Data.SQLite.dll\"");
                    tw.WriteLine($"    ConnectionString = \"{connString}\"");
                    tw.WriteLine("    ServicesConnectorModule = \"Groups Local Service Connector\"");
                    tw.WriteLine("    MessagingEnabled = true");
                    tw.WriteLine("    MessagingModule = \"Groups Messaging Module V2\"");
                    
                    // Apply MessageOnlineUsersOnly if configured
                    if (_settings.Groups != null && _settings.Groups.MessageOnlineUsersOnly)
                    {
                        tw.WriteLine("    MessageOnlineUsersOnly = true");
                    }
                }
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ StandaloneCommon.ini configured with Diva Wifi Service");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("  ℹ [WifiService] section with AuthenticationService added");
                Console.WriteLine("  ℹ [UserProfilesService] section added");
                Console.WriteLine("  ℹ [Groups] Module V2 with Local Service Connector configured");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Error configuring StandaloneCommon.ini: {ex.Message}");
                Console.ResetColor();
            }
        }

        private static void ConfigureStandaloneHypergrid()
        {
            Console.WriteLine("\n→ Configuring config-include/StandaloneHypergrid.ini...");
            
            if (!File.Exists("config-include/StandaloneHypergrid.ini.example"))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠ StandaloneHypergrid.ini.example not found, skipping...");
                Console.ResetColor();
                return;
            }

            // Create backup before modifying
            BackupSingleFile("config-include/StandaloneHypergrid.ini");

            try
            {
                string connString = GetConnectionString();
                
                using (TextReader tr = new StreamReader("config-include/StandaloneHypergrid.ini.example"))
                {
                    using (TextWriter tw = new StreamWriter("config-include/StandaloneHypergrid.ini"))
                    {
                        string line;
                        while ((line = tr.ReadLine()) != null)
                        {
                            if (line.Contains("ConnectionString") && !line.TrimStart().StartsWith(";"))
                                line = $"    ConnectionString = \"{connString}\"";
                            if (line.Contains("127.0.0.1") && !line.TrimStart().StartsWith(";"))
                                line = line.Replace("127.0.0.1", _settings.IpAddress);
                            if (line.Contains("HomeURI") && !line.TrimStart().StartsWith(";"))
                                line = $"    HomeURI = \"http://{_settings.IpAddress}:{_settings.HttpPort}\"";
                            if (line.Contains("GatekeeperURI") && !line.TrimStart().StartsWith(";"))
                                line = $"    GatekeeperURI = \"http://{_settings.IpAddress}:{_settings.HttpPort}\"";
                            if (line.Contains("welcome_message") && !line.TrimStart().StartsWith(";"))
                                line = $"    welcome_message = \"Welcome to {_settings.WorldName}\"";
                            
                            tw.WriteLine(line);
                        }
                        
                        // ========================================
                        // AgentPreferencesService Configuration
                        // Required for proper agent preferences storage
                        // ========================================
                        tw.WriteLine();
                        tw.WriteLine("; ========================================");
                        tw.WriteLine("; Agent Preferences Service");
                        tw.WriteLine("; ========================================");
                        tw.WriteLine();
                        tw.WriteLine("[AgentPreferencesService]");
                        tw.WriteLine("    LocalServiceModule = \"OpenSim.Services.UserAccountService.dll:AgentPreferencesService\"");
                        tw.WriteLine("    StorageProvider = \"OpenSim.Data.SQLite.dll\"");
                        tw.WriteLine($"    ConnectionString = \"{connString}\"");
                        
                        // ========================================
                        // Diva Distribution / MyWorld Settings
                        // Direkt integriert für Diva-Kompatibilität
                        // ========================================
                        tw.WriteLine();
                        tw.WriteLine("; ========================================");
                        tw.WriteLine("; Diva Distribution / MyWorld Settings");
                        tw.WriteLine("; Direkt integriert für Diva-Kompatibilität");
                        tw.WriteLine("; ========================================");
                        tw.WriteLine();
                        tw.WriteLine("[Startup]");
                        tw.WriteLine("    async_call_method = SmartThreadPool");
                        tw.WriteLine("    use_async_when_possible = false");
                        tw.WriteLine();
                        tw.WriteLine("[Network]");
                        tw.WriteLine($"    http_listener_port = {_settings.HttpPort}");
                        tw.WriteLine();
                        tw.WriteLine("[DataSnapshot]");
                        tw.WriteLine($"    gridname = \"{_settings.WorldName}\"");
                        tw.WriteLine();
                        tw.WriteLine("[UserProfiles]");
                        tw.WriteLine($"    ProfileServiceURL = \"http://{_settings.IpAddress}:{_settings.HttpPort}\"");
                        tw.WriteLine();
                        tw.WriteLine("[LoginService]");
                        tw.WriteLine($"    WelcomeMessage = \"Welcome to {_settings.WorldName}!\"");
                        tw.WriteLine($"    SRV_HomeURI = \"http://{_settings.IpAddress}:{_settings.HttpPort}\"");
                        tw.WriteLine($"    SRV_InventoryServerURI = \"http://{_settings.IpAddress}:{_settings.HttpPort}\"");
                        tw.WriteLine($"    SRV_AssetServerURI = \"http://{_settings.IpAddress}:{_settings.HttpPort}\"");
                        tw.WriteLine($"    SRV_FriendsServerURI = \"http://{_settings.IpAddress}:{_settings.HttpPort}\"");
                        tw.WriteLine($"    SRV_IMServerURI = \"http://{_settings.IpAddress}:{_settings.HttpPort}\"");
                        tw.WriteLine($"    SRV_GroupsServerURI = \"http://{_settings.IpAddress}:{_settings.HttpPort}\"");
                        tw.WriteLine($"    SRV_ProfileServerURI = \"http://{_settings.IpAddress}:{_settings.HttpPort}\"");
                        tw.WriteLine($"    MapTileURL = \"http://{_settings.IpAddress}:{_settings.HttpPort}/\"");
                        
                        // ========================================
                        // UserProfilesService Configuration
                        // Required for user profiles functionality with Hypergrid
                        // ========================================
                        tw.WriteLine();
                        tw.WriteLine("; ========================================");
                        tw.WriteLine("; User Profiles Service");
                        tw.WriteLine("; ========================================");
                        tw.WriteLine();
                        tw.WriteLine("[UserProfilesService]");
                        tw.WriteLine("    Enabled = true");
                        tw.WriteLine("    LocalServiceModule = \"OpenSim.Services.UserProfilesService.dll:UserProfilesService\"");
                        tw.WriteLine($"    ConnectionString = \"{connString}\"");
                        tw.WriteLine("    UserAccountService = OpenSim.Services.UserAccountService.dll:UserAccountService");
                        tw.WriteLine("    AuthenticationServiceModule = \"OpenSim.Services.AuthenticationService.dll:PasswordAuthenticationService\"");
                        
                        // ========================================
                        // Groups Configuration for Hypergrid
                        // Using Groups Module V2 with HG Service Connector
                        // ========================================
                        tw.WriteLine();
                        tw.WriteLine("; ========================================");
                        tw.WriteLine("; Groups Module V2 Configuration (Hypergrid)");
                        tw.WriteLine("; ========================================");
                        tw.WriteLine();
                        tw.WriteLine("[Groups]");
                        tw.WriteLine("    Enabled = true");
                        tw.WriteLine("    Module = \"Groups Module V2\"");
                        tw.WriteLine("    StorageProvider = \"OpenSim.Data.SQLite.dll\"");
                        tw.WriteLine($"    ConnectionString = \"{connString}\"");
                        tw.WriteLine("    ServicesConnectorModule = \"Groups HG Service Connector\"");
                        tw.WriteLine("    LocalService = local");
                        tw.WriteLine($"    HomeURI = \"http://{_settings.IpAddress}:{_settings.HttpPort}\"");
                        tw.WriteLine("    MessagingEnabled = true");
                        tw.WriteLine("    MessagingModule = \"Groups Messaging Module V2\"");
                        
                        // Apply MessageOnlineUsersOnly if configured
                        if (_settings.Groups != null && _settings.Groups.MessageOnlineUsersOnly)
                        {
                            tw.WriteLine("    MessageOnlineUsersOnly = true");
                        }
                    }
                }
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ StandaloneHypergrid.ini configured with Diva integration");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("  ℹ AgentPreferencesService added");
                Console.WriteLine("  ℹ Diva/MyWorld settings integrated");
                Console.WriteLine("  ℹ [UserProfilesService] section added");
                Console.WriteLine("  ℹ [Groups] Module V2 with HG Service Connector configured");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Error configuring StandaloneHypergrid.ini: {ex.Message}");
                Console.ResetColor();
            }
        }

        private static void ConfigureGridHypergrid()
        {
            Console.WriteLine("\n→ Configuring config-include/GridHypergrid.ini...");
            
            if (!File.Exists("config-include/GridHypergrid.ini.example"))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠ GridHypergrid.ini.example not found, skipping...");
                Console.ResetColor();
                return;
            }

            // Create backup before modifying
            BackupSingleFile("config-include/GridHypergrid.ini");

            try
            {
                string connString = GetConnectionString();
                
                using (TextReader tr = new StreamReader("config-include/GridHypergrid.ini.example"))
                {
                    using (TextWriter tw = new StreamWriter("config-include/GridHypergrid.ini"))
                    {
                        string line;
                        while ((line = tr.ReadLine()) != null)
                        {
                            if (line.Contains("ConnectionString") && !line.TrimStart().StartsWith(";"))
                                line = $"    ConnectionString = \"{connString}\"";
                            if (line.Contains("127.0.0.1") && !line.TrimStart().StartsWith(";"))
                                line = line.Replace("127.0.0.1", _settings.IpAddress);
                            if (line.Contains("HomeURI") && !line.TrimStart().StartsWith(";"))
                                line = $"    HomeURI = \"http://{_settings.IpAddress}:{_settings.HttpPort}\"";
                            if (line.Contains("GatekeeperURI") && !line.TrimStart().StartsWith(";"))
                                line = $"    GatekeeperURI = \"http://{_settings.IpAddress}:{_settings.HttpPort}\"";
                            if (line.Contains("welcome_message") && !line.TrimStart().StartsWith(";"))
                                line = $"    welcome_message = \"Welcome to {_settings.WorldName}\"";
                            if (line.Contains(":8002") && !line.TrimStart().StartsWith(";"))
                                line = line.Replace(":8002", $":{_settings.HttpPort}");
                            
                            tw.WriteLine(line);
                        }
                    }
                }
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ GridHypergrid.ini configured");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Error configuring GridHypergrid.ini: {ex.Message}");
                Console.ResetColor();
            }
        }

        private static void ConfigureOsslEnable()
        {
            Console.WriteLine("\n→ Configuring config-include/osslEnable.ini...");
            
            if (!File.Exists("config-include/osslEnable.ini.example"))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠ osslEnable.ini.example not found, skipping...");
                Console.ResetColor();
                return;
            }

            // Create backup before modifying
            BackupSingleFile("config-include/osslEnable.ini");

            try
            {
                // osslEnable.ini typically doesn't need IP/port replacements
                // but we copy it to ensure it's in place
                using (TextReader tr = new StreamReader("config-include/osslEnable.ini.example"))
                {
                    using (TextWriter tw = new StreamWriter("config-include/osslEnable.ini"))
                    {
                        string line;
                        while ((line = tr.ReadLine()) != null)
                        {
                            // Apply any necessary replacements here if needed
                            tw.WriteLine(line);
                        }
                    }
                }
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ osslEnable.ini configured");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Error configuring osslEnable.ini: {ex.Message}");
                Console.ResetColor();
            }
        }

        private static void ValidateConfiguration()
        {
            Console.WriteLine("\n╔════════════════════════════════════╗");
            Console.WriteLine("║   Validating Configuration         ║");
            Console.WriteLine("╚════════════════════════════════════╝\n");
            
            var issues = new List<string>();
            
            // Check required files
            if (!File.Exists("Regions/RegionConfig.ini"))
                issues.Add("Regions/RegionConfig.ini is missing");
            
            if (!File.Exists("config-include/MyWorld.ini"))
                issues.Add("config-include/MyWorld.ini is missing");
            
            // Validate settings
            if (string.IsNullOrWhiteSpace(_settings.WorldName))
                issues.Add("World name is not set");
            
            if (!ValidateIPOrDomain(_settings.IpAddress))
                issues.Add($"Invalid IP address or domain: {_settings.IpAddress}");
            
            if (!ValidateEmail(_settings.AdminEmail))
                issues.Add($"Invalid admin email: {_settings.AdminEmail}");
            
            if (_settings.HttpPort < 1 || _settings.HttpPort > 65535)
                issues.Add($"Invalid HTTP port: {_settings.HttpPort}");
            
            // Display results
            if (issues.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ Configuration validation passed!");
                Console.WriteLine($"  World Name: {_settings.WorldName}");
                Console.WriteLine($"  IP/Domain: {_settings.IpAddress}");
                Console.WriteLine($"  HTTP Port: {_settings.HttpPort}");
                Console.WriteLine($"  Database: {_settings.DbHost}/{_settings.DbSchema}");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Found {issues.Count} issue(s):");
                foreach (var issue in issues)
                {
                    Console.WriteLine($"   • {issue}");
                }
                Console.ResetColor();
            }
        }

        private static void CreateConfigBackup()
        {
            Console.WriteLine("\n→ Creating configuration backup...");
            
            try
            {
                if (!Directory.Exists(BackupDirectory))
                {
                    Directory.CreateDirectory(BackupDirectory);
                }
                
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                
                // Define all configuration files to backup with their descriptions
                var filesToBackup = new Dictionary<string, string>
                {
                    // Main directory files
                    { "OpenSim.ini", "OpenSim main configuration" },
                    { "Robust.ini", "Robust standalone configuration" },
                    { "Robust.HG.ini", "Robust Hypergrid configuration" },
                    { "Wifi.ini", "Wifi web interface configuration" },
                    
                    // config-include directory files
                    { "config-include/DivaPreferences.ini", "Diva preferences configuration" },
                    { "config-include/GridCommon.ini", "Grid common configuration" },
                    { "config-include/MyWorld.ini", "MyWorld custom configuration" },
                    { "config-include/StandaloneCommon.ini", "Standalone common configuration" },
                    { "config-include/StandaloneHypergrid.ini", "Standalone Hypergrid configuration" },
                    
                    // Regions directory files
                    { "Regions/Regions.ini", "Regions configuration" },
                    
                    // Additional files
                    { ConfigFile, "Configure tool settings" }
                };
                
                int backedUpCount = 0;
                int skippedCount = 0;
                var backedUpFiles = new List<string>();
                
                Console.WriteLine("\nBacking up configuration files:");
                Console.WriteLine("─────────────────────────────────────────────────────────");
                
                foreach (var item in filesToBackup)
                {
                    if (File.Exists(item.Key))
                    {
                        try
                        {
                            // Create backup with .bak extension and timestamp
                            string backupFileName = $"{item.Key}.bak_{timestamp}";
                            
                            // Ensure directory exists for the backup
                            string backupDir = Path.GetDirectoryName(backupFileName);
                            if (!string.IsNullOrEmpty(backupDir) && !Directory.Exists(backupDir))
                            {
                                Directory.CreateDirectory(backupDir);
                            }
                            
                            File.Copy(item.Key, backupFileName, true);
                            backedUpFiles.Add(backupFileName);
                            backedUpCount++;
                            
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"✓ {item.Key}");
                            Console.ResetColor();
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"✗ {item.Key} - Error: {ex.Message}");
                            Console.ResetColor();
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine($"○ {item.Key} (not found)");
                        Console.ResetColor();
                        skippedCount++;
                    }
                }
                
                Console.WriteLine("─────────────────────────────────────────────────────────");
                
                // Also create a compressed archive in the backup directory
                string backupName = $"config_backup_{timestamp}";
                string backupPath = Path.Combine(BackupDirectory, backupName);
                Directory.CreateDirectory(backupPath);
                
                // Copy files to archive directory
                foreach (var item in filesToBackup)
                {
                    if (File.Exists(item.Key))
                    {
                        string destPath = Path.Combine(backupPath, Path.GetFileName(item.Key));
                        File.Copy(item.Key, destPath, true);
                    }
                }
                
                // Create metadata
                var metadata = new BackupMetadata
                {
                    Timestamp = timestamp,
                    WorldName = _settings.WorldName,
                    ItemCount = backedUpCount
                };
                
                string metadataFile = Path.Combine(backupPath, "backup_info.json");
                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(metadataFile, JsonSerializer.Serialize(metadata, options));
                
                // Compress
                string zipFile = $"{backupPath}.zip";
                ZipFile.CreateFromDirectory(backupPath, zipFile);
                Directory.Delete(backupPath, true);
                
                var zipInfo = new FileInfo(zipFile);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n✓ Backup created successfully!");
                Console.WriteLine($"  Archive: {zipFile}");
                Console.WriteLine($"  Size: {FormatBytes(zipInfo.Length)}");
                Console.WriteLine($"  Files backed up: {backedUpCount}");
                Console.WriteLine($"  Files skipped: {skippedCount}");
                Console.WriteLine($"  Individual backups: {backedUpFiles.Count} files with .bak_{timestamp} extension");
                Console.ResetColor();
                
                // Clean up old backups (keep last 10 archives)
                CleanupOldBackups();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Backup failed: {ex.Message}");
                Console.ResetColor();
                if (_settings?.Debug == true)
                {
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }
        }
        
        private static void CleanupOldBackups()
        {
            try
            {
                if (!Directory.Exists(BackupDirectory))
                    return;
                
                var backupFiles = Directory.GetFiles(BackupDirectory, "config_backup_*.zip")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .ToList();
                
                if (backupFiles.Count > 10)
                {
                    Console.WriteLine("\nCleaning up old backup archives (keeping last 10)...");
                    var filesToDelete = backupFiles.Skip(10);
                    
                    foreach (var file in filesToDelete)
                    {
                        try
                        {
                            file.Delete();
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.WriteLine($"  Removed: {file.Name}");
                            Console.ResetColor();
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"  Could not remove {file.Name}: {ex.Message}");
                            Console.ResetColor();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"⚠ Could not clean up old backups: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        private static void BackupSingleFile(string filePath)
        {
            if (!File.Exists(filePath))
                return;
                
            try
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupFileName = $"{filePath}.bak_{timestamp}";
                
                // Ensure directory exists for the backup
                string backupDir = Path.GetDirectoryName(backupFileName);
                if (!string.IsNullOrEmpty(backupDir) && !Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                }
                
                File.Copy(filePath, backupFileName, true);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"  → Backup: {backupFileName}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"  ⚠ Could not backup {filePath}: {ex.Message}");
                Console.ResetColor();
            }
        }

        private static void RestoreFromBackup()
        {
            Console.WriteLine("\n╔════════════════════════════════════╗");
            Console.WriteLine("║   Restore from Backup              ║");
            Console.WriteLine("╚════════════════════════════════════╝\n");
            
            try
            {
                if (!Directory.Exists(BackupDirectory))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("⚠ No backups found.");
                    Console.ResetColor();
                    return;
                }
                
                var backupFiles = Directory.GetFiles(BackupDirectory, "config_backup_*.zip")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .ToList();
                
                if (!backupFiles.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("⚠ No backup files found.");
                    Console.ResetColor();
                    return;
                }
                
                Console.WriteLine("Available backups:\n");
                for (int i = 0; i < backupFiles.Count; i++)
                {
                    var file = backupFiles[i];
                    Console.WriteLine($"{i + 1}. {file.Name}");
                    Console.WriteLine($"   Created: {file.CreationTime:yyyy-MM-dd HH:mm:ss}");
                    Console.WriteLine($"   Size: {FormatBytes(file.Length)}");
                    Console.WriteLine();
                }
                
                Console.Write("Select backup to restore (number) or 0 to cancel: ");
                if (int.TryParse(Console.ReadLine(), out int selection) && selection > 0 && selection <= backupFiles.Count)
                {
                    var selectedBackup = backupFiles[selection - 1];
                    
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n⚠  WARNING: This will overwrite current configurations!");
                    Console.ResetColor();
                    Console.Write("Are you sure you want to continue? (yes/no): ");
                    
                    if (Console.ReadLine()?.ToLower() == "yes")
                    {
                        string tempDir = Path.Combine(BackupDirectory, "temp_restore");
                        if (Directory.Exists(tempDir))
                            Directory.Delete(tempDir, true);
                        
                        ZipFile.ExtractToDirectory(selectedBackup.FullName, tempDir);
                        
                        // Restore files
                        foreach (var file in Directory.GetFiles(tempDir))
                        {
                            string fileName = Path.GetFileName(file);
                            if (fileName == "backup_info.json")
                                continue;
                            
                            string destPath = fileName;
                            if (fileName == "MyWorld.ini")
                                destPath = "config-include/MyWorld.ini";
                            else if (fileName == "RegionConfig.ini")
                                destPath = "Regions/RegionConfig.ini";
                            else if (fileName.EndsWith(".ini"))
                                destPath = $"bin/{fileName}";
                            
                            File.Copy(file, destPath, true);
                            Console.WriteLine($"✓ Restored: {fileName}");
                        }
                        
                        Directory.Delete(tempDir, true);
                        
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\n✓ Restore completed successfully!");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine("Restore cancelled.");
                    }
                }
                else
                {
                    Console.WriteLine("Restore cancelled.");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ Restore failed: {ex.Message}");
                Console.ResetColor();
            }
        }

        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private static void DisplayInfo()
        {
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║              Configuration Complete!                         ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.WriteLine($"\n🌍 Your world is: {_settings.WorldName}");
            Console.WriteLine($"🔗 Your loginuri is: http://{_settings.IpAddress}:{_settings.HttpPort}");
            Console.WriteLine($"🌐 Your Wifi app is: http://{_settings.IpAddress}:{_settings.HttpPort}/wifi");
            Console.WriteLine($"\n👤 Your admin account for Wifi is:");
            Console.WriteLine($"   Username: {_settings.AdminFirstName} {_settings.AdminLastName}");
            Console.WriteLine($"   Password: {new string('*', _settings.AdminPassword.Length)}");
            Console.WriteLine($"   Email:    {_settings.AdminEmail}");
            
            if (string.IsNullOrEmpty(_settings.GmailAccount))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n⚠  Remember to set the Smtp Account for email notifications");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"\n📧 Your users get email notifications from {_settings.GmailAccount}@gmail.com");
            }
            
            if (myWorldReconfig)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\n→ NOTE: config-include/MyWorld.ini has been reconfigured.");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"\n📄 Your world's configuration is: config-include/MyWorld.ini");
            }
            
            Console.WriteLine($"\n💾 Database:");
            Console.WriteLine($"   Type:   {_settings.DbType}");
            if (_settings.DbType.Equals("MySQL", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"   Host:   {_settings.DbHost}");
                Console.WriteLine($"   Schema: {_settings.DbSchema}");
                Console.WriteLine($"   User:   {_settings.DbUser}");
            }
            else
            {
                Console.WriteLine($"   File:   OpenSim.db (SQLite)");
            }
            
            Console.WriteLine("\n" + new string('═', 64));
        }

        private static void DisplayHelp()
        {
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    Help & Usage                              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");
            Console.WriteLine("Usage: dotnet Configure.dll [OPTION]\n");
            Console.WriteLine("Options:");
            Console.WriteLine("  -a, --auto       Auto-configure using saved settings");
            Console.WriteLine("  -v, --validate   Validate current configuration");
            Console.WriteLine("  -b, --backup     Create backup of configuration files");
            Console.WriteLine("  -r, --restore    Restore from backup");
            Console.WriteLine("  -h, --help       Display this help message");
            Console.WriteLine("\nWithout options, the tool runs in interactive mode.");
        }

        private static void ApplyEngineSettings(ConfigurationSettings settings)
        {
            string openSimIni = Path.Combine("bin", "OpenSim.ini");
            if (!File.Exists(openSimIni))
            {
                Console.WriteLine("⚠️ OpenSim.ini not found. Skipping engine configuration.");
                return;
            }

            Console.WriteLine("\n🔧 Applying engine settings to OpenSim.ini...");

            // Create backup
            string backup = openSimIni + $".bak_{DateTime.Now:yyyyMMdd_HHmmss}";
            File.Copy(openSimIni, backup, true);
            Console.WriteLine($"   Backup created: {Path.GetFileName(backup)}");

            string content = File.ReadAllText(openSimIni);
            bool modified = false;

            // Apply Physics Engine settings
            if (settings.PhysicsEngine != null)
            {
                if (!string.IsNullOrEmpty(settings.PhysicsEngine.Meshing))
                {
                    content = ApplyIniSetting(content, "Startup", "meshing", settings.PhysicsEngine.Meshing, ref modified);
                    Console.WriteLine($"   ✓ Physics Meshing: {settings.PhysicsEngine.Meshing}");
                }
                if (!string.IsNullOrEmpty(settings.PhysicsEngine.Engine))
                {
                    content = ApplyIniSetting(content, "Startup", "physics", settings.PhysicsEngine.Engine, ref modified);
                    Console.WriteLine($"   ✓ Physics Engine: {settings.PhysicsEngine.Engine}");
                }
            }

            // Apply Script Engine settings
            if (settings.ScriptEngine != null)
            {
                if (!string.IsNullOrEmpty(settings.ScriptEngine.DefaultEngine))
                {
                    content = ApplyIniSetting(content, "Startup", "DefaultScriptEngine", $"\"{settings.ScriptEngine.DefaultEngine}\"", ref modified);
                    Console.WriteLine($"   ✓ Script Engine: {settings.ScriptEngine.DefaultEngine}");

                    // Enable YEngine section if YEngine is selected
                    if (settings.ScriptEngine.DefaultEngine.Equals("YEngine", StringComparison.OrdinalIgnoreCase))
                    {
                        content = ApplyIniSetting(content, "YEngine", "Enabled", "true", ref modified);
                        content = ApplyIniSetting(content, "YEngine", "MinTimerInterval", settings.ScriptEngine.MinTimerInterval.ToString(), ref modified);
                        content = ApplyIniSetting(content, "YEngine", "ScriptDistanceLimitFactor", settings.ScriptEngine.ScriptDistanceLimitFactor.ToString(), ref modified);
                        content = ApplyIniSetting(content, "YEngine", "Priority", settings.ScriptEngine.Priority ?? "BelowNormal", ref modified);
                        content = ApplyIniSetting(content, "YEngine", "MaxScriptEventQueue", settings.ScriptEngine.MaxScriptEventQueue.ToString(), ref modified);
                        Console.WriteLine("   ✓ YEngine enabled and configured");
                    }
                }
            }

            // Apply Network settings
            if (settings.Network != null)
            {
                if (!string.IsNullOrEmpty(settings.Network.OutboundDisallowForUserScriptsExcept))
                {
                    content = ApplyIniSetting(content, "Network", "OutboundDisallowForUserScriptsExcept", settings.Network.OutboundDisallowForUserScriptsExcept, ref modified);
                    Console.WriteLine($"   ✓ HTTP Filter: {settings.Network.OutboundDisallowForUserScriptsExcept}");
                }
                if (settings.Network.HttpBodyMaxLenMAX > 0)
                {
                    content = ApplyIniSetting(content, "Network", "HttpBodyMaxLenMAX", settings.Network.HttpBodyMaxLenMAX.ToString(), ref modified);
                }
            }

            // Apply OSSL settings
            if (settings.OSSL != null)
            {
                if (!string.IsNullOrEmpty(settings.OSSL.AllowOsslFunctions))
                {
                    content = ApplyIniSetting(content, "OSSL", "AllowOSFunctions", settings.OSSL.AllowOsslFunctions, ref modified);
                    Console.WriteLine($"   ✓ OSSL Functions: {settings.OSSL.AllowOsslFunctions}");
                }
                if (!string.IsNullOrEmpty(settings.OSSL.OsslThreatLevel))
                {
                    content = ApplyIniSetting(content, "OSSL", "OSFunctionThreatLevel", settings.OSSL.OsslThreatLevel, ref modified);
                    Console.WriteLine($"   ✓ OSSL Threat Level: {settings.OSSL.OsslThreatLevel}");
                }
            }
            
            // Apply Groups settings
            if (settings.Groups != null)
            {
                // MessageOnlineUsersOnly is required for "Groups Messaging Module V2"
                content = ApplyIniSetting(content, "Groups", "MessageOnlineUsersOnly", settings.Groups.MessageOnlineUsersOnly.ToString().ToLower(), ref modified);
                Console.WriteLine($"   ✓ Groups MessageOnlineUsersOnly: {settings.Groups.MessageOnlineUsersOnly}");
            }

            if (modified)
            {
                File.WriteAllText(openSimIni, content);
                Console.WriteLine("✅ Engine settings applied successfully!");
            }
            else
            {
                Console.WriteLine("ℹ️ No engine settings needed updating.");
            }
        }

        private static string ApplyIniSetting(string content, string section, string key, string value, ref bool modified)
        {
            // Pattern to find the section
            string sectionPattern = $@"^\s*\[{Regex.Escape(section)}\]";
            var sectionMatch = Regex.Match(content, sectionPattern, RegexOptions.Multiline);

            if (!sectionMatch.Success)
            {
                // Section doesn't exist, add it at the end
                content += $"\n[{section}]\n{key} = {value}\n";
                modified = true;
                return content;
            }

            // Find the next section or end of file
            int sectionStart = sectionMatch.Index;
            int sectionEnd = content.Length;
            var nextSectionMatch = Regex.Match(content.Substring(sectionStart + sectionMatch.Length), @"^\s*\[", RegexOptions.Multiline);
            if (nextSectionMatch.Success)
            {
                sectionEnd = sectionStart + sectionMatch.Length + nextSectionMatch.Index;
            }

            string sectionContent = content.Substring(sectionStart, sectionEnd - sectionStart);

            // Look for the key (commented or uncommented)
            string keyPattern = $@"^\s*;?\s*{Regex.Escape(key)}\s*=.*$";
            var keyMatch = Regex.Match(sectionContent, keyPattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);

            if (keyMatch.Success)
            {
                // Replace existing key
                string newLine = $"{key} = {value}";
                string newSectionContent = sectionContent.Remove(keyMatch.Index, keyMatch.Length).Insert(keyMatch.Index, newLine);
                content = content.Remove(sectionStart, sectionEnd - sectionStart).Insert(sectionStart, newSectionContent);
                modified = true;
            }
            else
            {
                // Key doesn't exist, add it after the section header
                int insertPos = sectionStart + sectionMatch.Length;
                content = content.Insert(insertPos, $"\n{key} = {value}");
                modified = true;
            }

            return content;
        }
    }
}