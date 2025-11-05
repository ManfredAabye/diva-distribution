# OpenSimulator Diva Distribution - Konfiguration abgeschlossen

## 📋 Quick Start Anleitung

### Voraussetzungen

- Windows 10/11
- [.NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) installiert

### 🚀 Start des Simulators

1. **Doppelklick auf `start-opensim.bat`** oder
2. **Über Terminal**: `dotnet OpenSim.dll`

### 🌐 Zugang zum System

#### Web-Interface (Wifi)

- **URL**: <http://127.0.0.1:9000/wifi>
- **Admin Login**: opensim opensim
- **Passwort**: opensim123

#### OpenSim Konsole

- **Region**: My World
- **Erstelle Benutzer über Konsole**: `create user opensim opensim opensim123 admin@opensim.local`

#### Viewer-Zugang

- **Grid URL**: <http://127.0.0.1:9000/>
- **Login**: opensim opensim
- **Passwort**: opensim123

### ⚙️ Konfiguration

#### Datenbankeinstellungen

- **Typ**: SQLite (lokale Datei)
- **Datei**: opensim.db (wird automatisch erstellt)

#### Netzwerk

- **IP**: 127.0.0.1 (localhost)
- **Port**: 9000
- **Region**: My World (256x256m)

### 📁 Wichtige Dateien

```bash
bin/
├── OpenSim.dll              # Hauptprogramm
├── start-opensim.bat        # Windows Startskript
├── opensim.db               # SQLite Datenbank (wird erstellt)
├── OpenSim.ini              # Hauptkonfiguration
├── Wifi.ini                 # Web-Interface Konfiguration
├── Regions/
│   └── Regions.ini          # Region-Konfiguration
└── config-include/
    ├── DivaPreferences.ini  # Diva-spezifische Einstellungen
    ├── MyWorld.ini          # Welt-Konfiguration
    └── DefaultUser.ini      # Standard-Benutzer Konfiguration
```

### 🔧 Weitere Konfiguration

#### Neue Benutzer erstellen

1. **Über Web**: <http://127.0.0.1:9000/wifi> (Register)
2. **Über Konsole**: `create user Vorname Nachname Passwort Email`

#### Backup/Migration

- Sichere die `opensim.db` Datei für Benutzerdaten
- Sichere `bin/Regions/` Ordner für Region-Konfigurationen

### 🆘 Troubleshooting

#### Simulator startet nicht

- Überprüfe ob .NET 8 Runtime installiert ist
- Prüfe ob Port 9000 frei ist
- Schaue in die Konsole nach Fehlermeldungen

#### Web-Interface nicht erreichbar

- Überprüfe ob OpenSim läuft
- Versuche <http://127.0.0.1:9000/wifi>
- Firewall/Antivirus könnte blockieren

#### Login funktioniert nicht

- Standard Login: `opensim opensim` / `opensim123`
- Erstelle neuen Benutzer über Konsole falls nötig

### 📖 Weitere Ressourcen

- **OpenSimulator Wiki**: <http://opensimulator.org/wiki/>
- **Diva Distribution**: <https://github.com/diva/diva-distribution>
- **OpenSim Discord**: <https://opensimulator.org/page.php?id=4>

---
**Konfiguration abgeschlossen am**: $(Get-Date)
**Version**: Diva Distribution .NET 8 Edition
