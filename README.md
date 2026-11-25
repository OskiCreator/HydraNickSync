# Hydra Nick Sync

A lightweight, zero-background-usage companion utility for **Hydra Launcher** that automatically syncs your Hydra profile nickname (`displayName`) across all your installed Hydra games—replacing the default `Player` name in a single click.

![Platform](https://shields.io)
![Dependencies](https://shields.io)
![Background Usage](https://shields.io)


##  Features

* **Automatic Profile Detection**: Reads your active `displayName` directly from Hydra Launcher's local database (`%APPDATA%\hydralauncher\hydra-db`)—even while Hydra is running.
* **Hydra Library Only**: Scans only games registered in your Hydra Launcher library (automatically ignores imported Steam library games).
* **Safe & Non-Destructive**: Only replaces entries currently set to `Player`. If a game already has a custom nickname configured, it will not be overwritten.
* **Zero Background Overhead**: Runs strictly on-demand when clicked and closes automatically after finishing (`0%` CPU/RAM usage when not in use).
* **Hydra-Inspired UI**: Clean, dark, rounded interface matching Hydra Launcher's aesthetic.


##  Supported Emulators & Config Files

Hydra Nick Sync automatically detects and patches default `Player` entries in:

* **Goldberg Steam Emu**: `steam_settings\account_name.txt`, `steam_settings\configs.user.ini`
* **CODEX / RUNE / FLT**: `steam_emu.ini`, `codex.ini`, `rune.ini`, `flt.ini`
* **Online-Fix**: `OnlineFix.ini`
* **Ubisoft / Uplay R1 & R2**: `upc_r1.ini`, `upc_r2.ini`, `uplay_r1.ini`, `uplay_r2.ini`
* **Other / Legacy**: `valve.ini`, `3dmgame.ini`, `ali213.ini`, `settings.ini`


##  How to Use

1. Download the latest **`HydraNickSync.exe`** from the [**Releases**](../../releases) section.
2. Run `HydraNickSync.exe` whenever you install a new game via Hydra Launcher (you can also pin it to your Windows Taskbar or Start Menu).
3. The app will automatically detect your Hydra nickname, update all `Player` configs in your Hydra games, display a summary, and close itself after ~2.5 seconds.


##  Build from Source

You don't need Visual Studio or external SDKs to compile this project. Windows includes the C# compiler (`csc.exe`) out of the box.

1. Clone or download this repository (`HydraNickSync.cs` and `icon-hydraNC.ico`).
2. Open **PowerShell** in the project folder and run:

```powershell
& "C:\Windows\Microsoft.NET\Framework64\v4.\(0.30319\csc.\)exe" `
  /target:winexe `
  /win32icon:"icon-hydraNC.ico" `
  /out:"HydraNickSync.exe" `
  /reference:System.Windows.Forms.dll `
  /reference:System.Drawing.dll `
  "HydraNickSync.cs"

## Disclaimer

This is an independent community-made tool and is not officially affiliated with or endorsed by the Hydra Launcher development team.
