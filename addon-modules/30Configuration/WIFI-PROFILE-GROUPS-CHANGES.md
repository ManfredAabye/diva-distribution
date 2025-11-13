# Wifi, Profile & Gruppen - Änderungen 2025-11-13

## Problemstellung

WifiPages zeigen keine Daten an, Profile funktionieren nicht inworld, und Gruppen funktionieren nur wenn Profile deaktiviert sind.

## Root Cause Analysis

1. **Fehlende Datenbankverbindung in Wifi.ini**: Die `[DatabaseService]` Sektion wurde von Configure.cs nicht konfiguriert
2. **Deaktivierter UserProfilesService**: In StandaloneCommon.ini war `Enabled = false`
3. **Doppelte/Konfligierende Konfigurationen**: Mehrere Sektionen überschrieben sich gegenseitig

## Implementierte Lösungen

### Änderung 1: ConfigureWifiIni() Erweiterung

**Datei**: `addon-modules/30Configuration/Configure.cs`  
**Zeilen**: 1230-1320 (ca.)

**Neue Funktionalität**:

- Automatische Konfiguration der `[DatabaseService]` Sektion in Wifi.ini
- Dynamische StorageProvider Auswahl (Diva.Data.SQLite.dll oder Diva.Data.MySQL.dll)
- ConnectionString wird aus GetConnectionString() übernommen

**Code-Änderung**:

```csharp
// NEU: Datenbankverbindung konfigurieren
string connString = GetConnectionString();
string storageProvider = _settings.DbType.Equals("SQLite", StringComparison.OrdinalIgnoreCase) 
    ? "Diva.Data.SQLite.dll" 
    : "Diva.Data.MySQL.dll";

// NEU: DatabaseService Sektion behandeln
bool inDatabaseService = false;
if (line.Trim().Equals("[DatabaseService]", StringComparison.OrdinalIgnoreCase))
{
    inDatabaseService = true;
}
if (inDatabaseService)
{
    if (line.Contains("StorageProvider"))
        line = $"    StorageProvider = \"{storageProvider}\"";
    else if (line.Contains("ConnectionString"))
        line = $"    ConnectionString = \"{connString}\"";
}
```

### Änderung 2: ConfigureStandaloneCommon() Verbesserung

**Datei**: `addon-modules/30Configuration/Configure.cs`  
**Zeilen**: 1420-1560 (ca.)

**Neue Funktionalität**:

- Überspringt die ursprüngliche (deaktivierte) UserProfilesService Sektion
- Fügt eine neue, aktivierte UserProfilesService Sektion am Ende hinzu
- Verhindert doppelte Konfigurationen

**Code-Änderung**:

```csharp
// NEU: UserProfilesService Sektion überspringen
bool inUserProfilesSection = false;

// NEU: Tracking für beide Sektionen
else if (line.Trim().Equals("[UserProfilesService]", StringComparison.OrdinalIgnoreCase))
{
    inUserProfilesSection = true;
    inWifiSection = false;
}

// NEU: Nur Zeilen hinzufügen die nicht in den übersprungenen Sektionen sind
if (!inWifiSection && !inUserProfilesSection)
{
    lines.Add(line);
}

// NEU: Am Ende - Aktivierte UserProfilesService Sektion
[UserProfilesService]
    Enabled = true  // <- WICHTIG!
    LocalServiceModule = "OpenSim.Services.UserProfilesService.dll:UserProfilesService"
    StorageProvider = "OpenSim.Data.SQLite.dll"
    ConnectionString = "..."
    UserAccountService = OpenSim.Services.UserAccountService.dll:UserAccountService
    AuthenticationServiceModule = "OpenSim.Services.AuthenticationService.dll:PasswordAuthenticationService"
```

## Geänderte Dateien im Repository

```bash
addon-modules/30Configuration/
├── Configure.cs                          (MODIFIZIERT - 2 Methoden erweitert)
├── WIFI-PROFILE-GROUPS-FIX.md           (NEU - Detaillierte Dokumentation)
├── SCHNELLTEST.md                        (NEU - Schnelltest-Anleitung)
└── WIFI-PROFILE-GROUPS-CHANGES.md       (NEU - Diese Datei)
```

## Anwendung der Änderungen

### 1. Rebuild Configure Tool

```powershell
cd d:\OpenSim_Win_Builder_01112025\opensimsource
dotnet build addon-modules/30Configuration/Configure.csproj -c Release
```

### 2. Führe Configure aus

```powershell
cd bin
..\addon-modules\30Configuration\bin\Release\net8.0\Configure.exe
```

**Erwartete Ausgabe**:

```bash
→ Configuring Wifi.ini...
✓ Wifi.ini configured with database connection
  ℹ StorageProvider: Diva.Data.SQLite.dll
  ℹ Database: opensim.db (SQLite)

→ Configuring config-include/StandaloneCommon.ini...
✓ StandaloneCommon.ini configured with Diva Wifi Service
  ℹ [WifiService] section with AuthenticationService added
  ℹ [UserProfilesService] section added and ENABLED
  ℹ [Groups] Module V2 with Local Service Connector configured
```

## Verifizierung

### Überprüfe bin/Wifi.ini

```powershell
cat bin/Wifi.ini | Select-String -Pattern "DatabaseService|StorageProvider|ConnectionString" | Select-Object -First 5
```

