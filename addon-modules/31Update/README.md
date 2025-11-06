# 🔄 Diva Distribution Update Tool für .NET 8

Ein robustes, vollautomatisches Update- und Backup-Tool für OpenSim Diva Distribution.

## ✨ Features

### 🔍 Update-Prüfung

- Automatische Prüfung auf neue Versionen über GitHub API
- Unterstützung für Pre-Release-Versionen
- Intelligenter Versions-Vergleich
- Anzeige von Release Notes
- Download-Optionen mit Größenangaben

### 💾 Backup-System

- Vollautomatische Backups aller wichtigen Konfigurationen
- ZIP-Kompression für platzsparende Archivierung
- Metadaten-Tracking (Version, Zeitstempel, Item-Anzahl)
- Automatische Bereinigung alter Backups (konfigurierbar)
- Backup beinhaltet:
  - `config-include/` - Konfigurationsdateien
  - `Regions/` - Region-Konfigurationen
  - `bin/OpenSim.ini` - OpenSim Hauptkonfiguration
  - `bin/Robust.ini` - Robust Server Konfiguration
  - `bin/Wifi.ini` - Wifi Web-Interface Konfiguration

### 🔄 Wiederherstellung

- Auswahl aus vorhandenen Backups
- Vollständige Wiederherstellung mit einem Klick
- Sicherheitsabfrage vor Überschreiben
- Detaillierte Fortschrittsanzeige

### 🔧 Konfigurations-Migration

- Automatische Migration von .NET Framework zu .NET 8
- Intelligente Erkennung von Änderungsbedarf
- Automatisches Backup vor Migration
- Unterstützung mehrerer Konfigurationsdateien

## 🚀 Verwendung

### Interaktiver Modus

```bash
cd bin
dotnet Update.dll
```

Zeigt ein benutzerfreundliches Menü mit folgenden Optionen:

1.**Check for Updates** - Prüft auf neue Versionen
2.**Backup Configuration** - Erstellt vollständiges Backup
3.**Migrate Configurations** - Migriert Konfigurationen für .NET 8
4.**Restore from Backup** - Stellt aus Backup wieder her
5.**View Current Version** - Zeigt Versionsinformationen
6.**Settings** - Konfiguriert Tool-Einstellungen
0.**Exit** - Beendet das Tool

### Kommandozeilen-Modus

```bash
# Update-Prüfung
dotnet Update.dll --check
dotnet Update.dll -c

# Backup erstellen
dotnet Update.dll --backup
dotnet Update.dll -b

# Konfiguration migrieren
dotnet Update.dll --migrate
dotnet Update.dll -m

# Aus Backup wiederherstellen
dotnet Update.dll --restore
dotnet Update.dll -r

# Versionsinformationen
dotnet Update.dll --version
dotnet Update.dll -v

# Hilfe anzeigen
dotnet Update.dll --help
dotnet Update.dll -h
```

## ⚙️ Konfiguration

Die Konfiguration erfolgt über `UpdateConfig.json`:

```json
{
  "UpdateServerUrl": "https://api.github.com/repos/diva/diva-distribution/releases",
  "CheckForPreReleases": false,
  "AutoBackup": true,
  "BackupRetentionDays": 30,
  "Debug": false,
  "CurrentVersion": "1.0.0"
}
```

### Konfigurations-Parameter

| Parameter | Typ | Beschreibung |
|-----------|-----|--------------|
| `UpdateServerUrl` | string | GitHub API URL für Release-Informationen |
| `CheckForPreReleases` | bool | Prüft auch auf Pre-Release-Versionen |
| `AutoBackup` | bool | Erstellt automatisch Backup vor Migrationen |
| `BackupRetentionDays` | int | Tage, wie lange Backups aufbewahrt werden |
| `Debug` | bool | Zeigt detaillierte Debug-Informationen |
| `CurrentVersion` | string | Aktuell installierte Version |

### Einstellungen über Menü ändern

Im interaktiven Modus können Sie über "Settings" (Option 6) folgende Einstellungen ändern:

