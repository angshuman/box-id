# 🏷️ Machine Label

A Windows taskbar label that shows a persistent, colored text badge directly on your taskbar — so you always know which machine you're on.

![.NET 8](https://img.shields.io/badge/.NET-8.0-blue)
![Windows](https://img.shields.io/badge/Platform-Windows-blue)

## Why?

When you're RDP'd into multiple machines, jump boxes, or dev/staging/prod servers, it's easy to lose track of *where* you are. Machine Label puts a bright, impossible-to-miss label right on your taskbar.

## Features

- 📌 **Always visible** — sits directly on the taskbar, not in the system tray
- 🎨 **Color coded** — pick any background/text color to distinguish environments
- 😀 **Emoji support** — use emoji for instant visual recognition (🔴 PROD, 🟢 DEV, etc.)
- 🖱️ **Draggable** — reposition the label anywhere along the taskbar
- ⚡ **Quick presets** — one-click setups for PROD, STAGING, DEV, TEST
- 📋 **Copy machine name** — right-click to copy the hostname
- 🚀 **Start with Windows** — optional auto-start via registry
- 🎛️ **Customizable** — font size, opacity, corner radius, bold, padding
- 🪶 **Lightweight** — single-instance WPF app, minimal resource usage

## Quick Start

```bash
# Build
dotnet build

# Run
dotnet run

# Or publish a single-file exe
dotnet publish -c Release
```

## Usage

1. **Run** the app — a label appears on your taskbar
2. **Right-click** the label to access settings or exit
3. **Double-click** the label to open settings
4. **Drag** the label left/right to reposition it on the taskbar
5. Configure text, colors, and style in the settings window

## Presets

| Preset  | Label                    | Color   |
|---------|--------------------------|---------|
| Default | 🖥️ HOSTNAME             | Orange  |
| PROD    | 🔴 PROD - HOSTNAME      | Red     |
| STAGING | 🟡 STAGING - HOSTNAME   | Yellow  |
| DEV     | 🟢 DEV - HOSTNAME       | Green   |
| TEST    | 🔵 TEST - HOSTNAME      | Blue    |

## Requirements

- Windows 10/11
- [.NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)

## Configuration

Settings are stored in `%APPDATA%\MachineLabel\settings.json`.
