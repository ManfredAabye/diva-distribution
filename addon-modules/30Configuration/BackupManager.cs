using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;

namespace MetaverseInk.Configuration
{
    /// <summary>
    /// Manages configuration file backups
    /// </summary>
    public static class BackupManager
    {
        private static readonly string BackupDirectory = "config-backups";
        
        /// <summary>
        /// Creates a comprehensive backup of all configuration files
        /// </summary>
        public static void CreateConfigBackup(ConfigurationSettings settings)
        {
            Console.WriteLine("\n→ Creating configuration backup...");
            
            try
            {
                if (!Directory.Exists(BackupDirectory))
                {
                    Directory.CreateDirectory(BackupDirectory);
                }
                
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                
                // Define all configuration files to backup
                var filesToBackup = GetConfigFilesToBackup();
                
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
                            string backupFileName = $"{item.Key}.bak_{timestamp}";
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
                
                // Create compressed archive
                CreateBackupArchive(filesToBackup, timestamp, settings, backedUpCount, skippedCount, backedUpFiles.Count);
                
                // Clean up old backups
                CleanupOldBackups();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Backup failed: {ex.Message}");
                Console.ResetColor();
                if (settings?.Debug == true)
                {
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }
        }
        
        /// <summary>
        /// Creates a backup of a single file
        /// </summary>
        public static void BackupSingleFile(string filePath)
        {
            if (!File.Exists(filePath))
                return;
            
            try
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupFileName = $"{filePath}.bak_{timestamp}";
                File.Copy(filePath, backupFileName, true);
                
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"  → Backup: {Path.GetFileName(backupFileName)}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"  ⚠ Backup failed: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        /// <summary>
        /// Restores configuration from a backup archive
        /// </summary>
        public static void RestoreFromBackup()
        {
            Console.WriteLine("\n╔════════════════════════════════════╗");
            Console.WriteLine("║   Restore from Backup              ║");
            Console.WriteLine("╚════════════════════════════════════╝\n");
            
            if (!Directory.Exists(BackupDirectory))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠ No backup directory found.");
                Console.ResetColor();
                return;
            }
            
            var backupFiles = Directory.GetFiles(BackupDirectory, "config_backup_*.zip")
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.CreationTime)
                .ToList();
            
            if (backupFiles.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠ No backups found.");
                Console.ResetColor();
                return;
            }
            
            Console.WriteLine("Available backups:");
            for (int i = 0; i < backupFiles.Count; i++)
            {
                Console.WriteLine($"  [{i + 1}] {backupFiles[i].Name} ({FormatBytes(backupFiles[i].Length)}) - {backupFiles[i].CreationTime}");
            }
            
            Console.Write("\nSelect backup to restore (or 0 to cancel): ");
            if (int.TryParse(Console.ReadLine(), out int selection) && selection > 0 && selection <= backupFiles.Count)
            {
                var selectedBackup = backupFiles[selection - 1];
                
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"\n⚠ This will overwrite current configuration. Continue? (y/n): ");
                Console.ResetColor();
                
                if (Console.ReadLine()?.ToLower() == "y")
                {
                    try
                    {
                        string tempDir = Path.Combine(Path.GetTempPath(), $"opensim_restore_{DateTime.Now:yyyyMMddHHmmss}");
                        ZipFile.ExtractToDirectory(selectedBackup.FullName, tempDir);
                        
                        // Restore files
                        foreach (var file in Directory.GetFiles(tempDir))
                        {
                            string fileName = Path.GetFileName(file);
                            if (fileName != "backup_info.json")
                            {
                                File.Copy(file, fileName, true);
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"✓ Restored {fileName}");
                                Console.ResetColor();
                            }
                        }
                        
                        Directory.Delete(tempDir, true);
                        
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\n✓ Configuration restored successfully!");
                        Console.ResetColor();
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"❌ Restore failed: {ex.Message}");
                        Console.ResetColor();
                    }
                }
            }
        }
        
        private static Dictionary<string, string> GetConfigFilesToBackup()
        {
            return new Dictionary<string, string>
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
                { "ConfigureSettings.json", "Configure tool settings" }
            };
        }
        
        private static void CreateBackupArchive(Dictionary<string, string> filesToBackup, string timestamp, 
            ConfigurationSettings settings, int backedUpCount, int skippedCount, int individualBackupCount)
        {
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
                WorldName = settings.WorldName,
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
            Console.WriteLine($"  Individual backups: {individualBackupCount} files with .bak_{timestamp} extension");
            Console.ResetColor();
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
                
                // Keep last 10 backups
                if (backupFiles.Count > 10)
                {
                    int deletedCount = 0;
                    foreach (var oldBackup in backupFiles.Skip(10))
                    {
                        oldBackup.Delete();
                        deletedCount++;
                    }
                    
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"  Cleaned up {deletedCount} old backup(s)");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"⚠ Backup cleanup failed: {ex.Message}");
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
    }
}
