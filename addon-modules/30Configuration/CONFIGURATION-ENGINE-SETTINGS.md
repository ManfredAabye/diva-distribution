# OpenSim Engine-Konfiguration (BulletSim, YEngine, OSSL)

## Übersicht

Die JSON-Konfigurationsdateien in diesem Verzeichnis wurden erweitert, um folgende Einstellungen automatisch zu konfigurieren:

1. **Physics Engine**: BulletSim (aktiviert), ubODE (deaktiviert)
2. **Script Engine**: YEngine (aktiviert), XEngine (deaktiviert)
3. **OSSL-Funktionen**: Vollständig aktiviert mit ThreatLevel High
4. **HTTP-Filter**: Localhost-Zugriff für llHttpRequest freigegeben

## Verfügbare Konfigurationsdateien

### 1. StandaloneSettings.json

**Verwendung**: Einzelne OpenSim-Instanz ohne Hypergrid

- Port: 9000
- HTTP-Filter-Ausnahme: `127.0.0.1:9000`
- Keine spezifischen Region-Flags

### 2. StandaloneHypergridSettings.json

**Verwendung**: Einzelne OpenSim-Instanz mit Hypergrid-Konnektivität

- Port: 9000
- HTTP-Filter-Ausnahme: `127.0.0.1:9000`
- Hypergrid aktiviert mit HGWorldMap
- Keine spezifischen Region-Flags

### 3. GridSettings.json

**Verwendung**: Multi-Instanz Grid (Robust) ohne Hypergrid

- Port: 8002 (Robust)
- HTTP-Filter-Ausnahme: `127.0.0.1:8002`
- Region-Flags: `DefaultRegion, FallbackRegion`

### 4. GridHypergridSettings.json

**Verwendung**: Multi-Instanz Grid (Robust) mit Hypergrid

- Port: 8002 (Robust)
- HTTP-Filter-Ausnahme: `127.0.0.1:8002`
- Region-Flags: `DefaultRegion, DefaultHGRegion, FallbackRegion`
- Hypergrid aktiviert mit HGWorldMap

## Konfigurierte Einstellungen

### Physics Engine (alle Profile)

```json
"PhysicsEngine": {
  "Engine": "BulletSim",
  "Meshing": "Meshmerizer",
  "DisableUbODE": true
}
```

**Ergebnis in OpenSim.ini:**

```ini
[Startup]
    meshing = Meshmerizer
    physics = BulletSim
    ; physics = ubODE  (auskommentiert/deaktiviert)
```

### Script Engine (alle Profile)

```json
"ScriptEngine": {
  "DefaultEngine": "YEngine",
  "YEngineEnabled": true,
  "XEngineEnabled": false,
  "MinTimerInterval": 0.05,
  "ScriptDistanceLimitFactor": 20,
  "DeleteScriptsOnStartup": false
}
```

**Ergebnis in OpenSim.ini:**

```ini
[Startup]
    DefaultScriptEngine = "YEngine"

[YEngine]
    Enabled = true
    MinTimerInterval = 0.05
    ScriptDistanceLimitFactor = 20
    DeleteScriptsOnStartup = false

; XEngine wird nicht konfiguriert oder deaktiviert
```

### OSSL-Funktionen (alle Profile)

```json
"OSSL": {
  "Enabled": true,
  "AllowOSFunctions": true,
  "AllowMODFunctions": true,
  "AllowLightShareFunctions": true,
  "OSFunctionThreatLevel": "High",
  "PermissionErrorToOwner": false
}
```

**Ergebnis in OpenSim.ini oder osslEnable.ini:**

```ini
[OSSL]
    AllowOSFunctions = true
    AllowMODFunctions = true
    AllowLightShareFunctions = true
    OSFunctionThreatLevel = High
    PermissionErrorToOwner = false
```

**WICHTIG**: Alle OSSL-Funktionsberechtigungen aus `DivaPreferences.ini` müssen ebenfalls in die finale Konfiguration übernommen werden.

### HTTP-Filter (alle Profile)

```json
"Network": {
  "OutboundDisallowForUserScriptsExcept": "127.0.0.1:9000",
  "HttpBodyMaxLenMAX": 16384,
  "ExternalHostNameForLSL": "127.0.0.1"
}
```

**Ergebnis in OpenSim.ini:**

```ini
[Network]
    ; Erlaubt llHttpRequest zu localhost
    OutboundDisallowForUserScriptsExcept = 127.0.0.1:9000
    HttpBodyMaxLenMAX = 16384
    ExternalHostNameForLSL = 127.0.0.1
```

