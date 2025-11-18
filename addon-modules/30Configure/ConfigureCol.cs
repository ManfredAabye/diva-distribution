/*
 * Copyright (c) Diva Configure Team. All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Diva.Configure
{
    /// <summary>
    /// Sammlung wiederverwendbarer Funktionen für Konfigurationsverwaltung
    /// Collection of reusable functions for configuration management
    /// </summary>
    public static class ConfigureCol
    {
        #region Backup Operations

        /// <summary>
        /// Erstellt ein Backup einer Datei mit Zeitstempel
        /// </summary>
        public static bool BackupFile(string filePath, string backupDir = null)
        {
            if (!File.Exists(filePath))
                return false;

            try
            {
                string fileName = Path.GetFileName(filePath);
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupPath = backupDir ?? Path.Combine(Path.GetDirectoryName(filePath), "backups");
                
                if (!Directory.Exists(backupPath))
                    Directory.CreateDirectory(backupPath);

                string backupFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{timestamp}{Path.GetExtension(fileName)}";
                string backupFullPath = Path.Combine(backupPath, backupFileName);

                File.Copy(filePath, backupFullPath, true);
                Console.WriteLine($"[BACKUP] Created: {backupFullPath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BACKUP ERROR] {filePath}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Erstellt Backup eines ganzen Verzeichnisses
        /// </summary>
        public static bool BackupDirectory(string dirPath, string backupDir)
        {
            if (!Directory.Exists(dirPath))
                return false;

            try
            {
                string dirName = Path.GetFileName(dirPath);
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupFullPath = Path.Combine(backupDir, $"{dirName}_{timestamp}");

                if (!Directory.Exists(backupDir))
                    Directory.CreateDirectory(backupDir);

                CopyDirectory(dirPath, backupFullPath);
                Console.WriteLine($"[BACKUP] Created directory: {backupFullPath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BACKUP ERROR] {dirPath}: {ex.Message}");
                return false;
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

            foreach (string dir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(destDir, Path.GetFileName(dir));
                CopyDirectory(dir, destSubDir);
            }
        }

        #endregion

        #region File Operations

        /// <summary>
        /// Liest gesamten Dateiinhalt
        /// </summary>
        public static string ReadFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"[READ] File not found: {filePath}");
                    return null;
                }

                return File.ReadAllText(filePath, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[READ ERROR] {filePath}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Schreibt Inhalt in Datei
        /// </summary>
        public static bool WriteFile(string filePath, string content, bool createBackup = true)
        {
            try
            {
                if (createBackup && File.Exists(filePath))
                    BackupFile(filePath);

                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                File.WriteAllText(filePath, content, Encoding.UTF8);
                Console.WriteLine($"[WRITE] Success: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WRITE ERROR] {filePath}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Löscht eine Datei
        /// </summary>
        public static bool DeleteFile(string filePath, bool createBackup = true)
        {
            try
            {
                if (!File.Exists(filePath))
                    return false;

                if (createBackup)
                    BackupFile(filePath);

                File.Delete(filePath);
                Console.WriteLine($"[DELETE] Removed: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DELETE ERROR] {filePath}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Benennt eine Datei um
        /// </summary>
        public static bool RenameFile(string oldPath, string newPath, bool createBackup = true)
        {
            try
            {
                if (!File.Exists(oldPath))
                    return false;

                if (createBackup)
                    BackupFile(oldPath);

                File.Move(oldPath, newPath);
                Console.WriteLine($"[RENAME] {oldPath} -> {newPath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RENAME ERROR] {oldPath}: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Line Operations

        /// <summary>
        /// Liest alle Zeilen aus einer Datei
        /// </summary>
        public static List<string> ReadLines(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return new List<string>();

                return File.ReadAllLines(filePath, Encoding.UTF8).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[READ LINES ERROR] {filePath}: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Schreibt Zeilen in eine Datei
        /// </summary>
        public static bool WriteLines(string filePath, List<string> lines, bool createBackup = true)
        {
            try
            {
                if (createBackup && File.Exists(filePath))
                    BackupFile(filePath);

                File.WriteAllLines(filePath, lines, Encoding.UTF8);
                Console.WriteLine($"[WRITE LINES] Success: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WRITE LINES ERROR] {filePath}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Findet Zeile mit bestimmtem Pattern
        /// </summary>
        public static int FindLine(List<string> lines, string pattern, bool isRegex = false)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                if (isRegex)
                {
                    if (Regex.IsMatch(lines[i], pattern))
                        return i;
                }
                else
                {
                    if (lines[i].Contains(pattern))
                        return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Ersetzt eine Zeile
        /// </summary>
        public static bool ReplaceLine(List<string> lines, int lineIndex, string newLine)
        {
            if (lineIndex < 0 || lineIndex >= lines.Count)
                return false;

            lines[lineIndex] = newLine;
            return true;
        }

        /// <summary>
        /// Löscht eine Zeile
        /// </summary>
        public static bool DeleteLine(List<string> lines, int lineIndex)
        {
            if (lineIndex < 0 || lineIndex >= lines.Count)
                return false;

            lines.RemoveAt(lineIndex);
            return true;
        }

        /// <summary>
        /// Fügt eine Zeile hinzu
        /// </summary>
        public static void InsertLine(List<string> lines, int lineIndex, string newLine)
        {
            if (lineIndex < 0)
                lineIndex = 0;
            if (lineIndex > lines.Count)
                lineIndex = lines.Count;

            lines.Insert(lineIndex, newLine);
        }

        /// <summary>
        /// Kommentiert eine Zeile aus (fügt ; am Anfang hinzu)
        /// </summary>
        public static bool CommentLine(List<string> lines, int lineIndex, string commentChar = ";")
        {
            if (lineIndex < 0 || lineIndex >= lines.Count)
                return false;

            string line = lines[lineIndex].TrimStart();
            
            // Prüfe ob bereits kommentiert
            if (line.StartsWith(commentChar))
                return true; // Bereits kommentiert

            // Füge Kommentar am Anfang hinzu (mit ursprünglichem Whitespace)
            string leadingWhitespace = lines[lineIndex].Substring(0, lines[lineIndex].Length - line.Length);
            lines[lineIndex] = leadingWhitespace + commentChar + " " + line;
            
            return true;
        }

        /// <summary>
        /// Entfernt Kommentar von einer Zeile
        /// </summary>
        public static bool UncommentLine(List<string> lines, int lineIndex, string commentChar = ";")
        {
            if (lineIndex < 0 || lineIndex >= lines.Count)
                return false;

            string line = lines[lineIndex].TrimStart();
            
            // Prüfe ob kommentiert
            if (!line.StartsWith(commentChar))
                return true; // Nicht kommentiert, nichts zu tun

            // Entferne Kommentar (inklusive optionalem Leerzeichen nach Kommentarzeichen)
            string leadingWhitespace = lines[lineIndex].Substring(0, lines[lineIndex].Length - line.Length);
            string uncommented = line.Substring(commentChar.Length).TrimStart();
            lines[lineIndex] = leadingWhitespace + uncommented;
            
            return true;
        }

        /// <summary>
        /// Prüft ob eine Zeile kommentiert ist
        /// </summary>
        public static bool IsLineCommented(string line, string commentChar = ";")
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;

            string trimmed = line.TrimStart();
            return trimmed.StartsWith(commentChar);
        }

        /// <summary>
        /// Kommentiert/Entkommentiert eine Zeile (Toggle)
        /// </summary>
        public static bool ToggleLineComment(List<string> lines, int lineIndex, string commentChar = ";")
        {
            if (lineIndex < 0 || lineIndex >= lines.Count)
                return false;

            if (IsLineCommented(lines[lineIndex], commentChar))
                return UncommentLine(lines, lineIndex, commentChar);
            else
                return CommentLine(lines, lineIndex, commentChar);
        }

        /// <summary>
        /// Kommentiert eine INI-Zeile mit Key aus
        /// </summary>
        public static bool CommentIniKey(string filePath, string section, string key, bool createBackup = true)
        {
            var lines = ReadLines(filePath);
            if (lines == null)
                return false;

            bool inSection = false;
            int keyIndex = -1;

            // Suche Section und Key
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();

                // Section gefunden
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    string currentSection = line.Substring(1, line.Length - 2).Trim();
                    inSection = currentSection.Equals(section, StringComparison.OrdinalIgnoreCase);
                    continue;
                }

                // Key in der richtigen Section gefunden
                if (inSection && line.Contains("="))
                {
                    string currentKey = line.Split('=')[0].Trim();
                    if (currentKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        keyIndex = i;
                        break;
                    }
                }
            }

            // Key gefunden?
            if (keyIndex == -1)
            {
                Console.WriteLine($"[COMMENT INI KEY] Key not found: [{section}] {key}");
                return false;
            }

            // Kommentiere Zeile
            if (!CommentLine(lines, keyIndex))
                return false;

            return WriteLines(filePath, lines, createBackup);
        }

        /// <summary>
        /// Entkommentiert eine INI-Zeile mit Key
        /// </summary>
        public static bool UncommentIniKey(string filePath, string section, string key, bool createBackup = true)
        {
            var lines = ReadLines(filePath);
            if (lines == null)
                return false;

            bool inSection = false;
            int keyIndex = -1;

            // Suche Section und Key (auch in kommentierten Zeilen)
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();

                // Section gefunden
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    string currentSection = line.Substring(1, line.Length - 2).Trim();
                    inSection = currentSection.Equals(section, StringComparison.OrdinalIgnoreCase);
                    continue;
                }

                // Key in der richtigen Section gefunden (auch wenn kommentiert)
                if (inSection)
                {
                    string uncommented = line.StartsWith(";") ? line.Substring(1).TrimStart() : line;
                    
                    if (uncommented.Contains("="))
                    {
                        string currentKey = uncommented.Split('=')[0].Trim();
                        if (currentKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            keyIndex = i;
                            break;
                        }
                    }
                }
            }

            // Key gefunden?
            if (keyIndex == -1)
            {
                Console.WriteLine($"[UNCOMMENT INI KEY] Key not found: [{section}] {key}");
                return false;
            }

            // Entkommentiere Zeile
            if (!UncommentLine(lines, keyIndex))
                return false;

            return WriteLines(filePath, lines, createBackup);
        }

        #endregion

        #region INI Configuration Operations

        /// <summary>
        /// Setzt einen INI-Wert in einer Konfigurationsdatei
        /// </summary>
        public static bool SetIniValue(string filePath, string section, string key, string value, bool createBackup = true)
        {
            var lines = ReadLines(filePath);
            if (lines == null)
                return false;

            bool inSection = false;
            int sectionIndex = -1;
            int keyIndex = -1;

            // Suche Section und Key
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();

                // Section gefunden
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    string currentSection = line.Substring(1, line.Length - 2);
                    if (currentSection.Equals(section, StringComparison.OrdinalIgnoreCase))
                    {
                        inSection = true;
                        sectionIndex = i;
                    }
                    else if (inSection)
                    {
                        // Nächste Section erreicht, Key nicht gefunden
                        break;
                    }
                }
                // Key gefunden
                else if (inSection && !string.IsNullOrWhiteSpace(line) && !line.StartsWith(";"))
                {
                    if (line.Contains("="))
                    {
                        string currentKey = line.Substring(0, line.IndexOf("=")).Trim();
                        if (currentKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            keyIndex = i;
                            break;
                        }
                    }
                }
            }

            // Key existiert - ersetzen
            if (keyIndex >= 0)
            {
                lines[keyIndex] = $"    {key} = {value}";
            }
            // Section existiert, Key nicht - hinzufügen
            else if (sectionIndex >= 0)
            {
                // Finde Ende der Section
                int insertIndex = sectionIndex + 1;
                while (insertIndex < lines.Count && 
                       !lines[insertIndex].Trim().StartsWith("[") &&
                       lines[insertIndex].Trim().Length > 0)
                {
                    insertIndex++;
                }
                lines.Insert(insertIndex, $"    {key} = {value}");
            }
            // Section existiert nicht - erstellen
            else
            {
                lines.Add($"\n[{section}]");
                lines.Add($"    {key} = {value}");
            }

            return WriteLines(filePath, lines, createBackup);
        }

        /// <summary>
        /// Liest einen INI-Wert aus einer Konfigurationsdatei
        /// </summary>
        public static string GetIniValue(string filePath, string section, string key)
        {
            var lines = ReadLines(filePath);
            if (lines == null)
                return null;

            bool inSection = false;

            foreach (var line in lines)
            {
                string trimmed = line.Trim();

                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                {
                    string currentSection = trimmed.Substring(1, trimmed.Length - 2);
                    inSection = currentSection.Equals(section, StringComparison.OrdinalIgnoreCase);
                }
                else if (inSection && trimmed.Contains("=") && !trimmed.StartsWith(";"))
                {
                    int equalIndex = trimmed.IndexOf("=");
                    string currentKey = trimmed.Substring(0, equalIndex).Trim();
                    
                    if (currentKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        return trimmed.Substring(equalIndex + 1).Trim();
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Kommentiert eine Zeile aus (fügt ; hinzu)
        /// </summary>
        public static bool CommentLine(string filePath, string section, string key, bool createBackup = true)
        {
            var lines = ReadLines(filePath);
            if (lines == null)
                return false;

            bool inSection = false;

            for (int i = 0; i < lines.Count; i++)
            {
                string trimmed = lines[i].Trim();

                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                {
                    string currentSection = trimmed.Substring(1, trimmed.Length - 2);
                    inSection = currentSection.Equals(section, StringComparison.OrdinalIgnoreCase);
                }
                else if (inSection && trimmed.Contains("=") && !trimmed.StartsWith(";"))
                {
                    int equalIndex = trimmed.IndexOf("=");
                    string currentKey = trimmed.Substring(0, equalIndex).Trim();
                    
                    if (currentKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        lines[i] = "    ; " + trimmed;
                        return WriteLines(filePath, lines, createBackup);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Entkommentiert eine Zeile (entfernt ;)
        /// </summary>
        public static bool UncommentLine(string filePath, string section, string key, bool createBackup = true)
        {
            var lines = ReadLines(filePath);
            if (lines == null)
                return false;

            bool inSection = false;

            for (int i = 0; i < lines.Count; i++)
            {
                string trimmed = lines[i].Trim();

                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                {
                    string currentSection = trimmed.Substring(1, trimmed.Length - 2);
                    inSection = currentSection.Equals(section, StringComparison.OrdinalIgnoreCase);
                }
                else if (inSection && trimmed.StartsWith(";"))
                {
                    string uncommented = trimmed.TrimStart(';').Trim();
                    if (uncommented.Contains("="))
                    {
                        int equalIndex = uncommented.IndexOf("=");
                        string currentKey = uncommented.Substring(0, equalIndex).Trim();
                        
                        if (currentKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            lines[i] = "    " + uncommented;
                            return WriteLines(filePath, lines, createBackup);
                        }
                    }
                }
            }

            return false;
        }

        #endregion

        #region JSON Operations

        /// <summary>
        /// Liest JSON-Konfiguration
        /// </summary>
        public static T ReadJsonConfig<T>(string filePath) where T : class
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"[JSON] File not found: {filePath}");
                    return null;
                }

                string json = File.ReadAllText(filePath, Encoding.UTF8);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };

                return JsonSerializer.Deserialize<T>(json, options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JSON ERROR] {filePath}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Schreibt JSON-Konfiguration
        /// </summary>
        public static bool WriteJsonConfig<T>(string filePath, T config, bool createBackup = true) where T : class
        {
            try
            {
                if (createBackup && File.Exists(filePath))
                    BackupFile(filePath);

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNameCaseInsensitive = true
                };

                string json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(filePath, json, Encoding.UTF8);
                
                Console.WriteLine($"[JSON] Saved: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JSON ERROR] {filePath}: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validiert IP-Adresse
        /// </summary>
        public static bool IsValidIP(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return false;

            if (ip.Equals("SYSTEMIP", StringComparison.OrdinalIgnoreCase))
                return true;

            var parts = ip.Split('.');
            if (parts.Length != 4)
                return false;

            return parts.All(part => byte.TryParse(part, out _));
        }

        /// <summary>
        /// Validiert Port-Nummer
        /// </summary>
        public static bool IsValidPort(string port)
        {
            if (int.TryParse(port, out int portNum))
                return portNum > 0 && portNum <= 65535;
            return false;
        }

        /// <summary>
        /// Validiert UUID
        /// </summary>
        public static bool IsValidUUID(string uuid)
        {
            return Guid.TryParse(uuid, out _);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Erstellt Verzeichnis falls nicht vorhanden
        /// </summary>
        public static bool EnsureDirectory(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    Console.WriteLine($"[DIRECTORY] Created: {path}");
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DIRECTORY ERROR] {path}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gibt Liste aller .ini Dateien in einem Verzeichnis zurück
        /// </summary>
        public static List<string> GetIniFiles(string directory)
        {
            try
            {
                if (!Directory.Exists(directory))
                    return new List<string>();

                return Directory.GetFiles(directory, "*.ini", SearchOption.TopDirectoryOnly).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GET FILES ERROR] {directory}: {ex.Message}");
                return new List<string>();
            }
        }

        #endregion

        #region Region Management

        /// <summary>
        /// Generiert Dateinamen aus Region Name
        /// My Region -> myregion.ini
        /// </summary>
        public static string GenerateRegionFileName(string regionName)
        {
            // Entferne ungültige Zeichen und ersetze Leerzeichen
            string fileName = regionName.ToLower()
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("_", "");

            // Entferne alle nicht-alphanumerischen Zeichen
            fileName = new string(fileName.Where(c => char.IsLetterOrDigit(c)).ToArray());

            return fileName + ".ini";
        }

        /// <summary>
        /// Liest Region-Daten aus INI-Datei
        /// </summary>
        public static RegionData ReadRegionFromFile(string filePath)
        {
            try
            {
                var region = new RegionData
                {
                    RegionName = GetIniValue(filePath, "Region", "RegionName"),
                    RegionUUID = GetIniValue(filePath, "Region", "RegionUUID"),
                    Location = GetIniValue(filePath, "Region", "Location"),
                    InternalPort = GetIniValue(filePath, "Region", "InternalPort"),
                    ExternalHostName = GetIniValue(filePath, "Region", "ExternalHostName"),
                    SizeX = GetIniValue(filePath, "Region", "SizeX") ?? "256",
                    SizeY = GetIniValue(filePath, "Region", "SizeY") ?? "256",
                    MaxPrims = GetIniValue(filePath, "Region", "MaxPrims") ?? "45000",
                    MaxAgents = GetIniValue(filePath, "Region", "MaxAgents") ?? "100"
                };

                return region;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Erstellt oder aktualisiert Region INI Datei
        /// </summary>
        public static bool CreateOrUpdateRegionFile(string filePath, RegionData region, bool backup = true)
        {
            try
            {
                EnsureDirectory(Path.GetDirectoryName(filePath));

                if (!File.Exists(filePath))
                {
                    // Erstelle neue Datei mit Template
                    var iniContent = GenerateRegionIniTemplate(region);
                    return WriteFile(filePath, iniContent, backup);
                }
                else
                {
                    // Backup existierende Datei
                    if (backup)
                        BackupFile(filePath);

                    // Update existierende Datei
                    SetIniValue(filePath, "Region", "RegionName", region.RegionName);
                    SetIniValue(filePath, "Region", "Location", region.Location);
                    SetIniValue(filePath, "Region", "InternalPort", region.InternalPort);
                    SetIniValue(filePath, "Region", "ExternalHostName", region.ExternalHostName);
                    SetIniValue(filePath, "Region", "SizeX", region.SizeX);
                    SetIniValue(filePath, "Region", "SizeY", region.SizeY);
                    SetIniValue(filePath, "Region", "MaxPrims", region.MaxPrims);
                    SetIniValue(filePath, "Region", "MaxAgents", region.MaxAgents);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Creating/updating region file: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Generiert Region INI Template
        /// </summary>
        private static string GenerateRegionIniTemplate(RegionData region)
        {
            return $@"; Region Configuration File
; Generated by Diva Configure on {DateTime.Now:yyyy-MM-dd HH:mm:ss}

[Region]
    RegionName = {region.RegionName}
    RegionUUID = {region.RegionUUID}
    Location = {region.Location}
    InternalPort = {region.InternalPort}
    ExternalHostName = {region.ExternalHostName}
    
    ; Region Size
    SizeX = {region.SizeX}
    SizeY = {region.SizeY}
    
    ; Limits
    MaxPrims = {region.MaxPrims}
    MaxAgents = {region.MaxAgents}
    
    ; Optional Settings
    ; MaptileStaticUUID = ""00000000-0000-0000-0000-000000000000""
    ; MaptileStaticFile = ""maptile.jpg""
    
[Estate]
    ; Estate settings can be configured here
    ; EstateName = ""My Estate""
    ; EstateOwner = ""00000000-0000-0000-0000-000000000000""

[Startup]
    ; Region-specific startup settings
    ; physics = OpenDynamicsEngine
    ; meshing = Meshmerizer
";
        }

        /// <summary>
        /// Gibt alle Region-Dateien zurück
        /// </summary>
        public static List<string> GetAllRegionFiles(string regionsPath)
        {
            return GetIniFiles(regionsPath);
        }

        /// <summary>
        /// Löscht Region-Datei
        /// </summary>
        public static bool DeleteRegionFile(string filePath, bool backup = true)
        {
            return DeleteFile(filePath, backup);
        }

        /// <summary>
        /// Erstellt interaktiv eine neue Region
        /// </summary>
        public static RegionData CreateRegionInteractive(ConfigureSettings settings)
        {
            Console.WriteLine("\n╔═══════════════════════════════════════════════════╗");
            Console.WriteLine("║             Create New Region                      ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════╝");

            var regionData = new RegionData();

            // Region Name
            Console.Write("\nRegion Name: ");
            regionData.RegionName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(regionData.RegionName))
            {
                Console.WriteLine("[ERROR] Region name cannot be empty!");
                return null;
            }

            // Region UUID
            Console.Write($"Region UUID (press Enter for random): ");
            string uuidInput = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(uuidInput))
            {
                regionData.RegionUUID = Guid.NewGuid().ToString();
                Console.WriteLine($"Generated UUID: {regionData.RegionUUID}");
            }
            else if (IsValidUUID(uuidInput))
            {
                regionData.RegionUUID = uuidInput;
            }
            else
            {
                Console.WriteLine("[ERROR] Invalid UUID!");
                return null;
            }

            // Location
            Console.Write($"Location (X,Y) [1000,1000]: ");
            string locationInput = Console.ReadLine();
            regionData.Location = !string.IsNullOrWhiteSpace(locationInput) ? locationInput : "1000,1000";

            // Internal Port
            Console.Write($"Internal Port [9000]: ");
            string portInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(portInput) && IsValidPort(portInput))
                regionData.InternalPort = portInput;
            else
                regionData.InternalPort = "9000";

            // External Host Name
            Console.Write($"External HostName [{settings.GlobalSettings.ExternalHostName}]: ");
            string hostnameInput = Console.ReadLine();
            regionData.ExternalHostName = !string.IsNullOrWhiteSpace(hostnameInput) 
                ? hostnameInput 
                : settings.GlobalSettings.ExternalHostName;

            // Size X
            Console.Write($"Size X (in meters) [256]: ");
            string sizeXInput = Console.ReadLine();
            regionData.SizeX = !string.IsNullOrWhiteSpace(sizeXInput) ? sizeXInput : "256";

            // Size Y
            Console.Write($"Size Y (in meters) [256]: ");
            string sizeYInput = Console.ReadLine();
            regionData.SizeY = !string.IsNullOrWhiteSpace(sizeYInput) ? sizeYInput : "256";

            // Max Prims
            Console.Write($"Max Prims [45000]: ");
            string maxPrimsInput = Console.ReadLine();
            regionData.MaxPrims = !string.IsNullOrWhiteSpace(maxPrimsInput) ? maxPrimsInput : "45000";

            // Max Agents
            Console.Write($"Max Agents [100]: ");
            string maxAgentsInput = Console.ReadLine();
            regionData.MaxAgents = !string.IsNullOrWhiteSpace(maxAgentsInput) ? maxAgentsInput : "100";

            return regionData;
        }

        /// <summary>
        /// Bearbeitet interaktiv eine existierende Region
        /// </summary>
        public static RegionData EditRegionInteractive(RegionData region)
        {
            Console.WriteLine($"\n[INFO] Editing: {region.RegionName}");
            Console.WriteLine("(Press Enter to keep current value)");

            // Edit fields
            Console.Write($"\nRegion Name [{region.RegionName}]: ");
            string newName = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newName))
                region.RegionName = newName;

            Console.Write($"Location (X,Y) [{region.Location}]: ");
            string newLocation = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newLocation))
                region.Location = newLocation;

            Console.Write($"Internal Port [{region.InternalPort}]: ");
            string newPort = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newPort) && IsValidPort(newPort))
                region.InternalPort = newPort;

            Console.Write($"External HostName [{region.ExternalHostName}]: ");
            string newHostname = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newHostname))
                region.ExternalHostName = newHostname;

            Console.Write($"Max Prims [{region.MaxPrims}]: ");
            string newMaxPrims = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newMaxPrims))
                region.MaxPrims = newMaxPrims;

            Console.Write($"Max Agents [{region.MaxAgents}]: ");
            string newMaxAgents = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newMaxAgents))
                region.MaxAgents = newMaxAgents;

            return region;
        }

        /// <summary>
        /// Listet alle Regionen tabellarisch auf
        /// </summary>
        public static void ListAllRegions(string regionsPath)
        {
            Console.WriteLine("\n╔═══════════════════════════════════════════════════╗");
            Console.WriteLine("║              All Regions                           ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════╝");

            var regionFiles = GetAllRegionFiles(regionsPath);

            if (regionFiles.Count == 0)
            {
                Console.WriteLine("\n[INFO] No region files found.");
            }
            else
            {
                Console.WriteLine($"\nFound {regionFiles.Count} region(s):\n");
                Console.WriteLine("┌─────────────────────┬──────────────────────────────────────┬──────────┬────────┐");
                Console.WriteLine("│ Region Name         │ UUID                                 │ Location │ Port   │");
                Console.WriteLine("├─────────────────────┼──────────────────────────────────────┼──────────┼────────┤");

                foreach (var file in regionFiles)
                {
                    var region = ReadRegionFromFile(file);
                    if (region != null)
                    {
                        string name = region.RegionName.PadRight(19);
                        if (name.Length > 19) name = name.Substring(0, 19);

                        string uuid = region.RegionUUID.PadRight(36);
                        string location = region.Location.PadRight(8);
                        string port = region.InternalPort.PadRight(6);

                        Console.WriteLine($"│ {name} │ {uuid} │ {location} │ {port} │");
                    }
                }
                Console.WriteLine("└─────────────────────┴──────────────────────────────────────┴──────────┴────────┘");
            }
        }

        /// <summary>
        /// Klont eine Region mit neuen Werten
        /// </summary>
        public static RegionData CloneRegion(RegionData sourceRegion, string newName = null, string newLocation = null, string newPort = null)
        {
            var clonedRegion = new RegionData
            {
                RegionName = newName ?? (sourceRegion.RegionName + " Copy"),
                RegionUUID = Guid.NewGuid().ToString(),
                Location = newLocation ?? sourceRegion.Location,
                InternalPort = newPort ?? sourceRegion.InternalPort,
                ExternalHostName = sourceRegion.ExternalHostName,
                SizeX = sourceRegion.SizeX,
                SizeY = sourceRegion.SizeY,
                MaxPrims = sourceRegion.MaxPrims,
                MaxAgents = sourceRegion.MaxAgents
            };

            return clonedRegion;
        }

        #endregion
    }
}
