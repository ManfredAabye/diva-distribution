# OpenSim Architecture Selection Guide

## Overview

The Configure tool now supports interactive selection of OpenSim architectures at startup. This allows you to configure your OpenSim instance for different deployment scenarios.

## Architecture Types

### 1. Standalone

**Best for:** Private testing, learning, single-user environments

**Characteristics:**

- Single OpenSim instance
- No Hypergrid connectivity
- All services run locally
- Simplest setup
- Cannot connect to other grids

**Configuration Files:**

- `config-include/Standalone.ini`
- `config-include/StandaloneCommon.ini`

---

### 2. StandaloneHypergrid (Recommended for Diva Distribution)

**Best for:** Personal grids that want to connect to the Metaverse

**Characteristics:**

- Single OpenSim instance
- Hypergrid enabled (can teleport to other grids)
- Diva Distribution default
- Can host visitors from other grids
- Can visit other Hypergrid-enabled grids

**Configuration Files:**

- `config-include/DivaPreferences.ini` (Diva Distribution)
- `config-include/StandaloneHypergrid.ini`
- `config-include/MyWorld.ini` (Diva-specific)
- `Wifi.ini` (Web interface)

---

### 3. Grid

**Best for:** Large private grids, organizations, commercial deployments

**Characteristics:**

- Multiple region servers
- Central grid services (Robust)
- No Hypergrid connectivity
- Scalable architecture
- Professional setup

**Configuration Files:**

- `Robust.ini` (Grid services)
- `config-include/Grid.ini`
- `config-include/GridCommon.ini`

---

### 4. GridHypergrid

**Best for:** Large public grids that want Metaverse connectivity

**Characteristics:**

- Multiple region servers
- Central grid services with Hypergrid
- Can connect to the Metaverse
- Most flexible but complex setup
- Requires proper network configuration

**Configuration Files:**

- `Robust.HG.ini` (Grid services with Hypergrid)
- `config-include/GridHypergrid.ini`
- `config-include/GridCommon.ini`
- `Wifi.ini` (Optional web interface)

---

## Interactive Selection

When you run the Configure tool interactively, you will be prompted to select your architecture:

```bash
╔══════════════════════════════════════════════════════════════╗
║           OpenSim Architecture Selection                    ║
╚══════════════════════════════════════════════════════════════╝

Please select your OpenSim architecture:

  [1] Standalone
      → Single instance without Hypergrid connectivity
      → Best for private, local testing

  [2] StandaloneHypergrid (Recommended for Diva Distribution)
      → Single instance with Hypergrid connectivity
      → Can connect to other OpenSim grids via Hypergrid
      → Default for Diva Distribution

  [3] Grid
      → Multi-instance grid setup without Hypergrid
      → Multiple region servers connecting to central grid services
      → Best for large private grids

  [4] GridHypergrid
      → Multi-instance grid with Hypergrid connectivity
      → Large grid that can connect to the Metaverse
      → Most complex but most flexible setup

Your choice [1-4] (default: 2 - StandaloneHypergrid): 
```

## Architecture-Specific Configuration

### What Gets Configured

**All Architectures:**

- Region definitions (`Regions/RegionConfig.ini`)
- Base OpenSim settings (`OpenSim.ini`)

**Standalone:**

- `config-include/Standalone.ini`
- `config-include/StandaloneCommon.ini`

**StandaloneHypergrid (Diva):**

- `config-include/DivaPreferences.ini`
- `config-include/StandaloneHypergrid.ini`
- `config-include/MyWorld.ini`
- `Wifi.ini`

**Grid:**

- `Robust.ini`
- `config-include/Grid.ini`
- `config-include/GridCommon.ini`

**GridHypergrid:**

- `Robust.HG.ini`
- `config-include/GridHypergrid.ini`
- `config-include/GridCommon.ini`
- `Wifi.ini`

## OpenSim.ini Include-Architecture

The tool automatically configures the correct `Include-Architecture` line in `OpenSim.ini`:

```ini
[Startup]
    ; Architecture selection
    Include-Architecture = "config-include/StandaloneHypergrid.ini"
    ; Include-Architecture = "config-include/Standalone.ini"
    ; Include-Architecture = "config-include/Grid.ini"
    ; Include-Architecture = "config-include/GridHypergrid.ini"
```

Only the selected architecture will be uncommented.

## Changing Architecture

To change your architecture after initial setup:

1. Run the Configure tool again: `dotnet Configure.dll`
2. Select a different architecture when prompted
3. The tool will reconfigure all necessary files
4. Automatic backups are created before changes

**Note:** Changing from Standalone to Grid (or vice versa) requires more manual configuration beyond what this tool provides.

## ConfigureSettings.json

Your architecture selection is saved in `ConfigureSettings.json`:

```json
{
  "WorldName": "My World",
  "Architecture": "StandaloneHypergrid",
  "IpAddress": "127.0.0.1",
  ...
}
```

This setting persists across runs and is used as the default for future configurations.

## Tips

### For Beginners

Start with **StandaloneHypergrid** (option 2). It's the easiest to set up and gives you the most flexibility.

### For Testing

Use **Standalone** (option 1) if you don't need Hypergrid and want the simplest possible setup.

### For Production

If you expect to host many regions and users, plan for **Grid** (option 3) or **GridHypergrid** (option 4) from the start.

### Diva Distribution Users

Always use **StandaloneHypergrid** (option 2) - this is what Diva Distribution is optimized for.

## Troubleshooting

### "Configuration file not found"

Some architecture files may not exist in your OpenSim distribution. The tool will skip missing files and warn you.

### Region conflicts

After changing architecture, validate your regions with the built-in validator to ensure no conflicts.

### Database changes

Architecture changes don't modify your database. If switching between architectures that use different database layouts, you may need to migrate data manually.

## See Also

- `README-MODULES.md` - Overall module documentation
- `ConfigurationEnums.cs` - Architecture type definitions
- OpenSim documentation: <http://opensimulator.org/wiki/>
