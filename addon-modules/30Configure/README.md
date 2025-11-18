# Diva Configure - OpenSim Configuration Management System

Ein modulares Konfigurationsverwaltungssystem für OpenSimulator mit integriertem Region-Management.

## Struktur

```bash
30Configure/
├── Configure.cs          - Hauptprogramm mit Menü und Region-Management
├── Configure.csproj      - .NET 8.0 Projekt
├── Configure.json        - Globale Voreinstellungen
├── ConfigureCol.cs       - Wiederverwendbare Funktionen/Werkzeuge
└── Spezifische Konfiguratoren:
    ├── ConfigureSDL.cs   - Standalone
    ├── ConfigureSDLHG.cs - StandaloneHG (Hypergrid)
    ├── ConfigureRO.cs    - Robust Grid
    ├── ConfigureROHG.cs  - RobustHG (Hypergrid) [Placeholder]
    ├── ConfigureROR.cs   - Robust + Regionen [Placeholder]
    └── ConfigureROHGR.cs - RobustHG + Regionen [Placeholder]
```

## Features

### ConfigureCol.cs - Wiederverwendbare Funktionen

#### Backup Operations

- `BackupFile(filePath)` - Datei-Backup mit Zeitstempel
- `BackupDirectory(dirPath)` - Verzeichnis-Backup
- `RestoreBackup(backupPath, targetPath)` - Backup wiederherstellen

#### File Operations

- `ReadFile(filePath)` - Datei lesen
- `WriteFile(filePath, content, backup)` - Datei schreiben
- `DeleteFile(filePath, backup)` - Datei löschen
- `RenameFile(oldPath, newPath)` - Datei umbenennen
- `CopyFile(source, destination)` - Datei kopieren

#### Line Operations

- `ReadLines(filePath)` - Zeilen lesen
- `WriteLines(filePath, lines, backup)` - Zeilen schreiben
- `FindLine(lines, pattern, isRegex)` - Zeile finden (mit Regex-Unterstützung)
- `ReplaceLine(lines, lineIndex, newLine)` - Zeile ersetzen
- `DeleteLine(lines, lineIndex)` - Zeile löschen
- `InsertLine(lines, lineIndex, newLine)` - Zeile einfügen
- `CommentLine(lines, lineIndex, commentChar)` - Zeile kommentieren
- `UncommentLine(lines, lineIndex, commentChar)` - Zeile entkommentieren
- `IsLineCommented(line, commentChar)` - Prüft ob Zeile kommentiert ist
- `ToggleLineComment(lines, lineIndex, commentChar)` - Kommentar umschalten

#### INI Configuration Operations

- `SetIniValue(filePath, section, key, value, backup)` - INI-Wert setzen
- `GetIniValue(filePath, section, key)` - INI-Wert lesen
- `CommentIniKey(filePath, section, key, backup)` - INI-Key kommentieren
- `UncommentIniKey(filePath, section, key, backup)` - INI-Key entkommentieren
- `SectionExists(filePath, section)` - Prüft ob Section existiert
- `KeyExists(filePath, section, key)` - Prüft ob Key existiert

#### JSON Operations

- `ReadJsonConfig<T>(filePath)` - JSON-Konfiguration lesen
- `WriteJsonConfig<T>(filePath, config, backup)` - JSON-Konfiguration schreiben

#### Region Management

- `GenerateRegionFileName(regionName)` - Generiert Dateinamen (My Region → myregion.ini)
- `ReadRegionFromFile(filePath)` - Liest Region-Daten aus INI
- `CreateOrUpdateRegionFile(filePath, region, backup)` - Erstellt/aktualisiert Region-Datei
- `CreateRegionInteractive(settings)` - Interaktive Region-Erstellung
- `EditRegionInteractive(region)` - Interaktive Region-Bearbeitung
- `ListAllRegions(regionsPath)` - Zeigt alle Regionen tabellarisch an
- `CloneRegion(sourceRegion, newName, newLocation, newPort)` - Klont eine Region
- `GetAllRegionFiles(regionsPath)` - Liste aller Region-Dateien
- `DeleteRegionFile(filePath, backup)` - Region-Datei löschen

#### Validation

- `IsValidIP(ip)` - IP-Adresse validieren (inkl. SYSTEMIP)
- `IsValidPort(port)` - Port validieren (1-65535)
- `IsValidUUID(uuid)` - UUID validieren

#### Helper Methods

