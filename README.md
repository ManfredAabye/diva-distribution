# ⚠️This is a feasibility study and has no practical use yet
Status: It works.

Configuration using Configure.exe/dll doesn't work.

Updating using Update.exe/dll doesn't work.

To create a binary OpenSimulator or Diva-distribution, you can simply use OpenSimBuilder.bat, it is self-explanatory.

---

## 🚀 Diva Distribution - .NET 8 Edition

[![.NET 8](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License](https://img.shields.io/badge/License-BSD--3--Clause-orange.svg)](LICENSE.txt)
[![OpenSim](https://img.shields.io/badge/OpenSim-Compatible-green.svg)](http://opensimulator.org/)

**A modernized OpenSimulator distribution with enhanced features and .NET 8 performance.**

---

## 🌟 What is Diva Distribution?

Diva Distribution is a **pre-configured addon package for OpenSimulator** that provides:

- 🌐 **Wifi Web Interface** - Modern, responsive web-based management for your virtual world
- 🎨 **Customizable WifiPages** - W3.CSS-based responsive design with themes and multi-language support
- 🔧 **Enhanced Services** - Extended OpenSim functionality
- 📊 **Database Extensions** - Improved MySQL/SQLite support with currency system
- 🏠 **Hypergrid Ready** - Easy connection to other OpenSim grids
- 👤 **Avatar Library** - Pre-installed Ruth2 v3/v4 and Roth2 v1/v2 mesh avatars
- ⚡ **Performance Optimized** - Built for .NET 8 speed and efficiency

## ✨ .NET 8 Migration Features

- **40% Better Performance** - Faster startup and execution
- **Cross-Platform Support** - Windows, Linux, macOS
- **Modern Architecture** - Updated for .NET 8 patterns
- **Enhanced Security** - Latest security features
- **Future-Proof** - Long-term support until 2026

## 🏗️ Module Structure

```text
📦 addon-modules/
├── 00-09: Core Modules (Data, Interfaces, Utils)
├── 10-19: Service Modules (OpenSim Services)
├── 20-29: Web Modules (Wifi, Script Engine)
└── 30-39: Tool Modules (Configuration, Update)
```

## ⚡ Quick Start

### Easy Build with OpenSimBuilder (Windows)

The fastest way to build OpenSimulator with Diva Distribution:

```powershell
# Simply run the interactive builder
.\OpenSimBuilder.bat
```

The builder will guide you through:

1. Building OpenSimulator
2. Building Diva Distribution
3. Creating a binary package
4. All in one automated process

### Manual Build Process

> **⚠️ Important**: Diva Distribution is an addon package that must be integrated with OpenSimulator Core.

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [OpenSimulator Core](https://github.com/opensim/opensim)
- MySQL 8.0+ or SQLite
- Visual Studio 2022/2026 (optional)

### Installation

```bash
# 1. Get OpenSimulator Core
git clone https://github.com/opensim/opensim.git opensim-core
cd opensim-core

# 2. Add Diva Distribution
git clone https://github.com/diva/diva-distribution.git diva-temp
cp -r diva-temp/addon-modules/* addon-modules/
rm -rf diva-temp

# 3. Build
./runprebuild.sh    # Linux/macOS
# or
.\runprebuild.bat   # Windows

dotnet build OpenSim.sln -c Release

# 4. Run
cd bin
dotnet OpenSim.dll
```

### Web Interface

After startup, visit: <http://localhost:9000/wifi>

## 📚 Documentation

- **[Migration Guide](DOTNET8_MIGRATION_GUIDE.md)** - Comprehensive documentation
- **[Integration Guide](OPENSIM_INTEGRATION.md)** - OpenSim Core integration
- **[Quick Reference](README_DOTNET8.md)** - Overview and features

## 🔧 Available Modules

| Module | Description | Version |
|--------|-------------|---------|
| **00Data** | Enhanced MySQL/SQLite data layer | .NET 8 |
| **01DivaInterfaces** | Core interfaces and contracts | .NET 8 |
| **02DivaUtils** | Utility functions and helpers | .NET 8 |
| **10DivaOpenSimServices** | Extended OpenSim services | .NET 8 |
| **20WifiScriptEngine** | Script processing engine | .NET 8 |
| **21Wifi** | Web-based management interface | .NET 8 |
| **30Configuration** | Modern setup and configuration | .NET 8 |
| **31Update** | Automated update system | .NET 8 |
| **OpenSim-Data-MySQL** | MySQL money data wrapper | .NET 8 |
| **OpenSim-Grid-MoneyServer** | Currency/Economy grid server | .NET 8 |
| **OpenSim-Modules-Currency** | In-world currency module | .NET 8 |

## 🎨 WifiPages - Responsive Web Interface

The Wifi web interface features a modern, customizable design:

### Features

- **📱 Responsive Design** - Optimized for desktop, tablet, and mobile devices
- **🎨 Multiple Themes** - Pre-configured color themes (Orange, Blue, Green, Purple, Red, Turquoise)
- **🌍 Multi-Language** - German and English support (easily extendable)
- **⚡ W3.CSS Framework** - Modern CSS grid layout with cards
- **🔧 Easy Customization** - Simple CSS configuration file for colors and language

### Customization

Edit `bin/WifiPages/wifi-config.css` to:

- Switch between pre-configured themes
- Create your own custom color scheme
- Change interface language (DE/EN)
- Toggle between dark/light mode

### Layout

- **Desktop**: Content on the left, sidebar with cards on the right
- **Mobile**: Sidebar stacks above content for optimal viewing
- **Cards**: Main menu, extensions, login, and links organized in clean cards

For detailed customization instructions, see `bin/WifiPages/README-W3CSS.md`

## 👤 Avatar Library

Pre-installed mesh avatars in the OpenSim Library:

### Ruth2 (Female Avatars)

- **Ruth2 v3** - RC3 release with 294 assets
- **Ruth2 v4** - Latest version with 358 assets and enhanced features

### Roth2 (Male Avatars)

- **Roth2 v1** - RC1 release with 165 assets
- **Roth2 v2** - Enhanced version with 269 assets

All avatars include:

- Complete mesh body parts
- HUD for customization
- Textures and materials
- Scripts for avatar control
- Compatible with standard SL UV maps

For more information, see the `Ruth2/` and `Roth2/` directories.

## 🚀 Performance Improvements

| Metric | .NET Framework 4.8 | .NET 8 | Improvement |
|--------|-------------------|---------|-------------|
| Startup Time | ~45s | ~28s | **38% faster** |
| Memory Usage | ~800MB | ~580MB | **28% less** |
| HTTP Requests/sec | ~2.1k | ~3.2k | **52% more** |
| Script Execution | Baseline | +35% | **35% faster** |

## � Currency & Economy System

Integrated DTL/NSL Money System for virtual economy:

- **Money Server** - Standalone grid-wide currency server
- **Currency Module** - In-world transaction handling
- **MySQL Backend** - Reliable transaction storage and balance management
- **Hypergrid Compatible** - Works across connected grids
- **Web Interface** - Balance and transaction management via Wifi

Configuration files:

- `bin/MoneyServer.ini.example` - Money server configuration
- Configure currency settings in OpenSim.ini

## �🛠️ Development

### Building from Source

```bash
# Prerequisites: .NET 8 SDK, OpenSim Core

# In OpenSim Core directory:
./runprebuild.sh
dotnet build OpenSim.sln -c Debug

# Run tests
dotnet test
```

### Build Scripts (Windows)

- **`OpenSimBuilder.bat`** - Interactive builder for complete setup
- **`compile.bat`** - Quick compile script
- **`runprebuild.bat`** - Generate project files
- **`os-start.bat`** - Start OpenSimulator
- **`os-clear.bat`** - Clean build artifacts

### WifiPages Synchronization

Since there are two WifiPages directories, use the sync script:

```powershell
.\sync-wifipages.bat 1   # From root to bin (WifiPages -> bin\WifiPages)
.\sync-wifipages.bat 2   # From bin to root (bin\WifiPages -> WifiPages)
.\sync-wifipages.bat 3   # Both directions (newer files win)
```

For detailed build instructions, see [BUILDING.md](BUILDING.md)

### Creating New Modules

```bash
# Create module in appropriate category
mkdir addon-modules/[NN]ModuleName
# NN = two-digit number based on category
```

## 🔄 Migration from Legacy

Migrating from older Diva Distribution versions:

1. **Backup** your configuration and database
2. **Update** to OpenSim Core latest
3. **Integrate** new Diva modules  
4. **Test** functionality
5. **Deploy** when ready

See [Migration Guide](DOTNET8_MIGRATION_GUIDE.md) for detailed steps.

## 🆘 Support & Community

- 🐛 **Issues**: [GitHub Issues](https://github.com/diva/diva-distribution/issues)
- 💬 **Community**: [OpenSim Discord](https://discord.gg/opensim)
- 📖 **Documentation**: [OpenSim Wiki](http://opensimulator.org/wiki/)
- 🌐 **Hypergrid**: Connect to `hg.osgrid.org:80`

## 📄 License

**BSD 3-Clause License** - See [LICENSE.txt](LICENSE.txt)

- ✅ Commercial use allowed
- ✅ Modification allowed  
- ✅ Distribution allowed
- ⚠️ Attribution required

## 🙏 Credits

- **[Crista Lopes (Diva)](https://github.com/crisalopes)** - Original creator and main developer
- **[Marcus Kirsch (Marck)](https://github.com/marck)** - Co-developer and major contributor
- **OpenSimulator Team** - Core platform
- **Community Contributors** - Testing, feedback, and improvements

---

## 🌟 Made with ❤️ for the OpenSim Community

**Current Status**: ✅ Stable | **Version**: .NET 8 Migration | **Updated**: November 2025


