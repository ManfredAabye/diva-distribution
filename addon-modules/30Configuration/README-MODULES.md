# OpenSim Configuration Tool - Modular Structure

## Overview

The Configure tool has been refactored into a modular structure for better maintainability and extensibility.

## File Structure

### Core Files

- **Configure.cs** - Main entry point and orchestration
- **ConfigurationEnums.cs** - Enumerations (ArchitectureType, RegionConfigStatus)
- **ConfigurationSettings.cs** - Settings data structures
- **BackupManager.cs** - Backup and restore functionality

### Architecture Support

The tool now supports multiple OpenSim architectures:

1. **Standalone** - Single instance without Hypergrid
2. **StandaloneHypergrid** - Single instance with Hypergrid (default for Diva Distribution)
3. **Grid** - Multi-instance grid without Hypergrid
4. **GridHypergrid** - Multi-instance grid with Hypergrid

## Features

### Automatic Configuration

- Detects and fixes common configuration issues
- Validates region configurations for conflicts
- Supports multiple database types (SQLite, MySQL)

### Region Validation

- Checks for duplicate locations
- Validates port assignments
- Detects region overlaps based on size
- 512m regions require 2 grid positions spacing

### Backup Management

- Automatic backups before configuration changes
- Compressed ZIP archives
- Individual timestamped .bak files
- Keeps last 10 backups
- Easy restore functionality

### Key Fixes Implemented

1. **UserProfilesService StorageProvider** - Automatically uses OpenSim.Data.SQLite.dll instead of Diva.Data.SQLite.dll
2. **AgentPreferencesService StorageProvider** - Automatically adds StorageProvider configuration to StandaloneCommon.ini and StandaloneHypergrid.ini to prevent "Could not find a storage interface" error
3. **Region Overlap Detection** - Validates that regions don't overlap based on their size
4. **Default Values** - SQLite database and 512m region sizes as defaults

## Usage

```bash
# Interactive mode
dotnet Configure.dll

# Auto-configure with saved settings
dotnet Configure.dll --auto

# Validate configuration
dotnet Configure.dll --validate

# Create backup
dotnet Configure.dll --backup

# Restore from backup
dotnet Configure.dll --restore
```

## Configuration Settings

Settings are stored in `ConfigureSettings.json`:

```json
{
  "WorldName": "My World",
  "DbType": "SQLite",
  "Architecture": "StandaloneHypergrid",
  "IpAddress": "127.0.0.1",
  "HttpPort": 9000,
  "BaseLocationX": 1000,
  "BaseLocationY": 1000,
  "RegionSizeX": 512,
  "RegionSizeY": 512,
  "RegionSizeZ": 512,
  "AutoBackup": true
}
```

## Development

### Adding New Modules

1. Create new .cs file in the same directory
2. Add to Configure.csproj ItemGroup
3. Use namespace `MetaverseInk.Configuration`
4. Follow existing patterns for BackupManager

### Future Enhancements

- FileConfigurer class for configuration file management
- RegionValidator class for advanced region validation
- Grid architecture support
- Enhanced UI/UX

## Notes

- Region size 512m = 2 grid positions (256m per position)
- Multiple regions must have unique: Location, InternalPort, RegionUUID
- StandaloneHypergrid is recommended for Diva Distribution
