# 🔧 Diva Distribution Configuration Tool für .NET 8

Ein modernes, benutzerfreundliches Konfigurationstool für die OpenSim Diva Distribution, das die Erstkonfiguration und Verwaltung vereinfacht.

## ✨ Features

### 🎯 Interaktive Konfiguration

- Benutzerfreundliche Eingabeaufforderungen mit Standardwerten
- Passwort-Maskierung für sichere Eingabe
- Eingabevalidierung (IP-Adressen, E-Mail, Ports)
- Automatische IP-Erkennung

### ⚙️ Automatische Konfiguration

- Verwendung gespeicherter Einstellungen aus JSON-Datei
- Keine manuelle Eingabe erforderlich
- Ideal für Skript-basierte Deployments

### 💾 Backup & Wiederherstellung

- Automatisches Backup vor Konfigurationsänderungen
- ZIP-komprimierte Backups mit Zeitstempel
- Einfache Wiederherstellung aus Backup-Archiven
- Metadaten-Tracking für jedes Backup

### ✅ Validierung

- Überprüfung aller Konfigurationsdateien
- Validierung von Netzwerkeinstellungen
- Erkennung fehlender oder ungültiger Konfigurationen
- Detaillierte Fehlerberichte

### 🎨 Moderne Benutzeroberfläche

- Farbcodierte Ausgaben (Grün=Erfolg, Gelb=Warnung, Rot=Fehler)
- Professionelles ASCII-Banner
- Übersichtliche Zusammenfassung am Ende
- Emoji-Unterstützung für bessere Lesbarkeit

## 🚀 Verwendung

### Interaktiver Modus (Empfohlen für Erstkonfiguration)

```bash
cd bin
dotnet Configure.dll
```

Das Tool führt Sie durch folgende Schritte:

1. **Welt-Grundeinstellungen**
   - Name Ihrer Welt
   - Externe IP-Adresse oder Domain
   - HTTP-Port

2. **Datenbank-Konfiguration**
   - **Datenbanktyp wählen:** MySQL oder SQLite
   - Bei **MySQL**:
     - Host (z.B. localhost)
     - Schema/Datenbankname
     - Benutzername
     - Passwort
   - Bei **SQLite**: Keine weiteren Eingaben erforderlich (verwendet OpenSim.db)

3. **Administrator-Account**
   - Vorname
   - Nachname
   - Passwort
   - E-Mail-Adresse

4. **E-Mail-Benachrichtigungen (Optional)**
   - Gmail-Account
   - Gmail-Passwort

5. **Region-Einstellungen**
   - Basis-Koordinaten (X, Y)
   - Regionsgröße (Standard: 256x256)

### Automatischer Modus

```bash
# Verwendet Einstellungen aus ConfigureSettings.json
dotnet Configure.dll --auto
dotnet Configure.dll -a
```

### Validierung

```bash
# Überprüft die aktuelle Konfiguration
dotnet Configure.dll --validate
dotnet Configure.dll -v
```

### Backup erstellen

```bash
dotnet Configure.dll --backup
dotnet Configure.dll -b
```

### Aus Backup wiederherstellen

```bash
dotnet Configure.dll --restore
dotnet Configure.dll -r
```

### Hilfe anzeigen

```bash
dotnet Configure.dll --help
dotnet Configure.dll -h
```

## ⚙️ Konfiguration

Die Konfiguration wird in `ConfigureSettings.json` gespeichert:

```json
{
  "WorldName": "My World",
  "DbType": "MySQL",
  "DbHost": "localhost",
  "DbSchema": "opensim",
  "DbUser": "opensim",
  "DbPassword": "secret",
  "AdminFirstName": "Wifi",
  "AdminLastName": "Admin",
  "AdminPassword": "secret",
  "AdminEmail": "admin@localhost",
  "IpAddress": "127.0.0.1",
  "HttpPort": 9000,
  "BaseLocationX": 1000,
  "BaseLocationY": 1000,
  "RegionSizeX": 256,
  "RegionSizeY": 256,
  "GmailAccount": "",
  "GmailPassword": "",
  "AutoBackup": true,
  "Debug": false
}
```

### Konfigurations-Parameter

