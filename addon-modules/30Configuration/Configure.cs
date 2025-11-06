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

namespace MetaverseInk.Configuration
{
    public class Configure
    {
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
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✓ Configuration loaded successfully");
                    Console.ResetColor();
                }
                else
                {
                    // Create default configuration
                    _settings = new ConfigurationSettings
                    {
                        WorldName = "My World",
                        DbHost = "localhost",
                        DbSchema = "opensim",
                        DbUser = "opensim",
                        DbPassword = "secret",
                        AdminFirstName = "Wifi",
                        AdminLastName = "Admin",
                        AdminPassword = "secret",
                        AdminEmail = "admin@localhost",
                        IpAddress = DetectExternalIP(),
                        HttpPort = 9000,
                        BaseLocationX = 1000,
                        BaseLocationY = 1000,
                        RegionSizeX = 256,
                        RegionSizeY = 256,
                        GmailAccount = string.Empty,
                        GmailPassword = string.Empty,
                        AutoBackup = true,
                        Debug = false
                    };
                    SaveConfiguration();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("⚠ No configuration found. Created default configuration.");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Error loading configuration: {ex.Message}");
                Console.ResetColor();
                _settings = new ConfigurationSettings(); // Use defaults
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
            
            ConfigureRegions();
            ConfigureMyWorld();
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
            
            ConfigureRegions();
            ConfigureMyWorld();
            DisplayInfo();
        }

