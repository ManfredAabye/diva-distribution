# OpenSim Configuration Fixes - Implementation Guide

## Overview

This document describes the configuration issues found in OpenSim.log and how they were fixed in the Configure module (30Configuration).

## Issues Identified

### 1. AgentPreferencesService - Missing StorageProvider ❌

**Error Message:**

```bash
ERROR [SERVER UTILS]: Error loading plugin OpenSim.Services.Interfaces.IAgentPreferencesService
Exception: Could not find a storage interface in the given module
ERROR [AGENT PREFERENCES CONNECTOR]: Can't load agent preferences service
```

**Root Cause:**
The `[AgentPreferencesService]` section in `StandaloneCommon.ini` and `StandaloneHypergrid.ini` was missing the `StorageProvider` and `ConnectionString` configuration.

**Solution Implemented:**
Modified `ConfigureStandaloneCommon()` and `ConfigureStandaloneHypergrid()` methods in `Configure.cs` to automatically append:

```ini
[AgentPreferencesService]
    ; Storage provider for agent preferences
    StorageProvider = "OpenSim.Data.SQLite.dll"
    ConnectionString = "URI=file:OpenSim.db,version=3,UseUTF16Encoding=True,Journal Mode=WAL,..."
```

**Code Location:**

- File: `addon-modules/30Configuration/Configure.cs`
- Methods: `ConfigureStandaloneCommon()`, `ConfigureStandaloneHypergrid()`
- Lines: ~1450-1480, ~1510-1540

---

### 2. SQLite Database Lock Issues ⚠️

**Error Message:**

```bash
ERROR [SERVICE BASE]: Failed to load plugin OpenSim.Data.IAgentPreferencesData
inner exception: The database file is locked
```

**Root Cause:**
SQLite connection string was using basic settings that could cause locking issues during concurrent access, especially during OpenSim startup when multiple services try to access the database simultaneously.

**Solution Implemented:**
Enhanced `GetConnectionString()` method to use optimized SQLite parameters:

```csharp
"URI=file:OpenSim.db,version=3,UseUTF16Encoding=True,Journal Mode=WAL,Synchronous=Normal,Cache Size=10000,Page Size=4096,Pooling=True,Max Pool Size=100"
```

**Connection String Parameters Explained:**

- `Journal Mode=WAL` - Write-Ahead Logging allows multiple readers with one writer
- `Synchronous=Normal` - Balance between safety and performance
- `Cache Size=10000` - 10MB cache for better performance
- `Page Size=4096` - Optimized page size for modern systems
- `Pooling=True` - Enable connection pooling
- `Max Pool Size=100` - Allow up to 100 pooled connections

**Benefits:**

- Reduces database lock conflicts
- Improves concurrent access performance
- Faster startup time
- Better handling of multiple services accessing the same database

**Code Location:**

- File: `addon-modules/30Configuration/Configure.cs`
- Method: `GetConnectionString()`
- Lines: ~588-603

---

### 3. Inventory Archiver Errors (Minor) ℹ️

**Error Messages:**

```bash
ERROR [INVENTORY ARCHIVER]: Inventory path Roth2-v1 does not exist
ERROR [INVENTORY ARCHIVER]: Inventory path Roth2-v2 does not exist
ERROR [INVENTORY ARCHIVER]: Inventory path Ruth2-v3 does not exist
ERROR [INVENTORY ARCHIVER]: Inventory path Ruth2-v4 does not exist
```

**Root Cause:**
These are expected during first run when loading avatar meshes. The inventory folders don't exist yet and will be created automatically.

**Action Required:**
None - these are informational errors that occur during initial avatar library loading.

---

## Summary of Fixes

| Issue | Severity | Fixed | Method |
|-------|----------|-------|--------|
| AgentPreferencesService missing StorageProvider | High | ✅ | Auto-add [AgentPreferencesService] section |
| SQLite database locking | Medium | ✅ | Optimized connection string with WAL mode |
| UserProfilesService StorageProvider | High | ✅ | Already fixed (DivaPreferences.ini) |
| Inventory archiver warnings | Low | N/A | Expected behavior |

---

## Testing & Verification

### Before Fix

```bash
2025-11-10 07:53:03,221 ERROR [SERVICE BASE]: Failed to load plugin
2025-11-10 07:53:03,231 ERROR [SERVER UTILS]: Could not find a storage interface
2025-11-10 07:53:03,280 ERROR [AGENT PREFERENCES CONNECTOR]: Can't load agent preferences service
```

### After Fix

- AgentPreferencesService loads successfully
- No "storage interface" errors
- Reduced database lock timeouts
- Faster startup time

---

## How to Apply Fixes

### Option 1: Re-run Configure Tool

```bash
cd D:\OpenSim_Win_Builder_01112025\opensimsource\bin
dotnet Configure.dll
```

Select your architecture and the tool will automatically apply all fixes.

### Option 2: Manual Application

1. **Add to StandaloneCommon.ini:**

```ini
[AgentPreferencesService]
    StorageProvider = "OpenSim.Data.SQLite.dll"
    ConnectionString = "URI=file:OpenSim.db,version=3,UseUTF16Encoding=True,Journal Mode=WAL,Synchronous=Normal,Cache Size=10000,Page Size=4096,Pooling=True,Max Pool Size=100"
```

2 Update all ConnectionString entries in configuration files to use the optimized string above.

---

## Architecture-Specific Notes

### Standalone

- Requires: `StandaloneCommon.ini` with AgentPreferencesService
- Uses: Local SQLite database

### StandaloneHypergrid

- Requires: `StandaloneHypergrid.ini` with AgentPreferencesService
- Uses: Local SQLite database
- Additional: DivaPreferences.ini for Diva-specific services

### Grid/GridHypergrid

- AgentPreferencesService configured in Robust server
- Region servers connect remotely
- No local StorageProvider needed

---

## Future Improvements

1. **Database Performance Monitoring**
   - Add connection pool statistics
   - Monitor lock wait times
   - Alert on excessive database locks

2. **Automatic Database Optimization**
   - Run VACUUM command periodically
   - Analyze and optimize indexes
   - Clean up orphaned records

3. **Configuration Validation**
   - Pre-flight checks before OpenSim starts
   - Verify all required services have StorageProvider
   - Test database connectivity

---

## References

- OpenSim Documentation: <http://opensimulator.org/wiki/>
- SQLite WAL Mode: <https://www.sqlite.org/wal.html>
- OpenSim Services: <http://opensimulator.org/wiki/Configuration>

---

## Changelog

2025-11-10

- Added AgentPreferencesService StorageProvider auto-configuration
- Optimized SQLite connection string with WAL mode
- Added connection pooling for better concurrency
- Enhanced error handling and logging
- Updated documentation

---

## Support

For issues or questions:

- Check OpenSim.log for detailed error messages
- Review this document for known fixes
- Re-run Configure tool to apply latest fixes
- Consult OpenSim community forums