| Parameter | Typ | Beschreibung | Standard |
|-----------|-----|--------------|----------|
| `WorldName` | string | Name Ihrer virtuellen Welt | "My World" |
| `DbType` | string | Datenbanktyp: "MySQL" oder "SQLite" | "MySQL" |
| `DbHost` | string | Datenbank-Server-Adresse (nur MySQL) | "localhost" |
| `DbSchema` | string | Datenbank-Schema/Name (nur MySQL) | "opensim" |
| `DbUser` | string | Datenbank-Benutzername (nur MySQL) | "opensim" |
| `DbPassword` | string | Datenbank-Passwort (nur MySQL) | "secret" |
| `AdminFirstName` | string | Vorname des Wifi-Administrators | "Wifi" |
| `AdminLastName` | string | Nachname des Wifi-Administrators | "Admin" |
| `AdminPassword` | string | Passwort des Administrators | "secret" |
| `AdminEmail` | string | E-Mail des Administrators | "admin@localhost" |
| `IpAddress` | string | Externe IP oder Domain | Auto-Detect |
| `HttpPort` | int | HTTP-Port für Web-Services | 9000 |
| `BaseLocationX` | int | X-Koordinate für erste Region | 1000 |
| `BaseLocationY` | int | Y-Koordinate für erste Region | 1000 |
| `RegionSizeX` | int | Breite der Regionen in Metern | 256 |
| `RegionSizeY` | int | Höhe der Regionen in Metern | 256 |
| `GmailAccount` | string | Gmail für E-Mail-Benachrichtigungen | "" |
| `GmailPassword` | string | Gmail-Passwort | "" |
| `AutoBackup` | bool | Automatisches Backup vor Änderungen | true |
| `Debug` | bool | Debug-Modus mit erweiterten Infos | false |

## 📁 Erstellte/Geänderte Dateien

Das Tool konfiguriert folgende Dateien automatisch:

### Hauptverzeichnis (/bin)

- **OpenSim.ini** - Hauptkonfiguration für OpenSim Server
- **Robust.ini** - Robust Standalone Grid Services
- **Robust.HG.ini** - Robust Hypergrid Configuration
- **Wifi.ini** - Web-Interface Konfiguration

### Konfigurationsverzeichnis (/bin/config-include)

- **DivaPreferences.ini** - Diva-spezifische Präferenzen
- **GridCommon.ini** - Gemeinsame Grid-Einstellungen
- **MyWorld.ini** - Ihre Welt-Konfiguration
- **StandaloneCommon.ini** - Standalone gemeinsame Einstellungen
- **StandaloneHypergrid.ini** - Standalone Hypergrid-Konfiguration

### Regions-Verzeichnis (/bin/Regions)

- **Regions.ini** - Region-Definitionen (früher RegionConfig.ini)

### 1. `Regions/Regions.ini`

Enthält die Konfiguration für alle Regionen:

- Region-Namen
- UUIDs (automatisch generiert)
- Koordinaten
- Größe
- Ports

### 2. `config-include/MyWorld.ini`

Hauptkonfiguration für Ihre Welt:

- Datenbankverbindung (MySQL oder SQLite)
- Netzwerkeinstellungen
- Standard-Region
- E-Mail-Konfiguration
- Home-Location

**Wichtig:** Bei erneuter Konfiguration wird automatisch ein Backup mit Zeitstempel erstellt.

## 💾 Backup-System

### Automatische Backups

Wenn `AutoBackup: true` gesetzt ist, erstellt das Tool automatisch ein Backup vor jeder Konfigurationsänderung.

### Backup-Inhalt

Backups umfassen folgende Dateien:

- `OpenSim.ini`
- `Robust.ini`
- `Robust.HG.ini`
- `Wifi.ini`
- `config-include/DivaPreferences.ini`
- `config-include/GridCommon.ini`
- `config-include/MyWorld.ini`
- `config-include/StandaloneCommon.ini`
- `config-include/StandaloneHypergrid.ini`
- `Regions/Regions.ini`
- `ConfigureSettings.json`

### Doppelte Backup-Strategie

Das Tool erstellt **zwei Arten von Backups**:

1. **Individuelles Backup** - Jede Datei wird vor Änderung mit `.bak_ZEITSTEMPEL` gesichert
2. **Vollständiges ZIP-Archiv** - Alle Konfigurationsdateien in einem komprimierten Archiv

### Backup-Verzeichnis