- `EnsureDirectory(path)` - Verzeichnis erstellen falls nicht vorhanden
- `GetIniFiles(directory)` - Liste aller .ini Dateien
- `ShowSuccess(message)` - Erfolgsmeldung anzeigen
- `ShowError(message)` - Fehlermeldung anzeigen
- `ShowWarning(message)` - Warnungsmeldung anzeigen

## Verwendung

### Kompilierung

```bash
# Im Verzeichnis addon-modules/30Configure/
dotnet build Configure.csproj

# Oder mit runprebuild.bat im Hauptverzeichnis
runprebuild.bat
```

### Interaktiver Modus

```bash
bin/Configure.exe
```

**Hauptmenü:**

1. Standalone Configuration
2. StandaloneHG Configuration
3. Robust Configuration
4. RobustHG Configuration
5. Robust + Regions Configuration
6. RobustHG + Regions Configuration
7. **Region Management** (Create/Edit/Delete/Clone/List)
8. Global Settings (IP, Database, etc.)
9. Backup Management

### Command-Line Modus

```bash
bin/Configure.exe standalone       # Standalone konfigurieren
bin/Configure.exe standalonehg     # StandaloneHG konfigurieren
bin/Configure.exe robust           # Robust konfigurieren
bin/Configure.exe robusthg         # RobustHG konfigurieren
bin/Configure.exe robust-regions   # Robust + Regionen
bin/Configure.exe robusthg-regions # RobustHG + Regionen
bin/Configure.exe region           # Region Management
bin/Configure.exe backup           # Backup Management
bin/Configure.exe help             # Hilfe anzeigen
```

## Central Region Management

Das integrierte Region-Management ermöglicht die zentrale Verwaltung aller Regionen:

### CRM Features

- **Create**: Neue Region erstellen mit automatischer Dateinamen-Generierung
- **Edit**: Bestehende Region bearbeiten (Name, Location, Port, etc.)
- **List**: Alle Regionen tabellarisch anzeigen
- **Delete**: Region löschen (mit Backup)
- **Clone**: Region kopieren mit neuen Werten

### Automatische Dateinamen-Generierung

Der Region-Name wird automatisch in einen Dateinamen konvertiert:

- "My Region" → `myregion.ini`
- "Welcome Area" → `welcomearea.ini`
- "Test-Region_01" → `testregion01.ini`

**Regeln:**

- Kleinbuchstaben
- Leerzeichen, Bindestriche und Unterstriche entfernt
- Nur alphanumerische Zeichen

### Region-Parameter

- **RegionName**: Anzeigename der Region
- **RegionUUID**: Eindeutige ID (automatisch generiert oder manuell)
- **Location**: Grid-Koordinaten (X,Y)
- **InternalPort**: Simulator-Port
- **ExternalHostName**: Externe Domain/IP
- **SizeX/SizeY**: Regionsgröße in Metern (default: 256)
- **MaxPrims**: Maximale Anzahl Primitives (default: 45000)
- **MaxAgents**: Maximale Anzahl Avatare (default: 100)

### Verwendung in Konfiguratoren

Alle spezifischen Konfiguratoren können die Region-Management-Funktionen nutzen:

```csharp
// Region erstellen
var region = ConfigureCol.CreateRegionInteractive(settings);
if (region != null)
{
    string fileName = ConfigureCol.GenerateRegionFileName(region.RegionName);
    string filePath = Path.Combine(regionsPath, fileName);
    ConfigureCol.CreateOrUpdateRegionFile(filePath, region, true);
}

// Region bearbeiten
var region = ConfigureCol.ReadRegionFromFile(regionFile);
region = ConfigureCol.EditRegionInteractive(region);
ConfigureCol.CreateOrUpdateRegionFile(regionFile, region, true);

// Regionen auflisten
ConfigureCol.ListAllRegions(settings.PathSettings.RegionsPath);

// Region klonen
var cloned = ConfigureCol.CloneRegion(sourceRegion, "New Name", "1001,1001", "9001");
```

## Konfiguration

### Configure.json

Die globalen Einstellungen werden in `Configure.json` gespeichert:

