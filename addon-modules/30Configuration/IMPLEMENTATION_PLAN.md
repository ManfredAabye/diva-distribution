# OpenSim Template System - Implementierungsplan

## Status: Templates Komplett ✅

Alle 8 JSON-Templates wurden erfolgreich erstellt:

### Erstellte Template-Dateien

1. ✅ **ConfigurationTemplates.json** (690 Bytes)
   - Master-Definition der 4 Architektur-Modi
   - Standalone, StandaloneHypergrid, Grid, GridHypergrid
   - Enthält RequiredFiles, RegionCount, Flags

2. ✅ **OpenSimIniTemplate.json** (2,8 KB)
   - Hauptkonfiguration für OpenSim.exe
   - 11 Sektionen: Const, Startup, Network, AccessControl, Map, Permissions, EstateManagement, SMTP, Architecture, RemoteAdmin, DataSnapshot

3. ✅ **RobustIniTemplate.json** (5,1 KB)
   - Robust-Server Konfiguration (Grid-Modus)
   - 20+ Service-Sektionen
   - DatabaseService, AssetService, InventoryService, GridService, LoginService, HypergridServices

4. ✅ **StandaloneCommonIniTemplate.json** (4,7 KB)
   - Common Services für Standalone-Modus
   - Alle lokalen Service-Connector
   - UserProfiles, Groups, Messaging, BakedTexture, MuteList

5. ✅ **StandaloneHypergridIniTemplate.json** (2,9 KB)
   - Hypergrid-spezifische Services
   - HGEntityTransferModule, HGInventoryAccessModule, HGAssetService
   - GatekeeperService, UserAgentService, HGFriendsModule

6. ✅ **SQLiteStandaloneIniTemplate.json** (1,3 KB)
   - SQLite-Datenbank Konfiguration
   - Separate .db-Dateien für jeden Service
   - 12 verschiedene Datenbanken

7. ✅ **WifiIniTemplate.json** (2,2 KB)
   - Diva Wifi Web-Interface
   - DatabaseService mit Diva.Data.SQLite.dll
   - WifiService mit allen Service-Referenzen
   - WifiApp mit 30+ Einstellungen

8. ✅ **osslEnableIniTemplate.json** (6,8 KB)
   - OSSL Script-Permissions
   - 100+ Allow_os* Einstellungen
   - XEngine und YEngine Konfiguration
   - Permission-Levels: true, PARCEL_OWNER, ESTATE_MANAGER, ESTATE_OWNER

9. ✅ **RegionConfigTemplate.json** (1,5 KB)
   - Region-Definitionen
   - DefaultRegion, FallbackRegion, DefaultHGRegion
   - UUID-Generierung, Locations, Ports

## Nächste Implementierungs-Schritte

### Phase 1: Template-Processing-Engine (Priorität: HOCH)

Implementiere in `Configure.cs`:

```csharp
// 1. JSON-Template-Loader
public static Dictionary<string, object> LoadTemplate(string templateName)
{
    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, 
        "../addon-modules/30Configuration/Templates", templateName);
    string json = File.ReadAllText(path);
    return JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
}

// 2. Variablen-Substitution
public static string SubstituteVariables(string text, Dictionary<string, string> variables)
{
    foreach (var kvp in variables)
    {
        text = text.Replace($"${{{kvp.Key}}}", kvp.Value);
    }
    return text;
}

// 3. INI-Generator (OHNE Kommentare)
public static void GenerateIniFromTemplate(
    string templateName, 
    string outputPath, 
    Dictionary<string, string> variables)
{
    var template = LoadTemplate(templateName);
    var sections = template["Sections"] as Dictionary<string, object>;
    
    using (StreamWriter writer = new StreamWriter(outputPath))
    {
        foreach (var section in sections)
        {
            writer.WriteLine($"[{section.Key}]");
            
            var keys = section.Value as Dictionary<string, string>;
            foreach (var key in keys)
            {
                string value = SubstituteVariables(key.Value, variables);
                writer.WriteLine($"{key.Key} = {value}");
            }
            
            writer.WriteLine(); // Leerzeile zwischen Sektionen
        }
    }
}

// 4. UUID-Generator
public static string GenerateNewUUID()
{
    return Guid.NewGuid().ToString();
}

// 5. Region-Config-Generator
public static void GenerateRegionConfig(
    string regionName,
    string templateName, // "DefaultRegion", "FallbackRegion", "DefaultHGRegion"
    Dictionary<string, string> variables)
{
    var template = LoadTemplate("RegionConfigTemplate.json");
    var regionTemplate = template[templateName] as Dictionary<string, object>;
    
    string outputPath = Path.Combine("Regions", $"{regionName}.ini");
    
    using (StreamWriter writer = new StreamWriter(outputPath))
    {
        foreach (var kvp in regionTemplate)
        {
            string value = kvp.Value.ToString();
            
            // UUID-Generierung
            if (value == "GENERATE_NEW_UUID")
            {
                value = GenerateNewUUID();
            }
            // Location-Berechnung (z.B. ${BaseLocationX+1})
            else if (value.Contains("+"))
            {
                value = CalculateLocationExpression(value, variables);
            }
            else
            {
                value = SubstituteVariables(value, variables);
            }
            
            writer.WriteLine($"{kvp.Key} = {value}");
        }
    }
}
```