```bash
config-backups/
├── config_backup_20251106_143022.zip
├── config_backup_20251105_091545.zip
└── config_backup_20251104_180312.zip
```

Jedes Backup enthält eine `backup_info.json` mit Metadaten:

```json
{
  "Timestamp": "20251106_143022",
  "WorldName": "My World",
  "ItemCount": 6
}
```

## ✅ Validierung2

Das Validierungs-Tool überprüft:

- ✓ Existenz aller benötigten Konfigurationsdateien
- ✓ Gültigkeit der IP-Adresse oder Domain
- ✓ E-Mail-Format des Administrators
- ✓ Port-Bereich (1-65535)
- ✓ Nicht-leere Pflichtwerte

### Beispiel-Ausgabe

```bash
╔════════════════════════════════════╗
║   Validating Configuration         ║
╚════════════════════════════════════╝

✓ Configuration validation passed!
  World Name: My World
  IP/Domain: example.com
  HTTP Port: 9000
  Database: localhost/opensim
```

## 🔒 Sicherheit

### Passwort-Eingabe

Passwörter werden während der Eingabe maskiert (*****):

- Keine Klartext-Anzeige im Terminal
- Backspace-Unterstützung zum Korrigieren
- Sichere Speicherung in JSON-Konfiguration

### Datenbank-Credentials

Datenbank-Passwörter werden in der Verbindungszeichenfolge in `MyWorld.ini` gespeichert. Stellen Sie sicher, dass diese Datei angemessen geschützt ist.

## 🎯 Anwendungsfälle

### 1. Erstkonfiguration einer neuen Installation

```bash
cd bin
dotnet Configure.dll
# Folgen Sie den Anweisungen
```

### 2. Schnelle Rekonfiguration mit gespeicherten Einstellungen

```bash
# Einstellungen in ConfigureSettings.json anpassen, dann:
dotnet Configure.dll --auto
```

### 3. Vor-Update-Backup

```bash
dotnet Configure.dll --backup
```

### 4. Nach fehlgeschlagenem Update wiederherstellen

```bash
dotnet Configure.dll --restore
```

### 5. Konfiguration überprüfen

```bash
dotnet Configure.dll --validate
```

## 🌐 Netzwerk-Konfiguration

### IP-Adresse / Domain

Das Tool akzeptiert:

- IPv4-Adressen (z.B. `192.168.1.100`)
- Domain-Namen (z.B. `myworld.example.com`)
- Localhost (`127.0.0.1` für lokale Tests)

**Hinweis:** Für externe Zugriffe benötigen Sie eine öffentliche IP oder Domain!

### Port-Konfiguration

- **HTTP-Port (Standard: 9000):** Für Web-Interface (Wifi)
- **Region-Ports:** Automatisch vergeben ab 9000 + Region-Nummer

### Firewall-Hinweise

Öffnen Sie folgende Ports:

- TCP 9000 (HTTP/Wifi)
- UDP 9000-9010 (Regionen)

## 📊 Ausgabe-Beispiele

### Erfolgreiche Konfiguration

```bash
╔══════════════════════════════════════════════════════════════╗
║              Configuration Complete!                         ║
╚══════════════════════════════════════════════════════════════╝

🌍 Your world is: My World
🔗 Your loginuri is: http://192.168.1.100:9000
🌐 Your Wifi app is: http://192.168.1.100:9000/wifi

👤 Your admin account for Wifi is:
   Username: Wifi Admin
   Password: ********
   Email:    admin@localhost

📧 Your users get email notifications from example@gmail.com

💾 Database:
   Type:   MySQL
   Host:   localhost
   Schema: opensim
   User:   opensim

════════════════════════════════════════════════════════════════
```

## 🐛 Fehlerbehandlung

Das Tool behandelt folgende Fehler:

- ❌ Fehlende Template-Dateien (`.ini.example`)
- ❌ Ungültige Eingabewerte
- ❌ Dateisystem-Zugriffsfehler
- ❌ JSON-Parsing-Fehler
- ❌ Backup/Restore-Fehler

Im Debug-Modus (`Debug: true`) werden vollständige Stack Traces angezeigt.

## 📋 Checkliste für Erstkonfiguration