## Region-Flags

### Grid (Robust ohne HG)

```json
"GridService": {
  "DefaultRegionFlags": "DefaultRegion, FallbackRegion"
}
```

**Ergebnis in GridService:**

- Die erste Region wird als `DefaultRegion` und `FallbackRegion` markiert

### GridHypergrid (Robust mit HG)

```json
"GridService": {
  "DefaultRegionFlags": "DefaultRegion, DefaultHGRegion, FallbackRegion"
}
```

**Ergebnis in GridService:**

- Die erste Region wird als `DefaultRegion`, `DefaultHGRegion` und `FallbackRegion` markiert

## Verwendung

### 1. JSON-Datei auswählen

Wählen Sie die passende JSON-Datei für Ihre Architektur:

- `StandaloneSettings.json` - Einzelinstanz ohne HG
- `StandaloneHypergridSettings.json` - Einzelinstanz mit HG
- `GridSettings.json` - Grid/Robust ohne HG
- `GridHypergridSettings.json` - Grid/Robust mit HG

### 2. JSON-Datei anpassen

Bearbeiten Sie die Grundeinstellungen nach Bedarf:

- `WorldName`
- `IpAddress`
- `HttpPort`
- `AdminFirstName`, `AdminLastName`, `AdminPassword`
- Datenbankeinstellungen

### 3. Konfiguration anwenden

Das 30Configuration-Modul liest die JSON-Dateien und schreibt automatisch:

- `OpenSim.ini`
- `config-include/*.ini` Dateien
- `Regions/RegionConfig.ini`

## Fehlerbehebung

### Problem: llHttpRequest zu localhost blockiert

**Symptom**: `[03:20] llHttpRequest: Request to http://127.0.0.1:9000 disallowed by filter`

**Lösung**: Überprüfen Sie, dass in `OpenSim.ini` folgende Zeile vorhanden ist:

```ini
OutboundDisallowForUserScriptsExcept = 127.0.0.1:9000
```

### Problem: OSSL-Funktionen nicht verfügbar

**Lösung**:

1. Prüfen Sie, dass `osslEnable.ini` aus `osslDefaultEnable.ini` geladen wird
2. Stellen Sie sicher, dass alle OSSL-Berechtigungen konfiguriert sind
3. Die Einstellungen aus `DivaPreferences.ini` [OSSL]-Sektion müssen übernommen werden

### Problem: Scripts laufen mit XEngine statt YEngine

**Lösung**: Überprüfen Sie in `OpenSim.ini`:

```ini
[Startup]
    DefaultScriptEngine = "YEngine"

[YEngine]
    Enabled = true
```

### Problem: Physics funktioniert nicht korrekt

**Lösung**: Überprüfen Sie in `OpenSim.ini`:

```ini
[Startup]
    meshing = Meshmerizer
    physics = BulletSim
```

## ArchitectureProfiles.json

Die Datei `ArchitectureProfiles.json` wurde erweitert mit:

- Engine-Einstellungen für jedes Profil
- `DefaultEngineSettings` Sektion mit vollständiger Dokumentation
- Region-Flags für Grid-Profile

Diese Datei dient als Master-Konfiguration für das 30Configuration-Modul.

## Wichtige Hinweise

1. **Nicht DivaPreferences.ini oder MyWorld.ini direkt einbinden**
   - Alle Einstellungen müssen in die Standard-OpenSim-Konfigurationsdateien übertragen werden
   - Das 30Configuration-Modul übernimmt dies automatisch basierend auf den JSON-Dateien

2. **OSSL-Berechtigungen**
   - Die vollständige Liste der OSSL-Funktionsberechtigungen aus `DivaPreferences.ini` sollte in die generierten Konfigurationsdateien übernommen werden
   - Dies umfasst alle `Allow_*` Einstellungen mit ihren Makros

3. **HTTP-Filter**
   - Passen Sie `OutboundDisallowForUserScriptsExcept` an Ihre IP und Port an
   - Für Grid-Setups verwenden Sie den Robust-Port (8002)
   - Für Standalone-Setups verwenden Sie den Region-Port (9000)

4. **Backup**
   - Erstellen Sie vor der Anwendung einer neuen Konfiguration immer ein Backup
   - Die JSON-Dateien sind Vorlagen und können nach Bedarf angepasst werden

## Versionshinweise

**Erstellt**: 12. November 2025
**Konfiguriert für**:

- BulletSim Physics Engine
- YEngine Script Engine  
- OSSL ThreatLevel High
- HTTP-Filter für localhost freigegeben
- Region-Flags für Grid-Konfigurationen
