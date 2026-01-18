# GhostMod

Ghost Racing mod for Initial Drift Online. Race against your own best times or challenge other players' ghost recordings.

## Features

- **Record Ghosts** - Automatically saves your best runs on any route
- **Race Your Ghost** - Challenge your personal best times
- **Share Ghosts** - Export and import ghost files with friends
- **Ghost Library** - Manage all your ghost recordings
- **Visual Customization** - Customize ghost car appearance

## Installation

### Prerequisites

1. [BepInEx 5.x](https://github.com/BepInEx/BepInEx/releases) installed in your game folder
2. Game must be run once after BepInEx installation

### Install the Mod

1. Download `GhostMod.dll` from [Releases](https://github.com/stashya/GhostMod/releases)
2. Copy to `[Game Folder]/BepInEx/plugins/GhostMod/GhostMod.dll`
3. Launch the game

## Usage

- Press **G** to open the Ghost Menu
- Select a route and start racing
- Your best times are automatically recorded
- Import/export ghosts to race against friends

## Building from Source

### Requirements

- [.NET SDK](https://dotnet.microsoft.com/download) (any recent version)
- Initial Drift Online installed via Steam
- BepInEx installed in game folder

### Build

Just run:

```
build.bat
```

This builds the mod and installs it to your game automatically.

### Custom Game Path

If your game isn't in the default Steam location, edit `GhostMod.csproj` and change the `GamePath`:

```xml
<GamePath>D:\Games\Initial Drift Online</GamePath>
```

## For Developers

### Project Structure

```
GhostMod/
├── src/
│   ├── Plugin.cs              # BepInEx plugin entry point
│   ├── GhostRacingManager.cs  # Core ghost racing logic
│   ├── Models/                # Data structures
│   ├── Services/              # Ghost file & car services
│   ├── Components/            # Unity components
│   └── UI/                    # Menu and HUD
├── GhostMod.csproj            # Project file
└── build.bat                  # Build script
```


## License

MIT License - See [LICENSE](LICENSE) file

## Credits

- Built for [Initial Drift Online](https://store.steampowered.com/app/1339860/Initial_Drift_Online/)
- Uses [BepInEx](https://github.com/BepInEx/BepInEx) modding framework
- Inspired by [WarmTofuMod](https://github.com/Kert/WarmTofuMod)