- [ ] Datenbank erstellt und Zugangsdaten bereit
- [ ] Externe IP-Adresse oder Domain bekannt
- [ ] Firewall-Ports geöffnet
- [ ] Gmail-Account für Benachrichtigungen (optional)
- [ ] `dotnet Configure.dll` ausgeführt
- [ ] Alle Werte eingegeben
- [ ] Validierung erfolgreich (`--validate`)
- [ ] Backup erstellt (`--backup`)

## 🔄 Migration von älteren Versionen

Wenn Sie von einer älteren Diva Distribution migrieren:

1. Erstellen Sie ein Backup Ihrer alten Konfiguration
2. Kopieren Sie Werte aus der alten `MyWorld.ini`
3. Führen Sie das Configuration Tool aus
4. Vergleichen Sie die neue Konfiguration mit der alten

## 💡 Tipps & Best Practices

1. **Immer Backups erstellen** vor größeren Änderungen
2. **Starke Passwörter** für Admin und Datenbank verwenden
3. **Validierung ausführen** nach manuellen Änderungen
4. **ConfigureSettings.json sichern** für schnelle Wiederherstellung
5. **E-Mail-Benachrichtigungen** für bessere Benutzererfahrung einrichten

## 📄 Lizenz

Teil der OpenSim Diva Distribution - siehe Hauptprojekt für Lizenzinformationen.

Testausgabe: 2024-06-10
Die Configure.cs konfiguriert jetzt alles automatisch basierend auf den Benutzereingaben:

✅ Was wird automatisch konfiguriert:

1. Benutzereingaben (Interactive Configuration):

✅ WorldName (Name deiner Welt)
✅ IP-Adresse/Domain (externes Hostname)
✅ HTTP Port (Standard: 9000)
✅ Datenbanktyp (MySQL/SQLite)
Bei MySQL: Host, Port, Database, User, Password
Bei SQLite: Automatisch, keine weitere Eingabe nötig
✅ Wifi Admin (Vorname, Nachname, Passwort, Email)
✅ Gmail-Konto (optional für Notifications)
✅ Region-Einstellungen:
Base Location X/Y
Region Size (einheitlich für X, Y, Z)
2. Automatische Konfiguration aller Dateien:
OpenSim.ini:

✅ BaseHostname = IP-Adresse
✅ BaseURL = http://${Const|BaseHostname}
✅ PublicPort = HTTP Port
✅ Include-Architecture = "config-include/DivaPreferences.ini" (Diva Distribution)
Robust.ini + Robust.HG.ini:

✅ ConnectionString (Datenbank)
✅ BaseHostname, BaseURL, PublicPort
✅ Alle IP-Adressen ersetzt
MyWorld.ini:

✅ ConnectionString
✅ [UserAgentService] mit StorageProvider + ConnectionString
✅ GridInfo (login, gridname, welcome, register URLs)
✅ HomeLocation
✅ SMTP-Einstellungen (wenn Gmail angegeben)
✅ Alle IP-Adressen und Ports
GridCommon.ini:

✅ Gatekeeper und HomeURI URLs
✅ IP-Adressen angepasst
StandaloneCommon.ini:

✅ ConnectionString
✅ IP-Adressen und Ports
Regions/RegionConfig.ini:

✅ Regionname = WorldName (statt "Default Region")
✅ Location (X, Y Koordinaten)
✅ SizeX, SizeY, SizeZ (einheitliche Größe)
✅ InternalPort automatisch berechnet (HttpPort + 10)
✅ ExternalHostName = IP-Adresse
✅ Automatische UUID-Generierung (RegionUUID, MaptileStaticUUID)
✅ Saubere Struktur mit allen optionalen Settings als Kommentare
3. Automatisches Backup-System:
✅ Erstellt ZIP-Archiv mit Timestamp
✅ Einzelne .bak-Dateien für alle konfigurierten Files
✅ Behält die letzten 10 Backups
✅ Löscht automatisch ältere Backups
4. Intelligente Features:
✅ Verwendet bestehende .ini Dateien wenn .example fehlen (Fallback-Logik für einige Dateien)
✅ Bewahrt bestehende Regions wenn RegionConfig.ini schon existiert
✅ Überprüft Datenbankverbindung bei MySQL
✅ Setzt Berechtigungen und Defaults

📋 Der komplette Ablauf:

Der User muss nur einmal die Werte eingeben, alles andere wird automatisch konfiguriert und aufeinander abgestimmt! 🎉
