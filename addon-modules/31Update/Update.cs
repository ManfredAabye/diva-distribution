using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace Update
{
    public class Update
    {
        private static readonly string BackupDirectory = "backups";
        private static readonly string ConfigFile = "UpdateConfig.json";
        private static UpdateConfiguration _config;

        public static async Task Main(string[] args)
        {
            DisplayBanner();
            
            try
            {
                LoadConfiguration();
                
                if (args.Length > 0)
                {
                    await HandleCommandLineArgs(args);
                }
                else
                {
                    await InteractiveMode();
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ Critical Error: {e.Message}");
                Console.ResetColor();
                if (_config?.Debug == true)
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
            Console.WriteLine("║       Diva Distribution Update Tool for .NET 8              ║");
            Console.WriteLine("║       Version 1.0.0                                          ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine($"Runtime: .NET {Environment.Version}");
            Console.WriteLine($"Current Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");
        }

        private static async Task InteractiveMode()
        {
            while (true)
            {
                Console.WriteLine("\n╔════════════════════════════════════╗");
                Console.WriteLine("║         Main Menu                  ║");
                Console.WriteLine("╚════════════════════════════════════╝");
                Console.WriteLine("1. Check for Updates");
                Console.WriteLine("2. Backup Configuration");
                Console.WriteLine("3. Migrate Configurations");
                Console.WriteLine("4. Restore from Backup");
                Console.WriteLine("5. View Current Version");
                Console.WriteLine("6. Settings");
                Console.WriteLine("0. Exit");
                Console.Write("\nSelect an option: ");
                
                string choice = Console.ReadLine();
                
                switch (choice)
                {
                    case "1":
                        await CheckForUpdates();
                        break;
                    case "2":
                        CreateFullBackup();
                        break;
                    case "3":
                        MigrateConfigs();
                        break;
                    case "4":
                        RestoreFromBackup();
                        break;
                    case "5":
                        DisplayCurrentVersion();
                        break;
                    case "6":
                        ConfigureSettings();
                        break;
                    case "0":
                        return;
                    default:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Invalid option. Please try again.");
                        Console.ResetColor();
                        break;
                }
            }
        }

        private static async Task HandleCommandLineArgs(string[] args)
        {
            string command = args[0].ToLower();
            
            switch (command)
            {
                case "--check":
                case "-c":
                    await CheckForUpdates();
                    break;
                case "--backup":
                case "-b":
                    CreateFullBackup();
                    break;
                case "--migrate":
                case "-m":
                    MigrateConfigs();
                    break;
                case "--restore":
                case "-r":
                    RestoreFromBackup();
                    break;
                case "--version":
                case "-v":
                    DisplayCurrentVersion();
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

        private static void LoadConfiguration()
        {
            try
            {
                if (File.Exists(ConfigFile))
                {
                    string json = File.ReadAllText(ConfigFile);
                    _config = JsonSerializer.Deserialize<UpdateConfiguration>(json);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✓ Configuration loaded successfully");
                    Console.ResetColor();
                }
                else
                {
                    // Create default configuration
                    _config = new UpdateConfiguration
                    {
                        UpdateServerUrl = "https://api.github.com/repos/diva/diva-distribution/releases",
                        CheckForPreReleases = false,
                        AutoBackup = true,
                        BackupRetentionDays = 30,
                        Debug = false,
                        CurrentVersion = "1.0.0"
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
                _config = new UpdateConfiguration(); // Use defaults
            }
        }

        private static void SaveConfiguration()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_config, options);
                File.WriteAllText(ConfigFile, json);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Error saving configuration: {ex.Message}");
                Console.ResetColor();
            }
        }

        private static async Task CheckForUpdates()
        {
            Console.WriteLine("\n╔════════════════════════════════════╗");
            Console.WriteLine("║     Checking for Updates...        ║");
            Console.WriteLine("╚════════════════════════════════════╝\n");
            
            using (var client = new HttpClient())
            {
                try
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.DefaultRequestHeaders.UserAgent.Add(
                        new ProductInfoHeaderValue("DivaDistribution-UpdateTool", "1.0"));
                    
                    Console.WriteLine($"→ Connecting to update server: {_config.UpdateServerUrl}");
                    
                    string url = _config.CheckForPreReleases 
                        ? _config.UpdateServerUrl 
                        : $"{_config.UpdateServerUrl}/latest";
                    
                    var response = await client.GetStringAsync(url);
                    
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✓ Successfully connected to update server");
                    Console.ResetColor();
                    
                    // Parse JSON response
                    using (JsonDocument doc = JsonDocument.Parse(response))
                    {
                        JsonElement root = doc.RootElement;
                        
                        if (_config.CheckForPreReleases && root.ValueKind == JsonValueKind.Array)
                        {
                            // Get first release from array
                            if (root.GetArrayLength() > 0)
                            {
                                root = root[0];
                            }
                        }
                        
                        string latestVersion = root.GetProperty("tag_name").GetString();
                        string releaseName = root.GetProperty("name").GetString();
                        string publishedAt = root.GetProperty("published_at").GetString();
                        string releaseUrl = root.GetProperty("html_url").GetString();
                        bool isPrerelease = root.GetProperty("prerelease").GetBoolean();
                        
                        Console.WriteLine($"\n📦 Latest Available Version: {latestVersion}");
                        Console.WriteLine($"📝 Release Name: {releaseName}");
                        Console.WriteLine($"📅 Published: {publishedAt}");
                        Console.WriteLine($"🔗 URL: {releaseUrl}");
                        
                        if (isPrerelease)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("⚠  This is a pre-release version");
                            Console.ResetColor();
                        }
                        
                        // Compare versions
                        if (CompareVersions(latestVersion, _config.CurrentVersion) > 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"\n✓ New version available! ({_config.CurrentVersion} → {latestVersion})");
                            Console.ResetColor();
                            
                            // Get release notes
                            if (root.TryGetProperty("body", out JsonElement bodyElement))
                            {
                                string releaseNotes = bodyElement.GetString();
                                Console.WriteLine("\n📋 Release Notes:");
                                Console.WriteLine(new string('─', 60));
                                if (!string.IsNullOrEmpty(releaseNotes))
                                {
                                    Console.WriteLine(releaseNotes.Substring(0, Math.Min(500, releaseNotes.Length)));
                                    if (releaseNotes.Length > 500)
                                    {
                                        Console.WriteLine("\n[...truncated. See full release notes at the URL above]");
                                    }
                                }
                                Console.WriteLine(new string('─', 60));
                            }
                            
                            // Offer to download
                            Console.Write("\nWould you like to see download options? (y/n): ");
                            if (Console.ReadLine()?.ToLower() == "y")
                            {
                                DisplayDownloadOptions(root);
                            }
                        }
                        else if (CompareVersions(latestVersion, _config.CurrentVersion) == 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"\n✓ You are running the latest version ({_config.CurrentVersion})");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"\n→ You are running a newer version ({_config.CurrentVersion}) than the latest release ({latestVersion})");
                            Console.ResetColor();
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"❌ Network Error: {ex.Message}");
                    Console.WriteLine("   Please check your internet connection and try again.");
                    Console.ResetColor();
                }
                catch (TaskCanceledException)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ Update check timed out. The server may be unavailable.");
                    Console.ResetColor();
                }
                catch (JsonException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"❌ Error parsing update information: {ex.Message}");
                    Console.ResetColor();
                }
            }
        }

        private static void DisplayDownloadOptions(JsonElement release)
        {
            if (release.TryGetProperty("assets", out JsonElement assets) && assets.GetArrayLength() > 0)
            {
                Console.WriteLine("\n📦 Available Downloads:");
                int index = 1;
                foreach (var asset in assets.EnumerateArray())
                {
                    string name = asset.GetProperty("name").GetString();
                    long size = asset.GetProperty("size").GetInt64();
                    string downloadUrl = asset.GetProperty("browser_download_url").GetString();
                    
                    Console.WriteLine($"{index}. {name} ({FormatBytes(size)})");
                    Console.WriteLine($"   URL: {downloadUrl}");
                    index++;
                }
            }
            else
            {
                Console.WriteLine("No downloadable assets found for this release.");
            }
        }

        private static int CompareVersions(string version1, string version2)
        {
            // Remove 'v' prefix if present
            version1 = version1?.TrimStart('v') ?? "0.0.0";
            version2 = version2?.TrimStart('v') ?? "0.0.0";
            
            try
            {
                Version v1 = new Version(version1);
                Version v2 = new Version(version2);
                return v1.CompareTo(v2);
            }
            catch
            {
                return string.Compare(version1, version2, StringComparison.OrdinalIgnoreCase);
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

        private static void CreateFullBackup()
        {
            Console.WriteLine("\n╔════════════════════════════════════╗");
            Console.WriteLine("║      Creating Backup...            ║");
            Console.WriteLine("╚════════════════════════════════════╝\n");
            
            try
            {
                // Create backup directory if it doesn't exist
                if (!Directory.Exists(BackupDirectory))
                {
                    Directory.CreateDirectory(BackupDirectory);
                    Console.WriteLine($"✓ Created backup directory: {BackupDirectory}");
                }
                
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupName = $"backup_{timestamp}";
                string backupPath = Path.Combine(BackupDirectory, backupName);
                Directory.CreateDirectory(backupPath);
                
                // List of important directories and files to backup
                var itemsToBackup = new Dictionary<string, string>
                {
                    { "config-include", "Configuration files" },
                    { "Regions", "Region configurations" },
                    { "bin/OpenSim.ini", "OpenSim configuration" },
                    { "bin/Robust.ini", "Robust configuration" },
                    { "bin/Wifi.ini", "Wifi configuration" }
                };
                
                int backedUpItems = 0;
                
                foreach (var item in itemsToBackup)
                {
                    string sourcePath = item.Key;
                    string description = item.Value;
                    
                    if (Directory.Exists(sourcePath))
                    {
                        string destPath = Path.Combine(backupPath, Path.GetFileName(sourcePath));
                        CopyDirectory(sourcePath, destPath);
                        Console.WriteLine($"✓ Backed up: {description} ({sourcePath})");
                        backedUpItems++;
                    }
                    else if (File.Exists(sourcePath))
                    {
                        string destPath = Path.Combine(backupPath, Path.GetFileName(sourcePath));
                        Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                        File.Copy(sourcePath, destPath, true);
                        Console.WriteLine($"✓ Backed up: {description} ({sourcePath})");
                        backedUpItems++;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"⚠ Skipped: {description} ({sourcePath}) - not found");
                        Console.ResetColor();
                    }
                }
                
                // Create backup metadata
                var metadata = new BackupMetadata
                {
                    Timestamp = timestamp,
                    Version = _config.CurrentVersion,
                    ItemCount = backedUpItems,
                    BackupPath = backupPath
                };
                
                string metadataFile = Path.Combine(backupPath, "backup_info.json");
                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(metadataFile, JsonSerializer.Serialize(metadata, options));
                
                // Compress backup
                string zipFile = $"{backupPath}.zip";
                Console.WriteLine($"\n→ Compressing backup...");
                ZipFile.CreateFromDirectory(backupPath, zipFile);
                
                // Remove uncompressed backup folder
                Directory.Delete(backupPath, true);
                
                var zipInfo = new FileInfo(zipFile);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n✓ Backup completed successfully!");
                Console.WriteLine($"  Location: {zipFile}");
                Console.WriteLine($"  Size: {FormatBytes(zipInfo.Length)}");
                Console.WriteLine($"  Items backed up: {backedUpItems}");
                Console.ResetColor();
                
                // Cleanup old backups
                CleanupOldBackups();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ Backup failed: {ex.Message}");
                Console.ResetColor();
            }
        }

        private static void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);
            
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }
            
            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
                CopyDirectory(subDir, destSubDir);
            }
        }

        private static void CleanupOldBackups()
        {
            try
            {
                if (!Directory.Exists(BackupDirectory))
                    return;
                
                var backupFiles = Directory.GetFiles(BackupDirectory, "backup_*.zip")
                    .Select(f => new FileInfo(f))
                    .Where(f => f.CreationTime < DateTime.Now.AddDays(-_config.BackupRetentionDays))
                    .ToList();
                
                if (backupFiles.Any())
                {
                    Console.WriteLine($"\n→ Cleaning up old backups (older than {_config.BackupRetentionDays} days)...");
                    foreach (var file in backupFiles)
                    {
                        file.Delete();
                        Console.WriteLine($"  Deleted: {file.Name}");
                    }
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"✓ Removed {backupFiles.Count} old backup(s)");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"⚠ Warning: Could not cleanup old backups: {ex.Message}");
                Console.ResetColor();
            }
        }

        private static void RestoreFromBackup()
        {
            Console.WriteLine("\n╔════════════════════════════════════╗");
            Console.WriteLine("║     Restore from Backup            ║");
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
                
                var backupFiles = Directory.GetFiles(BackupDirectory, "backup_*.zip")
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
                        Console.WriteLine("\n→ Restoring backup...");
                        
                        string tempDir = Path.Combine(BackupDirectory, "temp_restore");
                        if (Directory.Exists(tempDir))
                            Directory.Delete(tempDir, true);
                        
                        ZipFile.ExtractToDirectory(selectedBackup.FullName, tempDir);
                        
                        // Restore directories
                        foreach (var dir in Directory.GetDirectories(tempDir))
                        {
                            string dirName = Path.GetFileName(dir);
                            if (dirName != "bin") // Don't restore entire bin directory
                            {
                                string destDir = Path.Combine(Directory.GetCurrentDirectory(), dirName);
                                if (Directory.Exists(destDir))
                                {
                                    Directory.Delete(destDir, true);
                                }
                                CopyDirectory(dir, destDir);
                                Console.WriteLine($"✓ Restored: {dirName}");
                            }
                        }
                        
                        // Clean up temp directory
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

        public static void MigrateConfigs()
        {
            Console.WriteLine("\n╔════════════════════════════════════╗");
            Console.WriteLine("║   Migrating Configurations...      ║");
            Console.WriteLine("╚════════════════════════════════════╝\n");
            
            try
            {
                if (_config.AutoBackup)
                {
                    Console.WriteLine("→ Creating automatic backup before migration...");
                    CreateFullBackup();
                    Console.WriteLine();
                }
                
                var configFiles = new[]
                {
                    "config-include/MyWorld.ini",
                    "Regions/RegionConfig.ini",
                    "bin/OpenSim.ini",
                    "bin/Robust.ini",
                    "bin/Wifi.ini"
                };
                
                int migratedCount = 0;
                
                foreach (var configFile in configFiles)
                {
                    if (File.Exists(configFile))
                    {
                        // Read and check for .NET 8 compatibility
                        string content = File.ReadAllText(configFile);
                        
                        // Example migration: Update old .NET Framework references
                        bool modified = false;
                        
                        if (content.Contains(".NET Framework"))
                        {
                            content = content.Replace(".NET Framework", ".NET 8");
                            modified = true;
                        }
                        
                        if (modified)
                        {
                            File.WriteAllText(configFile, content);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"✓ Migrated: {configFile}");
                            Console.ResetColor();
                            migratedCount++;
                        }
                        else
                        {
                            Console.WriteLine($"→ Already up-to-date: {configFile}");
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"⚠ Not found: {configFile}");
                        Console.ResetColor();
                    }
                }
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n✓ Configuration migration completed!");
                Console.WriteLine($"  Files migrated: {migratedCount}");
                Console.ResetColor();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ Error during configuration migration: {e.Message}");
                Console.ResetColor();
            }
        }

        private static void DisplayCurrentVersion()
        {
            Console.WriteLine("\n╔════════════════════════════════════╗");
            Console.WriteLine("║       Version Information          ║");
            Console.WriteLine("╚════════════════════════════════════╝\n");
            Console.WriteLine($"Current Version: {_config.CurrentVersion}");
            Console.WriteLine($"Update Tool Version: 1.0.0");
            Console.WriteLine($".NET Runtime: {Environment.Version}");
            Console.WriteLine($"OS: {Environment.OSVersion}");
            Console.WriteLine($"64-bit Process: {Environment.Is64BitProcess}");
        }

        private static void ConfigureSettings()
        {
            Console.WriteLine("\n╔════════════════════════════════════╗");
            Console.WriteLine("║          Settings                  ║");
            Console.WriteLine("╚════════════════════════════════════╝\n");
            
            Console.WriteLine("1. Set Current Version");
            Console.WriteLine("2. Toggle Pre-release Updates");
            Console.WriteLine("3. Toggle Auto Backup");
            Console.WriteLine("4. Set Backup Retention Days");
            Console.WriteLine("5. Toggle Debug Mode");
            Console.WriteLine("0. Back to Main Menu");
            Console.Write("\nSelect option: ");
            
            string choice = Console.ReadLine();
            
            switch (choice)
            {
                case "1":
                    Console.Write("Enter current version: ");
                    _config.CurrentVersion = Console.ReadLine();
                    SaveConfiguration();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✓ Version updated");
                    Console.ResetColor();
                    break;
                case "2":
                    _config.CheckForPreReleases = !_config.CheckForPreReleases;
                    SaveConfiguration();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"✓ Pre-release updates: {(_config.CheckForPreReleases ? "Enabled" : "Disabled")}");
                    Console.ResetColor();
                    break;
                case "3":
                    _config.AutoBackup = !_config.AutoBackup;
                    SaveConfiguration();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"✓ Auto backup: {(_config.AutoBackup ? "Enabled" : "Disabled")}");
                    Console.ResetColor();
                    break;
                case "4":
                    Console.Write("Enter backup retention days: ");
                    if (int.TryParse(Console.ReadLine(), out int days) && days > 0)
                    {
                        _config.BackupRetentionDays = days;
                        SaveConfiguration();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"✓ Backup retention set to {days} days");
                        Console.ResetColor();
                    }
                    break;
                case "5":
                    _config.Debug = !_config.Debug;
                    SaveConfiguration();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"✓ Debug mode: {(_config.Debug ? "Enabled" : "Disabled")}");
                    Console.ResetColor();
                    break;
            }
        }

        private static void DisplayHelp()
        {
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    Help & Usage                            ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");
            Console.WriteLine("Usage: dotnet Update.dll [OPTION]\n");
            Console.WriteLine("Options:");
            Console.WriteLine("  -c, --check      Check for updates");
            Console.WriteLine("  -b, --backup     Create full backup");
            Console.WriteLine("  -m, --migrate    Migrate configurations");
            Console.WriteLine("  -r, --restore    Restore from backup");
            Console.WriteLine("  -v, --version    Display version information");
            Console.WriteLine("  -h, --help       Display this help message");
            Console.WriteLine("\nWithout options, the tool runs in interactive mode.");
        }
    }

    public class UpdateConfiguration
    {
        public string UpdateServerUrl { get; set; }
        public bool CheckForPreReleases { get; set; }
        public bool AutoBackup { get; set; }
        public int BackupRetentionDays { get; set; }
        public bool Debug { get; set; }
        public string CurrentVersion { get; set; }
    }

    public class BackupMetadata
    {
        public string Timestamp { get; set; }
        public string Version { get; set; }
        public int ItemCount { get; set; }
        public string BackupPath { get; set; }
    }
}