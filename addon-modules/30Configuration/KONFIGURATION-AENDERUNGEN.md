# Zusammenfassung der JSON-Konfigurationsänderungen

## Datum: 12. November 2025

### Geänderte Dateien

1. **StandaloneSettings.json**
2. **StandaloneHypergridSettings.json**
3. **GridSettings.json**
4. **GridHypergridSettings.json**
5. **ArchitectureProfiles.json**
6. **CONFIGURATION-ENGINE-SETTINGS.md** (neu erstellt)

### Hinzugefügte Konfigurationsabschnitte

Alle JSON-Dateien wurden um folgende Abschnitte erweitert:

#### PhysicsEngine

- Engine: BulletSim
- Meshing: Meshmerizer  
- DisableUbODE: true

#### ScriptEngine

- DefaultEngine: YEngine
- YEngineEnabled: true
- XEngineEnabled: false
- MinTimerInterval: 0.05
- ScriptDistanceLimitFactor: 20
- DeleteScriptsOnStartup: false

#### OSSL

- Enabled: true
- AllowOSFunctions: true
- AllowMODFunctions: true
- AllowLightShareFunctions: true
- OSFunctionThreatLevel: High
- PermissionErrorToOwner: false

#### Network

- OutboundDisallowForUserScriptsExcept: (angepasst pro Architektur)
- HttpBodyMaxLenMAX: 16384
- ExternalHostNameForLSL: 127.0.0.1

#### Zusätzliche Abschnitte

**GridSettings.json & GridHypergridSettings.json:**

- GridService.DefaultRegionFlags
- Robust.Enabled
- Robust.Port

**StandaloneHypergridSettings.json & GridHypergridSettings.json:**

- Hypergrid.Enabled
- Hypergrid.WorldMapModule

### Region-Flags

- **Grid**: "DefaultRegion, FallbackRegion"
- **GridHypergrid**: "DefaultRegion, DefaultHGRegion, FallbackRegion"

### Gelöste Probleme

1. ✅ osslEnable.ini Konfiguration aus DivaPreferences.ini
   - OSSL-Einstellungen jetzt in JSON-Vorlagen

2. ✅ llHttpRequest Filter für localhost
   - `OutboundDisallowForUserScriptsExcept` konfiguriert

3. ✅ BulletSim aktiviert, ubODE deaktiviert
   - PhysicsEngine-Sektion in allen Profilen

4. ✅ YEngine aktiviert, XEngine deaktiviert
   - ScriptEngine-Sektion in allen Profilen

### ArchitectureProfiles.json Erweiterungen

Neue Sektion `DefaultEngineSettings` mit:

- Vollständigen Engine-Einstellungen
- Kommentaren zur Verwendung
- Standardwerten für alle Architekturen

Jedes Profil enthält jetzt:

- PhysicsEngine-Einstellungen
- ScriptEngine-Einstellungen
- GridService-Flags (wo zutreffend)

### Nächste Schritte für 30Configuration Modul

Das Modul sollte folgende Einstellungen aus den JSON-Dateien in die INI-Dateien schreiben:

1. **In OpenSim.ini [Startup]:**

   ```ini
   meshing = Meshmerizer
   physics = BulletSim
   DefaultScriptEngine = "YEngine"
   ```

2. **In OpenSim.ini [Network]:**

   ```ini
   OutboundDisallowForUserScriptsExcept = 127.0.0.1:9000
   HttpBodyMaxLenMAX = 16384
   ExternalHostNameForLSL = 127.0.0.1
   ```

3. **In OpenSim.ini [YEngine]:**

   ```ini
   Enabled = true
   MinTimerInterval = 0.05
   ScriptDistanceLimitFactor = 20
   DeleteScriptsOnStartup = false
   ```

4. **In config-include/osslEnable.ini oder OpenSim.ini [OSSL]:**

   ```ini
   AllowOSFunctions = true
   AllowMODFunctions = true
   AllowLightShareFunctions = true
   OSFunctionThreatLevel = High
   PermissionErrorToOwner = false
   ```

   Plus alle `Allow_*` Berechtigungen aus DivaPreferences.ini

5. **Für Grid-Architekturen in GridService:**
   - Region-Flags setzen basierend auf `GridService.DefaultRegionFlags`

### Wichtige Hinweise

- **Keine direkte Einbindung** von DivaPreferences.ini oder MyWorld.ini
- Alle Einstellungen werden durch das 30Configuration-Modul aus den JSON-Dateien generiert
- Die JSON-Dateien sind Vorlagen und können angepasst werden
- OSSL-Funktionsberechtigungen müssen vollständig aus DivaPreferences.ini übernommen werden

### Dateipfade

Alle Dateien befinden sich in:
`addon-modules/30Configuration/`

- StandaloneSettings.json
- StandaloneHypergridSettings.json
- GridSettings.json
- GridHypergridSettings.json
- ArchitectureProfiles.json
- CONFIGURATION-ENGINE-SETTINGS.md (Dokumentation)
- KONFIGURATION-AENDERUNGEN.md (diese Datei)