```json
{
  "GlobalSettings": {
    "BaseIP": "127.0.0.1",
    "ExternalHostName": "SYSTEMIP",
    "DatabaseType": "SQLite",
    "MySQLConnectionString": "Data Source=localhost;Database=opensim;User ID=opensim;Password=***;",
    "GridName": "My OpenSim Grid",
    "GridNick": "MyGrid"
  },
  "StandaloneSettings": {
    "HttpPort": "9000",
    "InternalPort": "8003"
  },
  "RobustSettings": {
    "PublicPort": "8002",
    "PrivatePort": "8003",
    "GridServerURI": "http://127.0.0.1:8002",
    "GatekeeperURI": "http://127.0.0.1:8002"
  },
  "RegionSettings": {
    "RegionName": "My Region",
    "RegionUUID": "00000000-0000-0000-0000-000000000000",
    "Location": "1000,1000",
    "InternalPort": "9000",
    "ExternalHostName": "SYSTEMIP",
    "MaxPrims": "45000",
    "MaxAgents": "100"
  },
  "PathSettings": {
    "BinPath": "bin",
    "ConfigIncludePath": "bin/config-include",
    "RegionsPath": "bin/Regions",
    "BackupPath": "backups"
  }
}
```

### Dateistruktur

Nach der Konfiguration werden folgende Dateien verwaltet:

```bash
bin/
├── OpenSim.ini              - Hauptkonfiguration
├── Robust.ini               - Robust Grid (ohne Hypergrid)
├── Robust.HG.ini            - Robust Grid (mit Hypergrid)
├── config-include/
│   ├── GridCommon.ini       - Grid-Einstellungen
│   ├── osslEnable.ini       - OSSL-Funktionen
│   └── storage/
│       ├── SQLiteStandalone.ini
│       └── MySQLStandalone.ini
└── Regions/
    ├── myregion.ini         - Region "My Region"
    ├── welcomearea.ini      - Region "Welcome Area"
    └── testregion01.ini     - Region "Test-Region_01"
```

## Automatische Backups

Vor jeder Änderung an Konfigurationsdateien wird automatisch ein Backup erstellt:

- **Zeitstempel-Format**: `yyyyMMdd_HHmmss`
- **Backup-Verzeichnis**: `backups/` (konfigurierbar)
- **Backup-Struktur**: `backups/DateiName_JJJJMMTT_HHMMSS.bak`
- **Kann deaktiviert werden**: `backup = false` Parameter

**Beispiele:**

```bash
backups/OpenSim.ini_20251115_143025.bak
backups/myregion.ini_20251115_143026.bak
backups/Robust.HG.ini_20251115_143027.bak
```

**Backup-Verwaltung:**

- Über Menüpunkt 9 (Backup Management)
- Liste aller Backups
- Backup wiederherstellen
- Alte Backups löschen

## Erweiterbarkeit

Das System ist modular aufgebaut und kann einfach erweitert werden:

### Neuen Konfigurator erstellen

1. **Neue Datei erstellen**: `ConfigureXYZ.cs`
2. **Namespace**: `Diva.Configure`
3. **Klasse**: `public static class ConfigureXYZ`
4. **Methode**: `public static void Configure(ConfigureSettings settings)`
5. **ConfigureCol nutzen**: Alle Basis-Funktionen sind verfügbar
6. **In Configure.cs registrieren**: Menüpunkt und Command-Line-Option hinzufügen

**Beispiel:**

```csharp
namespace Diva.Configure
{
    public static class ConfigureMeinModus
    {
        public static void Configure(ConfigureSettings settings)
        {
            // IP-Adresse setzen
            ConfigureCol.SetIniValue("bin/OpenSim.ini", "Network", 
                "http_listener_port", "9000", true);
            
            // Region erstellen
            var region = ConfigureCol.CreateRegionInteractive(settings);
            if (region != null)
            {
                string fileName = ConfigureCol.GenerateRegionFileName(region.RegionName);
                string path = Path.Combine(settings.PathSettings.RegionsPath, fileName);
                ConfigureCol.CreateOrUpdateRegionFile(path, region, true);
            }
            
            Console.WriteLine("[SUCCESS] Konfiguration abgeschlossen!");
        }
    }
}
```

### Vorteile der modularen Architektur

- **Keine Code-Duplizierung**: Alle Basis-Funktionen in ConfigureCol.cs
- **Konsistenz**: Gleiche Backup-Strategie, Fehlerbehandlung, Validierung
- **Wartbarkeit**: Änderungen an einer Stelle wirken sich überall aus
- **Wiederverwendbarkeit**: Region-Management, INI-Operationen, etc. überall nutzbar

## Entwicklungsstatus

### ✅ Fertiggestellt

- ✅ **ConfigureCol.cs** - Vollständige Utility-Bibliothek
  - Backup-Management
  - File/Line-Operationen
  - INI-Konfiguration (inkl. Comment/Uncomment)
  - JSON-Operationen
  - Region-Management (Create/Edit/Delete/Clone/List)
  - Validierung (IP/Port/UUID)