        private static void GetUserInput()
        {
            Console.WriteLine("Please provide the following information (press Enter to use default):\n");
            
            // World Name
            Console.Write($"Name of your world [{_settings.WorldName}]: ");
            string input = Console.ReadLine();
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

            // Database Host
            Console.Write($"Database host [{_settings.DbHost}]: ");
            input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
                _settings.DbHost = input;

            // Database Schema
            Console.Write($"Database schema [{_settings.DbSchema}]: ");
            input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
                _settings.DbSchema = input;

            // Database User
            Console.Write($"Database user [{_settings.DbUser}]: ");
            input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
                _settings.DbUser = input;

            // Database Password
            Console.Write($"Database password [{new string('*', _settings.DbPassword.Length)}]: ");
            input = ReadPassword();
            if (!string.IsNullOrWhiteSpace(input))
                _settings.DbPassword = input;

            // Wifi Admin
            Console.Write($"\nWifi admin first name [{_settings.AdminFirstName}]: ");
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

            Console.Write($"\nWifi admin email [{_settings.AdminEmail}]: ");
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

            // Gmail (optional)
            Console.Write($"\nGmail account for notifications (optional) [{_settings.GmailAccount}]: ");
            input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
            {
                _settings.GmailAccount = input;
                
                Console.Write("Gmail password: ");
                string gmailPwd = ReadPassword();
                if (!string.IsNullOrWhiteSpace(gmailPwd))
                    _settings.GmailPassword = gmailPwd;
            }

            // Region Settings
            Console.WriteLine($"\n--- Region Settings ---");
            Console.Write($"Base location X coordinate [{_settings.BaseLocationX}]: ");
            input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input) && int.TryParse(input, out int locX))
                _settings.BaseLocationX = locX;

            Console.Write($"Base location Y coordinate [{_settings.BaseLocationY}]: ");
            input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input) && int.TryParse(input, out int locY))
                _settings.BaseLocationY = locY;

            Console.Write($"Region size X [{_settings.RegionSizeX}]: ");
            input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input) && int.TryParse(input, out int sizeX))
                _settings.RegionSizeX = sizeX;

            Console.Write($"Region size Y [{_settings.RegionSizeY}]: ");
            input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input) && int.TryParse(input, out int sizeY))
                _settings.RegionSizeY = sizeY;

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

        private static RegionConfigStatus CheckRegionConfig()
        {
            if (File.Exists("Regions/RegionConfig.ini"))
            {
                using (TextReader tr = new StreamReader("Regions/RegionConfig.ini"))
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
                Console.WriteLine("   Please edit file Regions/RegionConfig.ini and delete all references to MasterAvatar.");
                Console.ResetColor();
                return;
            }

            // else RegionConfigStatus.NeedsCreation
            if (!File.Exists("Regions/RegionConfig.ini.example"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ Error: Regions/RegionConfig.ini.example not found!");
                Console.ResetColor();
                return;
            }

            int count = 0;
            try
            {
                using (TextReader tr = new StreamReader("Regions/RegionConfig.ini.example"))
                {
                    using (TextWriter tw = new StreamWriter("Regions/RegionConfig.ini"))
                    {
                        string line;
                        while ((line = tr.ReadLine()) != null)
                        {
                            if (line.Contains("My World"))
                                line = line.Replace("My World", _settings.WorldName);
                            if (line.Contains("RegionUUID"))
                                line = line.Replace("RegionUUID", "RegionUUID = " + UUID.Random().ToString());
                            if (line.Contains("Location"))
                            {
                                count++;
                                int locX = _settings.BaseLocationX + count * 1000;
                                int locY = _settings.BaseLocationY;
                                line = line.Replace("1000,1000", $"{locX},{locY}");
                            }
                            if (line.Contains("SizeX"))
                                line = $"SizeX = {_settings.RegionSizeX}";
                            if (line.Contains("SizeY"))
                                line = $"SizeY = {_settings.RegionSizeY}";
                            if (line.Contains("ExternalHostName"))
                                line = $"ExternalHostName = {_settings.IpAddress}";
                            if (line.Contains("InternalPort"))
                                line = $"InternalPort = {9000 + count}";
                            
                            tw.WriteLine(line);
                        }
                    }
                }
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✓ Your regions have been configured for first run");
                Console.WriteLine($"  Regions created: {count}");
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

            string connString = $"ConnectionString = \"Data Source={_settings.DbHost};Database={_settings.DbSchema};User ID={_settings.DbUser};Password={_settings.DbPassword};Old Guids=true;Allow Zero Datetime=true;\"";

            try
            {
                using (TextReader tr = new StreamReader("config-include/MyWorld.ini.example"))
                {
                    using (TextWriter tw = new StreamWriter("config-include/MyWorld.ini"))
                    {
                        string line;
                        while ((line = tr.ReadLine()) != null)
                        {
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

                            tw.WriteLine(line);
                        }
                    }
                }
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ Your World has been successfully configured for .NET 8");
                Console.ResetColor();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Error configuring MyWorld: {e.Message}");
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
                string backupName = $"config_backup_{timestamp}";
                string backupPath = Path.Combine(BackupDirectory, backupName);
                Directory.CreateDirectory(backupPath);
                
                var filesToBackup = new Dictionary<string, string>
                {
                    { "config-include/MyWorld.ini", "MyWorld configuration" },
                    { "Regions/RegionConfig.ini", "Region configuration" },
                    { "bin/OpenSim.ini", "OpenSim configuration" },
                    { "bin/Robust.ini", "Robust configuration" },
                    { "bin/Wifi.ini", "Wifi configuration" },
                    { ConfigFile, "Configure tool settings" }
                };
                
                int backedUpCount = 0;
                
                foreach (var item in filesToBackup)
                {
                    if (File.Exists(item.Key))
                    {
                        string destPath = Path.Combine(backupPath, Path.GetFileName(item.Key));
                        File.Copy(item.Key, destPath, true);
                        backedUpCount++;
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
                Console.WriteLine($"✓ Backup created successfully!");
                Console.WriteLine($"  Location: {zipFile}");
                Console.WriteLine($"  Size: {FormatBytes(zipInfo.Length)}");
                Console.WriteLine($"  Files backed up: {backedUpCount}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Backup failed: {ex.Message}");
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
            Console.WriteLine($"   Host:   {_settings.DbHost}");
            Console.WriteLine($"   Schema: {_settings.DbSchema}");
            Console.WriteLine($"   User:   {_settings.DbUser}");
            
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
    }

    public class ConfigurationSettings
    {
        public string WorldName { get; set; }
        public string DbHost { get; set; }
        public string DbSchema { get; set; }
        public string DbUser { get; set; }
        public string DbPassword { get; set; }
        public string AdminFirstName { get; set; }
        public string AdminLastName { get; set; }
        public string AdminPassword { get; set; }
        public string AdminEmail { get; set; }
        public string IpAddress { get; set; }
        public int HttpPort { get; set; }
        public int BaseLocationX { get; set; }
        public int BaseLocationY { get; set; }
        public int RegionSizeX { get; set; }
        public int RegionSizeY { get; set; }
        public string GmailAccount { get; set; }
        public string GmailPassword { get; set; }
        public bool AutoBackup { get; set; }
        public bool Debug { get; set; }
    }

    public class BackupMetadata
    {
        public string Timestamp { get; set; }
        public string WorldName { get; set; }
        public int ItemCount { get; set; }
    }
}