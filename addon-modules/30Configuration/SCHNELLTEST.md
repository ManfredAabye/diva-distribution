# Schnelltest-Anleitung: Wifi, Profile & Gruppen

## Schnellstart

### 1. Build und führe Configure aus

```powershell
# Im opensimsource Verzeichnis
cd d:\OpenSim_Win_Builder_01112025\opensimsource

# Build Configure Tool
dotnet build addon-modules/30Configuration/Configure.csproj -c Release

# Wechsel zu bin Verzeichnis
cd bin

# Führe Configure aus
..\addon-modules\30Configuration\bin\Release\net8.0\Configure.exe

# Oder mit dotnet
# dotnet ..\addon-modules\30Configuration\bin\Release\net8.0\Configure.dll
```

### 2. Drücke Enter für Standardeinstellungen

Das Tool wird automatisch konfigurieren:

- ✅ Wifi.ini mit Datenbankverbindung
- ✅ StandaloneCommon.ini mit aktiviertem UserProfilesService
- ✅ Gruppen-Konfiguration
- ✅ Alle anderen notwendigen Dateien

### 3. Erwartete Ausgabe

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

### Überprüfe Wifi.ini

```powershell
cat bin/Wifi.ini | Select-String -Pattern "DatabaseService|StorageProvider|ConnectionString" | Select-Object -First 5
```

Erwartetes Ergebnis:

```bash
[DatabaseService]
    StorageProvider = "Diva.Data.SQLite.dll"
    ConnectionString = "Data Source=opensim.db;Version=3;UseUTF16Encoding=True"
```

### Überprüfe StandaloneCommon.ini

```powershell
cat bin/config-include/StandaloneCommon.ini | Select-String -Pattern "UserProfilesService" -Context 0,2
```

Erwartetes Ergebnis:

```bash
[UserProfilesService]
    Enabled = true
    LocalServiceModule = "OpenSim.Services.UserProfilesService.dll:UserProfilesService"
```

## Test mit OpenSim

### Starte OpenSim

```powershell
cd bin
dotnet OpenSim.dll
```

### Teste Wifi (im Browser)

```bash
http://127.0.0.1:9000/wifi/
```

**Erwartung**:

- Statistiken werden angezeigt
- Benutzer-Liste (falls vorhanden)
- Regionen-Liste
- Login funktioniert

### Teste Profile (Inworld)

1. Logge dich ein mit einem Viewer
2. Rechte Maustaste auf deinen Avatar → "Profil"
3. Bearbeite "About" oder andere Felder
4. Speichere
5. Logge aus und wieder ein
6. **Erwartung**: Änderungen sind gespeichert

### Teste Gruppen (Inworld)

1. Erstelle eine Gruppe
2. Setze Gruppen-Titel
3. **Erwartung**: Gruppen funktionieren UND Profile funktionieren weiterhin

## Fehlerbehebung

### Wifi zeigt keine Daten

```powershell
# Überprüfe Log
cat bin/OpenSim.log | Select-String -Pattern "Wifi|DatabaseService" | Select-Object -Last 20
```

### Profile funktionieren nicht

```powershell
# Überprüfe ob UserProfilesService aktiviert ist
cat bin/config-include/StandaloneCommon.ini | Select-String -Pattern "UserProfilesService" -Context 0,5
```

Stelle sicher:

- `Enabled = true`
- ConnectionString ist gesetzt
- StorageProvider ist gesetzt

### Gruppen funktionieren nicht

```powershell
# Überprüfe Groups Konfiguration
cat bin/config-include/StandaloneCommon.ini | Select-String -Pattern "\[Groups\]" -Context 0,8
```

Stelle sicher:

- `Enabled = true`
- `Module = "Groups Module V2"`
- `ServicesConnectorModule = "Groups Local Service Connector"`

## Manuelle Korrektur (falls Configure nicht funktioniert)

### Wifi.ini

Bearbeite `bin/Wifi.ini` und stelle sicher dass am Anfang steht:

```ini
[DatabaseService]
    StorageProvider = "Diva.Data.SQLite.dll"
    ConnectionString = "Data Source=opensim.db;Version=3;UseUTF16Encoding=True"
```

### StandaloneCommon.ini

Bearbeite `bin/config-include/StandaloneCommon.ini` und füge am Ende hinzu:

```ini
[UserProfilesService]
    Enabled = true
    LocalServiceModule = "OpenSim.Services.UserProfilesService.dll:UserProfilesService"
    StorageProvider = "OpenSim.Data.SQLite.dll"
    ConnectionString = "Data Source=opensim.db;Version=3;UseUTF16Encoding=True"
    UserAccountService = OpenSim.Services.UserAccountService.dll:UserAccountService
    AuthenticationServiceModule = "OpenSim.Services.AuthenticationService.dll:PasswordAuthenticationService"

[Groups]
    Enabled = true
    Module = "Groups Module V2"
    StorageProvider = "OpenSim.Data.SQLite.dll"
    ConnectionString = "Data Source=opensim.db;Version=3;UseUTF16Encoding=True"
    ServicesConnectorModule = "Groups Local Service Connector"
    MessagingEnabled = true
    MessagingModule = "Groups Messaging Module V2"
```

## Vollständiger Test-Durchlauf

```powershell
# 1. Build Configure
cd d:\OpenSim_Win_Builder_01112025\opensimsource
dotnet build addon-modules/30Configuration/Configure.csproj -c Release

# 2. Führe Configure aus
cd bin
..\addon-modules\30Configuration\bin\Release\net8.0\Configure.exe

# 3. Verifiziere Wifi.ini
Write-Host "`n=== Wifi.ini DatabaseService ===" -ForegroundColor Cyan
cat Wifi.ini | Select-String -Pattern "\[DatabaseService\]" -Context 0,3

# 4. Verifiziere StandaloneCommon.ini UserProfilesService
Write-Host "`n=== StandaloneCommon.ini UserProfilesService ===" -ForegroundColor Cyan
cat config-include/StandaloneCommon.ini | Select-String -Pattern "Configured by Configure Tool - ENABLED" -Context 3,8

# 5. Verifiziere Groups
Write-Host "`n=== StandaloneCommon.ini Groups ===" -ForegroundColor Cyan
cat config-include/StandaloneCommon.ini | Select-String -Pattern "Groups Module V2 Configuration" -Context 3,10

# 6. Starte OpenSim
Write-Host "`n=== Starting OpenSim ===" -ForegroundColor Green
dotnet OpenSim.dll
```

## Erfolg Kriterien

✅ **Wifi funktioniert**

- Webseite zeigt Statistiken
- Login funktioniert
- Benutzerverwaltung funktioniert

✅ **Profile funktionieren**

- Profil-Änderungen werden gespeichert
- Profil-Daten bleiben nach Logout/Login erhalten

✅ **Gruppen funktionieren**

- Gruppen können erstellt werden
- Gruppen-Titel wird gespeichert
- Gruppen-Nachrichten funktionieren

✅ **Profile UND Gruppen funktionieren gleichzeitig**

- Keine Konflikte zwischen den beiden Systemen