- ✅ **Configure.cs** - Hauptprogramm mit interaktivem Menü
  - Command-Line-Unterstützung
  - Region-Management-Menü integriert
  - Global Settings / Backup Management
- ✅ **ConfigureSDL.cs** - Standalone-Konfiguration
- ✅ **ConfigureSDLHG.cs** - StandaloneHG-Konfiguration
- ✅ **ConfigureRO.cs** - Robust Grid-Konfiguration
- ✅ **Configure.json** - Settings-Template
- ✅ **Configure.csproj** - .NET 8.0 Projekt

### 🚧 Placeholder (noch zu implementieren)

- 🚧 **ConfigureROHG.cs** - RobustHG (Hypergrid)
- 🚧 **ConfigureROR.cs** - Robust + DefaultRegion + FallbackRegion
- 🚧 **ConfigureROHGR.cs** - RobustHG + DefaultRegion + DefaultHGRegion + FallbackRegion

**Hinweis**: Die Placeholder-Dateien existieren bereits mit grundlegender Struktur und müssen nur noch mit spezifischer Logik gefüllt werden.

## Best Practices

### Backup-Strategie

- **Immer Backups erstellen**: Bei allen Datei-Änderungen `backup = true`
- **Backup-Verzeichnis prüfen**: Regelmäßig alte Backups aufräumen
- **Vor großen Änderungen**: Komplettes Verzeichnis-Backup mit `ConfigureCol.BackupDirectory()`

### INI-Konfiguration

- **Section/Key-Struktur prüfen**: Mit `SectionExists()` und `KeyExists()` validieren
- **Kommentare nutzen**: Mit `CommentIniKey()` / `UncommentIniKey()` Features aktivieren/deaktivieren
- **Validierung**: IP, Port, UUID immer mit entsprechenden `IsValid*()` Funktionen prüfen

### Region-Management

- **Eindeutige Namen**: Regionsnamen sollten eindeutig sein
- **Port-Konflikte vermeiden**: Bei mehreren Regionen unterschiedliche Ports verwenden
- **Grid-Koordinaten**: Location sollte nicht mit anderen Regionen kollidieren
- **UUID**: Immer automatisch generieren lassen, außer bei Migration

### Fehlerbehandlung

- **Return-Werte prüfen**: Alle ConfigureCol-Funktionen geben `bool` oder `null` zurück
- **Console-Output**: Nutze `ShowSuccess()`, `ShowError()`, `ShowWarning()` für konsistente Ausgaben
- **Try-Catch**: In eigenen Konfiguratoren um kritische Operationen

### Performance

- **Batch-Operationen**: `ReadLines()` → Änderungen → `WriteLines()` statt vieler einzelner `SetIniValue()`
- **Validierung vorher**: IP/Port/UUID vor Datei-Operationen validieren
- **Directory-Checks**: `EnsureDirectory()` vor `CreateOrUpdateRegionFile()`

## Troubleshooting

### Häufige Probleme

**Problem**: "Region file already exists"

- **Lösung**: Datei mit `DeleteRegionFile()` löschen oder überschreiben bestätigen

**Problem**: "Section not found in INI"

- **Lösung**: Section mit `InsertLine()` manuell erstellen oder Template verwenden

**Problem**: "Invalid UUID format"

- **Lösung**: UUID-Format prüfen mit `IsValidUUID()` oder automatisch generieren lassen

**Problem**: "Port already in use"

- **Lösung**: Port mit `IsValidPort()` prüfen und anderen Port wählen

**Problem**: Backup-Verzeichnis voll

- **Lösung**: Über Menü 9 (Backup Management) alte Backups löschen

## Technische Details

### Dependencies

- .NET 8.0 Runtime
- System.Text.Json (für JSON-Operationen)
- System.Text.RegularExpressions (für Regex-Suche)

### Dateiformate

- **INI**: UTF-8 encoding, Sections in `[Brackets]`, Key=Value Pairs
- **JSON**: Pretty-printed mit Einrückung
- **Backups**: Originalformat + Zeitstempel-Suffix `.bak`

### Namenskonventionen

- **Klassen**: PascalCase (ConfigureSDL, ConfigureCol)
- **Methoden**: PascalCase (CreateRegion, SetIniValue)
- **Parameter**: camelCase (filePath, createBackup)
- **Konstanten**: UPPER_CASE (nicht verwendet)

## License

Copyright (c) Diva Configure Team. All rights reserved.

---

**Version**: 1.0.0  
**Letzte Aktualisierung**: 15. November 2025  
**Kompatibilität**: OpenSimulator 0.9.3+, .NET 8.0