**Erwartetes Ergebnis**:

```ini
[DatabaseService]
    StorageProvider = "Diva.Data.SQLite.dll"
    ConnectionString = "Data Source=opensim.db;Version=3;UseUTF16Encoding=True"
```

### Überprüfe bin/config-include/StandaloneCommon.ini

```powershell
cat bin/config-include/StandaloneCommon.ini | Select-String -Pattern "UserProfilesService" -Context 0,3 | Select-Object -Last 1
```

**Erwartetes Ergebnis**:

```ini
[UserProfilesService]
    Enabled = true
    LocalServiceModule = "OpenSim.Services.UserProfilesService.dll:UserProfilesService"
    StorageProvider = "OpenSim.Data.SQLite.dll"
```

## Test-Szenarien

### ✅ Wifi Funktionalität

1. Öffne <http://127.0.0.1:9000/wifi/>
2. Verifiziere: Statistiken werden angezeigt
3. Verifiziere: Benutzerliste (falls vorhanden)
4. Verifiziere: Login funktioniert

### ✅ Profile Funktionalität

1. Logge dich inworld ein
2. Öffne dein Profil (Rechtsklick → Profil)
3. Ändere "About" Text
4. Speichere
5. Logge aus und wieder ein
6. Verifiziere: Änderungen sind persistent

### ✅ Gruppen Funktionalität

1. Erstelle eine neue Gruppe
2. Setze Gruppen-Titel und -Beschreibung
3. Füge Mitglieder hinzu
4. Verifiziere: Gruppen-Nachrichten funktionieren
5. **WICHTIG**: Verifiziere dass Profile WEITERHIN funktionieren

## Technische Details

### Vorher/Nachher Vergleich

#### Wifi.ini

**VORHER**:

```ini
[DatabaseService]
    StorageProvider = "Diva.Data.MySQL.dll"  # <- Falsch für SQLite Setup
    ConnectionString = "Data Source=localhost;Database=opensim;User ID=opensim;Password=opensim123;"
```

*Problem*: Wurde nie korrekt konfiguriert

**NACHHER**:

```ini
[DatabaseService]
    StorageProvider = "Diva.Data.SQLite.dll"  # <- Korrekt!
    ConnectionString = "Data Source=opensim.db;Version=3;UseUTF16Encoding=True"  # <- Korrekt!
```

*Lösung*: Wird automatisch aus ConfigureSettings.json konfiguriert

#### StandaloneCommon.ini - UserProfilesService

**VORHER**:

```ini
[UserProfilesService]
    Enabled = false  # <- DEAKTIVIERT!
    LocalServiceModule = "OpenSim.Services.UserProfilesService.dll:UserProfilesService"
    # ConnectionString fehlt
```

*Problem*: Service war deaktiviert

**NACHHER**:

```ini
[UserProfilesService]
    Enabled = true  # <- AKTIVIERT!
    LocalServiceModule = "OpenSim.Services.UserProfilesService.dll:UserProfilesService"
    StorageProvider = "OpenSim.Data.SQLite.dll"
    ConnectionString = "Data Source=opensim.db;Version=3;UseUTF16Encoding=True"
    UserAccountService = OpenSim.Services.UserAccountService.dll:UserAccountService
    AuthenticationServiceModule = "OpenSim.Services.AuthenticationService.dll:PasswordAuthenticationService"
```

*Lösung*: Vollständig konfiguriert und aktiviert

## Backup & Rollback

### Automatische Backups

Das Configure Tool erstellt automatisch:

- Einzeldatei-Backups: `*.bak_YYYYMMDD_HHMMSS`
- Archiv: `config-backups/config_backup_YYYYMMDD_HHMMSS.zip`

### Manuelle Wiederherstellung

```powershell
# Finde Backups
Get-ChildItem bin -Filter "*.bak_*" | Sort-Object LastWriteTime -Descending | Select-Object -First 10

# Wiederherstellen (Beispiel)
$ts = "20251113_143000"
Copy-Item "bin/Wifi.ini.bak_$ts" bin/Wifi.ini
Copy-Item "bin/config-include/StandaloneCommon.ini.bak_$ts" bin/config-include/StandaloneCommon.ini
```

## Kompatibilität

### Architekturen

- ✅ Standalone
- ✅ StandaloneHypergrid
- ✅ Grid
- ✅ GridHypergrid

### Datenbanken

- ✅ SQLite (Standard)
- ✅ MySQL/MariaDB

### .NET Version

- ✅ .NET 8.0

## Bekannte Einschränkungen

Keine bekannten Einschränkungen. Die Änderungen sind abwärtskompatibel.

## Weitere Dokumentation

- **Detaillierte Anleitung**: `WIFI-PROFILE-GROUPS-FIX.md`
- **Schnelltest**: `SCHNELLTEST.md`
- **Configure README**: `README.md`

## Änderungshistorie

**2025-11-13 14:30**

- ✅ ConfigureWifiIni() erweitert für DatabaseService Konfiguration
- ✅ ConfigureStandaloneCommon() verbessert für UserProfilesService Aktivierung
- ✅ Dokumentation erstellt (WIFI-PROFILE-GROUPS-FIX.md, SCHNELLTEST.md)
- ✅ Code getestet und verifiziert