- Aktuelle Version setzen
- Pre-Release Updates aktivieren/deaktivieren
- Auto-Backup aktivieren/deaktivieren
- Backup-Aufbewahrungszeit ändern
- Debug-Modus aktivieren/deaktivieren

## 📁 Backup-Struktur

Backups werden im Verzeichnis `backups/` gespeichert:

```bash
backups/
├── backup_20251106_143022.zip
├── backup_20251105_091545.zip
└── backup_20251104_180312.zip
```

Jedes Backup enthält:

- Alle wichtigen Konfigurationsdateien und -ordner
- `backup_info.json` mit Metadaten:

  ```json
  {
    "Timestamp": "20251106_143022",
    "Version": "1.0.0",
    "ItemCount": 5,
    "BackupPath": "backups/backup_20251106_143022"
  }
  ```

## 🔒 Sicherheit

- ✅ Automatische Backups vor kritischen Operationen
- ✅ Sicherheitsabfragen vor Überschreiben
- ✅ Detaillierte Fehlerbehandlung
- ✅ Rollback-Möglichkeit durch Backup-System
- ✅ Keine automatischen Downloads ohne Bestätigung

## 🎨 Benutzeroberfläche

Das Tool bietet eine farbcodierte Konsolenausgabe:

- 🟢 **Grün**: Erfolgreiche Operationen
- 🟡 **Gelb**: Warnungen
- 🔴 **Rot**: Fehler
- 🔵 **Cyan**: Informationen

## 📋 Anforderungen

- .NET 8.0 Runtime oder höher
- Schreibrechte im Installationsverzeichnis
- Internetverbindung für Update-Prüfung

## 🐛 Fehlerbehandlung

Das Tool behandelt folgende Fehlerszenarien:

- Netzwerkfehler (keine Internetverbindung)
- Timeout bei API-Anfragen
- Fehlende Konfigurationsdateien
- Ungültige JSON-Daten
- Dateisystem-Fehler (Zugriffsprobleme)
- Unvollständige Backups

Im Debug-Modus werden zusätzlich Stack Traces angezeigt.

## 📝 Beispiel-Sitzung

```bash
╔══════════════════════════════════════════════════════════════╗
║       Diva Distribution Update Tool for .NET 8              ║
║       Version 1.0.0                                          ║
╚══════════════════════════════════════════════════════════════╝
Runtime: .NET 8.0.0
Current Date: 2025-11-06 14:30:22

✓ Configuration loaded successfully

╔════════════════════════════════════╗
║         Main Menu                  ║
╚════════════════════════════════════╝
1. Check for Updates
2. Backup Configuration
3. Migrate Configurations
4. Restore from Backup
5. View Current Version
6. Settings
0. Exit

Select an option: 1

╔════════════════════════════════════╗
║     Checking for Updates...        ║
╚════════════════════════════════════╝

→ Connecting to update server: https://api.github.com/repos/diva/diva-distribution/releases/latest
✓ Successfully connected to update server

📦 Latest Available Version: v2.0.0
📝 Release Name: Diva Distribution 2.0.0
📅 Published: 2025-11-01T12:00:00Z
🔗 URL: https://github.com/diva/diva-distribution/releases/tag/v2.0.0

✓ New version available! (1.0.0 → v2.0.0)

📋 Release Notes:
────────────────────────────────────────────────────────────
Major update with .NET 8 support and new features...
────────────────────────────────────────────────────────────

Would you like to see download options? (y/n): y

📦 Available Downloads:
1. diva-distribution-2.0.0.zip (45.2 MB)
   URL: https://github.com/diva/diva-distribution/releases/download/v2.0.0/diva-distribution-2.0.0.zip
```

## 🔧 Entwicklung

Das Tool ist modular aufgebaut und kann einfach erweitert werden:

- `CheckForUpdates()` - Update-Prüfung
- `CreateFullBackup()` - Backup-Erstellung
- `RestoreFromBackup()` - Backup-Wiederherstellung
- `MigrateConfigs()` - Konfigurations-Migration
- `ConfigureSettings()` - Einstellungsverwaltung

## 📄 Lizenz

Teil der OpenSim Diva Distribution - siehe Hauptprojekt für Lizenzinformationen.
