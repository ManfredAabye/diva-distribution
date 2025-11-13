# OpenSim Configuration Template System

## Übersicht

Das neue Template-System generiert alle OpenSim-Konfigurationsdateien **ohne Kommentare** aus JSON-Definitionen. Alle Einstellungen werden zentral über das `Configure`-Tool in `addon-modules/30Configuration` verwaltet.

## Architektur-Modi

Das System unterstützt 4 verschiedene Architektur-Modi:

### 1. Standalone

- **Beschreibung**: Einzelner lokaler Server ohne Hypergrid
- **Regionen**: 1 Region (DefaultRegion)
- **Konfigurationsdateien**:
  - OpenSim.ini
  - Standalone.ini
  - StandaloneCommon.ini
  - SQLiteStandalone.ini
  - osslEnable.ini
  - Regions/DefaultRegion.ini

### 2. StandaloneHypergrid (Diva Standard)

- **Beschreibung**: Einzelner Server mit Hypergrid-Unterstützung
- **Regionen**: 1 Region (DefaultRegion)
- **Konfigurationsdateien**:
  - OpenSim.ini
  - Standalone.ini
  - StandaloneCommon.ini
  - StandaloneHypergrid.ini
  - SQLiteStandalone.ini
  - osslEnable.ini
  - Wifi.ini
  - Regions/DefaultRegion.ini

### 3. Grid (Robust)

- **Beschreibung**: Multi-Region Grid mit Robust-Server
- **Regionen**: 2 Regionen (DefaultRegion, FallbackRegion)
- **Konfigurationsdateien**:
  - OpenSim.ini
  - Robust.ini
  - GridCommon.ini
  - osslEnable.ini
  - Regions/DefaultRegion.ini
  - Regions/FallbackRegion.ini

### 4. GridHypergrid (RobustHG)

- **Beschreibung**: Multi-Region Grid mit Robust-Server und Hypergrid
- **Regionen**: 3 Regionen (DefaultRegion, DefaultHGRegion, FallbackRegion)
- **Konfigurationsdateien**:
  - OpenSim.ini
  - Robust.HG.ini
  - GridCommon.ini
  - osslEnable.ini
  - Wifi.ini
  - Regions/DefaultRegion.ini
  - Regions/DefaultHGRegion.ini
  - Regions/FallbackRegion.ini

## JSON Template-Struktur

### ConfigurationTemplates.json

Definiert die 4 Architektur-Modi mit ihren zugehörigen Dateien und Eigenschaften:

```json
{
  "Architectures": {
    "Standalone": {
      "Description": "...",
      "RequiredFiles": [...],
      "RequiresRobust": false,
      "RequiresHypergrid": false,
      "RegionCount": 1
    }
  }
}
```

### Einzelne Template-Dateien

Jede Konfigurationsdatei hat ein entsprechendes JSON-Template:

- **OpenSimIniTemplate.json** - OpenSim.ini Hauptkonfiguration
- **RobustIniTemplate.json** - Robust.ini / Robust.HG.ini Server-Konfiguration
- **StandaloneCommonIniTemplate.json** - StandaloneCommon.ini Services
- **StandaloneHypergridIniTemplate.json** - StandaloneHypergrid.ini HG-Services
- **SQLiteStandaloneIniTemplate.json** - SQLiteStandalone.ini Datenbank
- **WifiIniTemplate.json** - Wifi.ini Web-Interface
- **osslEnableIniTemplate.json** - osslEnable.ini Script-Permissions
- **RegionConfigTemplate.json** - Regions/*.ini Region-Definitionen

## Variablen-Substitution

Templates verwenden `${VariableName}` für dynamische Werte:

### Netzwerk-Variablen

- `${IpAddress}` - Server IP-Adresse
- `${HttpPort}` - HTTP Port (Standard: 9000)
- `${PublicPort}` - Öffentlicher Port

### Grid-Variablen

- `${GridName}` - Name des Grids
- `${WorldName}` - Name der Hauptregion
- `${WelcomeMessage}` - Login-Willkommensnachricht

### Region-Variablen

- `${BaseLocationX}` - X-Koordinate (z.B. 1000)
- `${BaseLocationY}` - Y-Koordinate (z.B. 1000)
- `${RegionSizeX}` - Regiongröße X (Standard: 512)
- `${RegionSizeY}` - Regiongröße Y (Standard: 512)
- `${RegionSizeZ}` - Regiongröße Z (Standard: 4096)

### Admin-Variablen

- `${AdminFirst}` - Vorname des Admins
- `${AdminLast}` - Nachname des Admins
- `${AdminEmail}` - Email des Admins

### Datenbank-Variablen (SQLite)

- `${AssetDll}` - OpenSim.Data.SQLite.dll
- `${AssetConnectionString}` - URI=file:Asset.db,version=3
- `${InventoryDll}` - OpenSim.Data.SQLite.dll
- `${InventoryConnectionString}` - URI=file:inventory.db,version=3
- `${GridDll}` - OpenSim.Data.SQLite.dll
- `${GridConnectionString}` - URI=file:regions.db,version=3
- `${PresenceDll}` - OpenSim.Data.SQLite.dll
- `${PresenceConnectionString}` - URI=file:presence.db,version=3
- `${AuthenticationDll}` - OpenSim.Data.SQLite.dll
- `${AuthenticationConnectionString}` - URI=file:auth.db,version=3
- `${UserAccountDll}` - OpenSim.Data.SQLite.dll
- `${UserAccountConnectionString}` - URI=file:userprofiles.db,version=3
- `${GridUserDll}` - OpenSim.Data.SQLite.dll
- `${GridUserConnectionString}` - URI=file:griduser.db,version=3
- `${AvatarDll}` - OpenSim.Data.SQLite.dll
- `${AvatarConnectionString}` - URI=file:avatars.db,version=3
- `${FriendsDll}` - OpenSim.Data.SQLite.dll
- `${FriendsConnectionString}` - URI=file:friends.db,version=3
- `${EstateDll}` - OpenSim.Data.SQLite.dll
- `${EstateConnectionString}` - URI=file:estate.db,version=3
- `${GroupsDll}` - OpenSim.Data.SQLite.dll
- `${GroupsConnectionString}` - URI=file:opensim.db,version=3
- `${ProfilesDll}` - OpenSim.Data.SQLite.dll
- `${ProfilesConnectionString}` - URI=file:userprofiles.db,version=3
- `${MuteListDll}` - OpenSim.Data.SQLite.dll
- `${MuteListConnectionString}` - URI=file:mutelist.db,version=3
- `${IMDll}` - OpenSim.Data.SQLite.dll
- `${IMConnectionString}` - URI=file:im.db,version=3
- `${WifiDll}` - Diva.Data.SQLite.dll
- `${WifiConnectionString}` - Data Source=WifiData.db;Version=3;

### Sicherheits-Variablen

- `${ForgotPasswordHashSecret}` - Geheimer Hash für Passwort-Reset

### Hypergrid-Variablen

- `${AllowHypergridMapSearch}` - true/false

## Template-Format

Jedes Template verwendet folgende JSON-Struktur:

```json
{
  "TemplateNameTemplate": {
    "Description": "Beschreibung des Templates",
    "Sections": {
      "SectionName": {
        "KeyName": "value",
        "KeyWithVariable": "${VariableName}"
      }
    }
  }
}
```

## Generierte .ini-Dateien Format

Die generierten .ini-Dateien haben **keine Kommentare** und folgen diesem Format:

```ini
[SectionName]
KeyName = value
KeyWithVariable = substituted_value

[NextSection]
...
```

## Verwendung

### 1. Configure-Tool ausführen

```powershell
cd D:\OpenSim_Win_Builder_01112025\opensimsource\bin
.\Configure.exe
```

### 2. Architektur auswählen

Wähle einen der 4 Modi:

1. Standalone
2. StandaloneHypergrid (empfohlen für Diva)
3. Grid (Robust)
4. GridHypergrid (RobustHG)

### 3. Konfiguration eingeben

Das Tool fragt nach:

- IP-Adresse
- HTTP Port
- Grid-Name
- Region-Name
- Admin-Informationen
- Region-Größe und -Position
- etc.

### 4. Konfigurationsdateien werden generiert

Alle .ini-Dateien werden automatisch erstellt:

- `bin/OpenSim.ini`
- `bin/Robust.ini` oder `bin/Robust.HG.ini` (wenn Grid)
- `bin/config-include/StandaloneCommon.ini` (wenn Standalone)
- `bin/config-include/StandaloneHypergrid.ini` (wenn StandaloneHypergrid)
- `bin/config-include/storage/SQLiteStandalone.ini`
- `bin/config-include/osslEnable.ini`
- `bin/Wifi.ini` (wenn Hypergrid)
- `bin/Regions/DefaultRegion.ini`
- `bin/Regions/FallbackRegion.ini` (wenn Grid)
- `bin/Regions/DefaultHGRegion.ini` (wenn GridHypergrid)

## Region-Konfiguration

### DefaultRegion

- **Name**: Vom Benutzer gewählter Welt-Name
- **Location**: `${BaseLocationX}, ${BaseLocationY}`
- **InternalPort**: 9000
- **Flags**: DefaultRegion, FallbackRegion

### FallbackRegion (nur Grid/GridHypergrid)

- **Name**: FallbackRegion
- **Location**: `${BaseLocationX+1}, ${BaseLocationY}`
- **InternalPort**: 9001
- **Flags**: FallbackRegion

### DefaultHGRegion (nur GridHypergrid)

- **Name**: DefaultHGRegion
- **Location**: `${BaseLocationX}, ${BaseLocationY+1}`
- **InternalPort**: 9002
- **Flags**: DefaultHGRegion

## Datenbank-Konfiguration

### SQLite (Standard)

Alle Services verwenden separate SQLite-Datenbanken:

- **Assets**: `Asset.db`
- **Inventory**: `inventory.db`
- **Avatars**: `avatars.db`
- **Authentication**: `auth.db`
- **UserAccounts**: `userprofiles.db`
- **GridUser**: `griduser.db`
- **Presence**: `presence.db`
- **Friends**: `friends.db`
- **Estate**: `estate.db`
- **Grid**: `regions.db`
- **Groups**: `opensim.db`
- **Profiles**: `userprofiles.db`
- **MuteList**: `mutelist.db`
- **IM**: `im.db`
- **Wifi**: `WifiData.db`

### MySQL/MariaDB (Optional)

Kann durch Änderung der Variablen konfiguriert werden:

```bash
${AssetDll} = "OpenSim.Data.MySQL.dll"
${AssetConnectionString} = "Data Source=localhost;Database=opensim;User ID=opensim;Password=***;"
```

## Service-Konfiguration

### Standalone-Services (StandaloneCommon.ini)

Alle Services laufen lokal:

- AssetService
- InventoryService
- GridService
- PresenceService
- AuthenticationService
- UserAccountService
- GridUserService
- AvatarService
- FriendsService
- EstateService
- LibraryService
- LoginService
- MapImageService
- Groups (V2)
- UserProfiles
- BakedTexture
- MuteList

### Hypergrid-Services (StandaloneHypergrid.ini)

Zusätzliche HG-Services:

- HGEntityTransferModule
- HGInventoryAccessModule
- HGAssetService
- HGFriendsModule
- HGInventoryService
- UserAgentService
- GatekeeperService

### Robust-Services (Robust.ini / Robust.HG.ini)

Zentrale Grid-Services:

- DatabaseService
- AssetService
- InventoryService
- GridService
- AuthenticationService
- UserAccountService
- GridUserService
- PresenceService
- AvatarService
- FriendsService
- LibraryService
- LoginService
- MapImageService
- GridInfoService

Mit HG zusätzlich:

- GatekeeperService
- UserAgentService
- HGInventoryService
- HGAssetService
- HGFriendsService
- Messaging
- Groups

## OSSL-Permissions (osslEnable.ini)

Script-Funktionen sind konfiguriert mit Permissions:

- `true` - Alle können verwenden
- `PARCEL_OWNER` - Nur Parcel-Besitzer
- `ESTATE_MANAGER` - Estate Manager
- `ESTATE_OWNER` - Estate Besitzer
- `GRID_GOD` - Grid Gods

Beispiele:

- `Allow_osTerrainFlush = "ESTATE_MANAGER,ESTATE_OWNER"`
- `Allow_osSetParcelDetails = "ESTATE_MANAGER,ESTATE_OWNER"`
- `Allow_osTeleportAgent = "ESTATE_MANAGER,ESTATE_OWNER"`
- `Allow_osGetAgents = "true"`

## Wifi-Konfiguration (Wifi.ini)

Diva Wifi Web-Interface:

- **DatabaseService**: Eigene SQLite-Datenbank (WifiData.db)
- **Service-Referenzen**: Alle OpenSim-Services verlinkt
- **URLs**: LoginURL, WebAddress, HomeURI alle auf `http://${IpAddress}:${HttpPort}`
- **Admin**: AdminFirst, AdminLast, AdminEmail
- **Features**: Account-Management, User-Suche, Hypergrid-Integration

## Wichtige Funktionen

### Profile und Gruppen

**Beide funktionieren gleichzeitig** durch korrekte Konfiguration:

#### UserProfilesService (StandaloneCommon.ini)

```ini
[UserProfilesService]
LocalServiceModule = OpenSim.Services.UserProfilesService.dll:UserProfilesService
Enabled = true
UserAccountService = OpenSim.Services.UserAccountService.dll:UserAccountService
AuthenticationServiceModule = OpenSim.Services.AuthenticationService.dll:PasswordAuthenticationService
StorageProvider = OpenSim.Data.SQLite.dll
ConnectionString = URI=file:userprofiles.db,version=3
```

#### Groups (StandaloneCommon.ini)

```ini
[Groups]
Enabled = true
Module = Groups Module V2
StorageProvider = OpenSim.Data.SQLite.dll
ConnectionString = URI=file:opensim.db,version=3
ServicesConnectorModule = Groups Local Service Connector
LocalService = local
NoticesEnabled = true
MessageOnlineUsersOnly = true
```

### Wifi-Datenbank-Anbindung

**Wifi.ini DatabaseService-Sektion** ist erforderlich:

```ini
[DatabaseService]
StorageProvider = Diva.Data.SQLite.dll
ConnectionString = Data Source=WifiData.db;Version=3;
```

**WifiService-Sektion** verlinkt alle Services:

```ini
[WifiService]
GridService = OpenSim.Services.GridService.dll:GridService
UserAccountService = OpenSim.Services.UserAccountService.dll:UserAccountService
AuthenticationService = OpenSim.Services.AuthenticationService.dll:PasswordAuthenticationService
PresenceService = OpenSim.Services.PresenceService.dll:PresenceService
UserProfilesService = OpenSim.Services.UserProfilesService.dll:UserProfilesService
InventoryService = OpenSim.Services.InventoryService.dll:XInventoryService
AvatarService = OpenSim.Services.AvatarService.dll:AvatarService
FriendsService = OpenSim.Services.FriendsService.dll:FriendsService
GridUserService = OpenSim.Services.UserAccountService.dll:GridUserService
```

## Dateipfade

### Template-Dateien

```bash
addon-modules/30Configuration/Templates/
├── ConfigurationTemplates.json
├── OpenSimIniTemplate.json
├── RobustIniTemplate.json
├── StandaloneCommonIniTemplate.json
├── StandaloneHypergridIniTemplate.json
├── SQLiteStandaloneIniTemplate.json
├── WifiIniTemplate.json
├── osslEnableIniTemplate.json
└── RegionConfigTemplate.json
```

### Generierte Konfigurationsdateien

```bash
bin/
├── OpenSim.ini
├── Robust.ini (oder Robust.HG.ini)
├── Wifi.ini
├── config-include/
│   ├── StandaloneCommon.ini
│   ├── StandaloneHypergrid.ini
│   ├── GridCommon.ini
│   ├── osslEnable.ini
│   └── storage/
│       └── SQLiteStandalone.ini
└── Regions/
    ├── DefaultRegion.ini
    ├── FallbackRegion.ini
    └── DefaultHGRegion.ini
```

## Vorteile des neuen Systems

✅ **Keine Kommentare** - Saubere, kompakte Konfigurationsdateien
✅ **Zentrale Verwaltung** - Alle Einstellungen über Configure-Tool
✅ **Variablen-Substitution** - Dynamische Werte automatisch ersetzt
✅ **Architektur-basiert** - Passende Dateien für jeden Modus
✅ **Konsistenz** - Keine doppelten oder widersprüchlichen Einträge
✅ **Wartbarkeit** - JSON-Templates leicht zu pflegen
✅ **Erweiterbar** - Neue Architekturen einfach hinzufügbar

## Nächste Schritte

1. ✅ Alle JSON-Templates erstellt
2. ⏳ Template-Processing-Engine in Configure.cs implementieren
3. ⏳ Variable-Substitution-System implementieren
4. ⏳ Region-Config-Generator implementieren
5. ⏳ Architektur-Auswahl-Logik implementieren
6. ⏳ Testen aller 4 Architektur-Modi
7. ⏳ MySQL/MariaDB-Unterstützung hinzufügen

## Support

Bei Problemen:

1. Prüfe die generierten .ini-Dateien auf Fehler
2. Prüfe OpenSim.log für Fehlermeldungen
3. Stelle sicher, dass alle benötigten DLLs vorhanden sind
4. Prüfe Datenbank-Verbindungen
5. Siehe WIFI-PROFILE-GROUPS-FIX.md für spezifische Fixes