### Phase 2: Architektur-Auswahl-Logik (Priorität: HOCH)

```csharp
public static void ConfigureByArchitecture(string architecture)
{
    // Lade ConfigurationTemplates.json
    var templates = LoadTemplate("ConfigurationTemplates.json");
    var archDef = templates["Architectures"][architecture];
    
    var requiredFiles = archDef["RequiredFiles"] as List<string>;
    var regionCount = (int)archDef["RegionCount"];
    var requiresRobust = (bool)archDef["RequiresRobust"];
    var requiresHypergrid = (bool)archDef["RequiresHypergrid"];
    
    // Sammle Variablen vom Benutzer
    var variables = CollectUserVariables();
    
    // Generiere alle erforderlichen Dateien
    foreach (string file in requiredFiles)
    {
        string templateName = file.Replace(".ini", "IniTemplate.json");
        string outputPath = Path.Combine("bin", file);
        
        GenerateIniFromTemplate(templateName, outputPath, variables);
    }
    
    // Generiere Region-Configs
    GenerateRegionConfigs(architecture, regionCount, variables);
}
```

### Phase 3: Variablen-Sammlung (Priorität: MITTEL)

```csharp
public static Dictionary<string, string> CollectUserVariables()
{
    var vars = new Dictionary<string, string>();
    
    // Netzwerk
    vars["IpAddress"] = PromptUser("IP-Adresse", "127.0.0.1");
    vars["HttpPort"] = PromptUser("HTTP Port", "9000");
    vars["PublicPort"] = vars["HttpPort"];
    
    // Grid
    vars["GridName"] = PromptUser("Grid Name", "My OpenSim");
    vars["WorldName"] = PromptUser("Region Name", "Welcome");
    vars["WelcomeMessage"] = PromptUser("Welcome Message", "Welcome to OpenSim!");
    
    // Region
    vars["BaseLocationX"] = PromptUser("Base Location X", "1000");
    vars["BaseLocationY"] = PromptUser("Base Location Y", "1000");
    vars["RegionSizeX"] = PromptUser("Region Size X", "512");
    vars["RegionSizeY"] = PromptUser("Region Size Y", "512");
    vars["RegionSizeZ"] = PromptUser("Region Size Z", "4096");
    
    // Admin
    vars["AdminFirst"] = PromptUser("Admin First Name", "Admin");
    vars["AdminLast"] = PromptUser("Admin Last Name", "User");
    vars["AdminEmail"] = PromptUser("Admin Email", "admin@localhost");
    
    // SQLite-Datenbank-Variablen
    SetSQLiteVariables(vars);
    
    // Sicherheit
    vars["ForgotPasswordHashSecret"] = GenerateRandomSecret();
    
    return vars;
}

public static void SetSQLiteVariables(Dictionary<string, string> vars)
{
    vars["AssetDll"] = "OpenSim.Data.SQLite.dll";
    vars["AssetConnectionString"] = "URI=file:Asset.db,version=3";
    vars["InventoryDll"] = "OpenSim.Data.SQLite.dll";
    vars["InventoryConnectionString"] = "URI=file:inventory.db,version=3";
    vars["GridDll"] = "OpenSim.Data.SQLite.dll";
    vars["GridConnectionString"] = "URI=file:regions.db,version=3";
    // ... alle anderen DB-Variablen
    vars["WifiDll"] = "Diva.Data.SQLite.dll";
    vars["WifiConnectionString"] = "Data Source=WifiData.db;Version=3;";
}
```

### Phase 4: Region-Config-Generierung (Priorität: MITTEL)

