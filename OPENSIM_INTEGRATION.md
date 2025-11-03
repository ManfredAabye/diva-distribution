# Integration der Diva Distribution in OpenSimulator Core

## Wichtiger Hinweis

Die Diva Distribution muss **innerhalb** des OpenSimulator Core-Projekts kompiliert werden, nicht als separates Projekt.

## Korrekte Integration

### 1. OpenSimulator Repository Setup

```bash
# OpenSimulator Core Repository klonen
git clone https://github.com/opensim/opensim.git opensim-core
cd opensim-core

# Diva Distribution als addon-modules hinzufügen
git submodule add https://github.com/diva/diva-distribution.git addon-modules/diva
# oder
cp -r /pfad/zur/diva-distribution/addon-modules/* addon-modules/
```

### 2. Prebuild-Integration

Das OpenSim Core `prebuild.xml` bereits die Zeile:

```xml
<?include file="addon-modules/*/prebuild*.xml" ?>
```

Diese lädt automatisch alle Module aus `addon-modules/` einschließlich der Diva-Module.

### 3. Build-Prozess

```bash
# Im OpenSim Core Verzeichnis:
./runprebuild.sh          # Linux/macOS
# oder
.\runprebuild.bat         # Windows

# Kompilieren mit VS 2022/2026:
msbuild OpenSim.sln -p:Configuration=Release

# Oder mit .NET CLI:
dotnet build OpenSim.sln -c Release
```

### 4. Visual Studio 2022/2026 Integration

Die `runprebuild.bat` unterstützt jetzt:

- Visual Studio 2026 (bevorzugt)
- Visual Studio 2022
- .NET 8 CLI als Fallback

Das Target ist jetzt `vs2022` statt der veralteten `vs2015`.

### 5. Keine Mono-Abhängigkeit

- ❌ Mono wird nicht mehr verwendet
- ✅ Native .NET 8 Runtime
- ✅ Visual Studio 2022/2026 MSBuild
- ✅ Cross-platform mit .NET CLI

## Verzeichnisstruktur

```bash
opensim-core/
├── OpenSim/              # Core OpenSim Code
├── bin/                  # Compiled Output
├── addon-modules/        # Addon Modules
│   ├── 00Data/          # Diva Data Layer
│   ├── 01DivaInterfaces/# Diva Interfaces
│   ├── 21Wifi/          # Wifi Web Interface
│   └── ...              # Weitere Diva Module
├── prebuild.xml         # Haupt-Prebuild (lädt addon-modules)
└── OpenSim.sln          # Generated Solution
```

## Wichtige Datei-Anpassungen

### runprebuild.bat (Windows)

- Target: `vs2022` statt `vs2015`
- Unterstützung für VS 2022/2026
- .NET CLI Fallback

### runprebuild.sh (Linux/macOS)  

- Target: `vs2022`
- .NET CLI Integration
- Mono nur für Prebuild.exe (falls benötigt)

### Modul prebuild.xml Dateien

- Entfernte `frameworkVersion` Attribute
- Korrekte Pfade zu OpenSim.Framework
- .NET 8 Kompatibilität

## Build-Reihenfolge

1. **OpenSim Core** wird zuerst kompiliert
2. **Addon-Module** werden gegen die Core-Assemblies gelinkt
3. **Alle Module** landen im gemeinsamen `bin/` Verzeichnis
4. **OpenSim.exe** lädt Module zur Laufzeit

## Fazit

Die Diva Distribution ist ein **Addon-Paket** für OpenSimulator, kein eigenständiges Projekt. Sie muss im OpenSim Core integriert und zusammen kompiliert werden.
