# Wifi, Profile und Gruppen - Fehlerbehebung

## Problem Beschreibung

Die WifiPages zeigen keine Daten an, Profile funktionieren nicht inworld, und Gruppen funktionieren nur wenn Profile nicht konfiguriert sind.

## Ursachen

1. **Wifi.ini fehlende Datenbankkonfiguration**: Die `[DatabaseService]` Sektion in `bin/Wifi.ini` hatte keine korrekt konfigurierte ConnectionString
2. **UserProfilesService deaktiviert**: In `StandaloneCommon.ini` war die UserProfilesService standardmäßig auf `Enabled = false` gesetzt
3. **Doppelte Konfigurationssektionen**: Mehrere UserProfilesService Sektionen führten zu Konflikten
4. **Fehlende Service Dependencies**: Wifi benötigt explizite Service-Referenzen für Datenbankzugriff

## Lösung

Die folgenden Änderungen wurden im `Configure.cs` Tool vorgenommen:

### 1. ConfigureWifiIni() - Datenbankverbindung

**Datei**: `addon-modules/30Configuration/Configure.cs`

Die Methode wurde erweitert um:

- Automatische Konfiguration der `[DatabaseService]` Sektion
- Korrekte `StorageProvider` Auswahl (Diva.Data.SQLite.dll oder Diva.Data.MySQL.dll)
- ConnectionString basierend auf den Einstellungen

```csharp
// Neu hinzugefügt
string connString = GetConnectionString();
string storageProvider = _settings.DbType.Equals("SQLite", StringComparison.OrdinalIgnoreCase) 
    ? "Diva.Data.SQLite.dll" 
    : "Diva.Data.MySQL.dll";
```

### 2. ConfigureStandaloneCommon() - UserProfilesService aktivieren

**Datei**: `addon-modules/30Configuration/Configure.cs`

Die Methode wurde verbessert um:

- Die ursprüngliche (deaktivierte) UserProfilesService Sektion zu überspringen
- Eine neue, korrekt konfigurierte und **aktivierte** UserProfilesService Sektion hinzuzufügen
- Doppelte Konfigurationen zu vermeiden

```csharp
// Überspringt die alte UserProfilesService Sektion
bool inUserProfilesSection = false;

// Fügt neue, aktivierte Sektion hinzu
[UserProfilesService]
    Enabled = true  // <- WICHTIG: Aktiviert!
    LocalServiceModule = "OpenSim.Services.UserProfilesService.dll:UserProfilesService"
    StorageProvider = "OpenSim.Data.SQLite.dll"
    ConnectionString = "..."
```

## Anwendung der Fixes

### Schritt 1: Build das Configure Tool neu

```powershell
cd d:\OpenSim_Win_Builder_01112025\opensimsource
dotnet build addon-modules/30Configuration/Configure.csproj -c Release
```

### Schritt 2: Führe Configure aus

```powershell
cd bin
dotnet ..\addon-modules\30Configuration\bin\Release\net8.0\Configure.dll
```

oder:

```powershell
cd bin
..\addon-modules\30Configuration\bin\Release\net8.0\Configure.exe
```

### Schritt 3: Wähle die automatische Konfiguration

Drücke einfach Enter um die Standardeinstellungen zu verwenden, oder gib neue Werte ein.

### Schritt 4: Überprüfe die generierten Dateien

Nach der Konfiguration sollten folgende Dateien korrekt konfiguriert sein:

1. **bin/Wifi.ini**
   - `[DatabaseService]` mit ConnectionString
   - `StorageProvider = "Diva.Data.SQLite.dll"` (oder MySQL)

2. **bin/config-include/StandaloneCommon.ini**
   - `[UserProfilesService]` mit `Enabled = true`
   - `[Groups]` korrekt konfiguriert
   - `[WifiService]` mit allen Service Dependencies

## Manuelle Überprüfung (falls notwendig)

### Überprüfe bin/Wifi.ini

Die Datei sollte so aussehen:

```ini
[DatabaseService]
    StorageProvider = "Diva.Data.SQLite.dll"
    ConnectionString = "Data Source=opensim.db;Version=3;UseUTF16Encoding=True"

[WifiService]
    Enabled = true
    ServerPort = ${Const|PublicPort}
    GridName = "My World"
    LoginURL = "http://127.0.0.1:9000"
    WebAddress = "http://127.0.0.1:9000"
    # ... weitere Einstellungen
```

### Überprüfe bin/config-include/StandaloneCommon.ini

Am Ende der Datei sollten folgende Sektionen sein:

```ini
; ========================================
; User Profiles Service
; Configured by Configure Tool - ENABLED
; ========================================

[UserProfilesService]
    Enabled = true
    LocalServiceModule = "OpenSim.Services.UserProfilesService.dll:UserProfilesService"
    StorageProvider = "OpenSim.Data.SQLite.dll"
    ConnectionString = "Data Source=opensim.db;Version=3;UseUTF16Encoding=True"
    UserAccountService = OpenSim.Services.UserAccountService.dll:UserAccountService
    AuthenticationServiceModule = "OpenSim.Services.AuthenticationService.dll:PasswordAuthenticationService"

; ========================================
; Groups Module V2 Configuration
; Configured by Configure Tool
; ========================================

[Groups]
    Enabled = true
    Module = "Groups Module V2"
    StorageProvider = "OpenSim.Data.SQLite.dll"
    ConnectionString = "Data Source=opensim.db;Version=3;UseUTF16Encoding=True"
    ServicesConnectorModule = "Groups Local Service Connector"
    MessagingEnabled = true
    MessagingModule = "Groups Messaging Module V2"
```

## Testen

### 1. Starte OpenSim

```powershell
cd bin
dotnet OpenSim.dll
```

### 2. Teste Wifi

Öffne im Browser: `http://127.0.0.1:9000/wifi/`

Die Wifi-Seiten sollten jetzt:

- Benutzerstatistiken anzeigen
- Regionsinformationen anzeigen
- Korrekt funktionieren

### 3. Teste Profile Inworld

1. Logge dich mit einem Viewer ein
2. Öffne dein Profil (rechte Maustaste auf Avatar → Profil)
3. Ändere deine Profile-Informationen (z.B. "About", Interests)
4. Speichere die Änderungen
5. Logge aus und wieder ein
6. Überprüfe ob die Änderungen gespeichert wurden

### 4. Teste Gruppen

1. Erstelle eine neue Gruppe
2. Füge Mitglieder hinzu
3. Überprüfe Gruppennachrichten
4. Überprüfe ob Profile weiterhin funktionieren

## Bekannte Probleme und Lösungen

### Problem: "Gruppen funktionieren nicht mehr wenn Profile aktiviert sind"

**Ursache**: Konfligierende Datenbank-ConnectionStrings oder fehlende Service-Konnektoren

**Lösung**:

- Stelle sicher dass beide Sektionen (UserProfilesService und Groups) denselben ConnectionString verwenden
- Verwende `Groups Local Service Connector` für Standalone-Modus
- Verwende `Groups HG Service Connector` für Hypergrid-Modus

### Problem: "Wifi zeigt immer noch keine Daten"

**Mögliche Ursachen**:

1. Datenbank ist leer (keine Benutzer/Regionen)
2. Falsche Datenbankverbindung
3. Dienste sind nicht gestartet

**Lösung**:

1. Erstelle einen Test-Benutzer über die Konsole: `create user`
2. Überprüfe die OpenSim.log Datei auf Fehler
3. Stelle sicher dass die Datenbank korrekt initialisiert ist

### Problem: "Profile werden nicht gespeichert"

**Ursache**: UserProfilesService nicht korrekt initialisiert

**Lösung**:

1. Überprüfe dass `Enabled = true` in der UserProfilesService Sektion
2. Überprüfe die Log-Datei auf "UserProfilesService" Einträge
3. Stelle sicher dass die Datenbanktabellen erstellt wurden

## Weitere Hinweise

### Backup

Vor der Neukonfiguration erstellt das Tool automatisch Backups:

- Individuelle `.bak_YYYYMMDD_HHMMSS` Dateien
- Komprimiertes Archiv in `config-backups/`

### Wiederherstellung

Falls Probleme auftreten, kannst du die Original-Konfiguration wiederherstellen:

```powershell
cd bin
Copy-Item OpenSim.ini.bak_YYYYMMDD_HHMMSS OpenSim.ini
Copy-Item Wifi.ini.bak_YYYYMMDD_HHMMSS Wifi.ini
Copy-Item config-include\StandaloneCommon.ini.bak_YYYYMMDD_HHMMSS config-include\StandaloneCommon.ini
```

### Debug-Modus

Für detaillierte Informationen setze in `ConfigureSettings.json`:

```json
{
  "Debug": true,
  // ... andere Einstellungen
}
```

## Support

Bei weiteren Problemen:

1. Überprüfe die Log-Dateien in `bin/`
2. Suche nach ERROR oder WARN Einträgen
3. Überprüfe die Datenbank-ConnectionString Syntax
4. Stelle sicher dass alle .dll Dateien vorhanden sind

## Änderungshistorie

- **2025-11-13**: Initiale Fehlerbehebung
  - ConfigureWifiIni() erweitert für Datenbankverbindung
  - ConfigureStandaloneCommon() verbessert für UserProfilesService
  - Doppelte Konfigurationssektionen entfernt