```csharp
public static void GenerateRegionConfigs(
    string architecture, 
    int regionCount, 
    Dictionary<string, string> variables)
{
    switch (architecture)
    {
        case "Standalone":
        case "StandaloneHypergrid":
            GenerateRegionConfig("DefaultRegion", "DefaultRegion", variables);
            break;
            
        case "Grid":
            GenerateRegionConfig("DefaultRegion", "DefaultRegion", variables);
            GenerateRegionConfig("FallbackRegion", "FallbackRegion", variables);
            break;
            
        case "GridHypergrid":
            GenerateRegionConfig("DefaultRegion", "DefaultRegion", variables);
            GenerateRegionConfig("DefaultHGRegion", "DefaultHGRegion", variables);
            GenerateRegionConfig("FallbackRegion", "FallbackRegion", variables);
            break;
    }
}
```

### Phase 5: Location-Berechnung (Priorität: NIEDRIG)

```csharp
public static string CalculateLocationExpression(
    string expression, 
    Dictionary<string, string> variables)
{
    // Beispiel: "${BaseLocationX+1}" -> "1001"
    var match = Regex.Match(expression, @"\$\{(\w+)([\+\-])(\d+)\}");
    if (match.Success)
    {
        string varName = match.Groups[1].Value;
        string op = match.Groups[2].Value;
        int offset = int.Parse(match.Groups[3].Value);
        int baseValue = int.Parse(variables[varName]);
        
        int result = op == "+" ? baseValue + offset : baseValue - offset;
        return result.ToString();
    }
    
    return SubstituteVariables(expression, variables);
}
```

## Test-Plan

### Test 1: Standalone-Architektur

```powershell
cd bin
.\Configure.exe

# Auswahl: Standalone
# Eingaben:
#   IP: 127.0.0.1
#   Port: 9000
#   Grid: TestGrid
#   Region: Welcome
#   Admin: Test User

# Erwartete Dateien:
# - OpenSim.ini
# - config-include/StandaloneCommon.ini
# - config-include/storage/SQLiteStandalone.ini
# - config-include/osslEnable.ini
# - Regions/DefaultRegion.ini
```

### Test 2: StandaloneHypergrid-Architektur

```powershell
# Auswahl: StandaloneHypergrid
# Zusätzliche Dateien:
# - config-include/StandaloneHypergrid.ini
# - Wifi.ini
```

### Test 3: Grid-Architektur

```powershell
# Auswahl: Grid
# Erwartete Dateien:
# - OpenSim.ini
# - Robust.ini
# - config-include/GridCommon.ini
# - config-include/osslEnable.ini
# - Regions/DefaultRegion.ini
# - Regions/FallbackRegion.ini
```

### Test 4: GridHypergrid-Architektur

```powershell
# Auswahl: GridHypergrid
# Erwartete Dateien:
# - OpenSim.ini
# - Robust.HG.ini
# - config-include/GridCommon.ini
# - config-include/osslEnable.ini
# - Wifi.ini
# - Regions/DefaultRegion.ini
# - Regions/DefaultHGRegion.ini
# - Regions/FallbackRegion.ini
```

## Dependencies

### NuGet-Pakete (bereits vorhanden)

- Newtonsoft.Json (für JSON-Parsing)

### System-Namespaces

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
```

## Zeitschätzung

- **Phase 1** (Template-Processing): 4-6 Stunden
- **Phase 2** (Architektur-Logik): 2-3 Stunden
- **Phase 3** (Variablen-Sammlung): 2-3 Stunden
- **Phase 4** (Region-Configs): 2-3 Stunden
- **Phase 5** (Location-Berechnung): 1-2 Stunden
- **Testing**: 3-4 Stunden

**Gesamt**: 14-21 Stunden

## Prioritäten

1. **SOFORT**: Phase 1 (Template-Processing-Engine)
2. **HOCH**: Phase 2 (Architektur-Auswahl)
3. **MITTEL**: Phase 3 (Variablen-Sammlung)
4. **MITTEL**: Phase 4 (Region-Configs)
5. **NIEDRIG**: Phase 5 (Advanced Features)

## Erfolgs-Kriterien

✅ Alle 4 Architektur-Modi funktionieren
✅ Generierte .ini-Dateien haben KEINE Kommentare
✅ Alle Variablen korrekt substituiert
✅ Region-UUIDs werden automatisch generiert
✅ OpenSim startet ohne Fehler
✅ Wifi-Webinterface erreichbar
✅ Profile und Gruppen funktionieren
✅ Hypergrid-Verbindungen funktionieren (bei HG-Modi)

## Nächster Schritt

**Beginne mit Phase 1**: Template-Processing-Engine in Configure.cs implementieren

Die Templates sind fertig und vollständig dokumentiert in `README_TEMPLATES.md`.
